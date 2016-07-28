// <copyright file="ShutdownDetector.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Hangfire.Logging;

namespace Hangfire.AspNet
{
    internal sealed class ShutdownDetector : IRegisteredObject, IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<ShutdownDetector>();
        private static readonly TimeSpan CheckForShutdownTimerInterval = TimeSpan.FromSeconds(10);

        private readonly CancellationTokenSource _cts;
        private IDisposable _checkForShutdownTimer;

        public ShutdownDetector()
        {
            _cts = new CancellationTokenSource();
        }

        internal CancellationToken Token => _cts.Token;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Initialize must not throw")]
        internal void Initialize()
        {
            try
            {
                HostingEnvironment.RegisterObject(this);
                
                // Normally when the AppDomain shuts down, IRegisteredObject.Stop method gets called,
                // except when ASP.NET waits for requests to end before calling IRegisteredObject.Stop.
                // Pending requests may prevent background processing from stopping, leaving processing
                // servers as zombies, because another instance was started, and we aren't expecting that
                // an old one will continue to process background jobs.
                // So we are using more aggressive checks, in addition to IRegisteredObject.Stop method,
                // to be able to shutdown the processing regardless of any pending HTTP requests. In fact,
                // any signal that AppDomain is going to shutdown should trigger the processing to stop.
                if (HttpRuntime.UsingIntegratedPipeline)
                {
                    // StopListening event is triggered by IIS, to signal that no new request should
                    // be served by this application instance anymore. We should stop the background
                    // processing also.
                    RegisterForStopListeningEvent();

                    // When AppDomain is unloaded due to bin directory change during deployments, there
                    // is no any chance to get any notification, before IRegisteredObject.Stop is
                    // triggered. I've investigated ASP.NET source code for about a week, and found
                    // that timer with checking for shutdown reason is the only suitable way to get
                    // to know that application is going to shutdown.
                    _checkForShutdownTimer = new Timer(CheckForAppDomainShutdown, state: null,
                        dueTime: CheckForShutdownTimerInterval, period: CheckForShutdownTimerInterval);
                }
                else
                {
                    throw new NotSupportedException("Classic Pipeline is not supported. Switch your application pool to use Integrated Pipeline");
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Shutdown detection setup failed:", ex);
            }
        }
        
        private void RegisterForStopListeningEvent()
        {
            var stopEvent = typeof(HostingEnvironment).GetEvent("StopListening");
            if (stopEvent == null) return;

            stopEvent.AddEventHandler(null, new EventHandler(StopListening));
            Logger.Trace("StopListening even handler registered successfully.");
        }

        private void StopListening(object sender, EventArgs e)
        {
            Logger.Trace("StopListening event triggered.");
            Cancel();
        }

        private void CheckForAppDomainShutdown(object state)
        {
            if (UnsafeIISMethods.RequestedAppDomainRestart)
            {
                Logger.Trace("`RequestedAppDomainRestart` triggered.");
                Cancel();
            }
            
            // TODO: HostingEnvironment.ShutdownReason doesn't use any locks, when reading/writing the value
            if (HostingEnvironment.ShutdownReason != ApplicationShutdownReason.None)
            {
                Logger.Trace("HostingEnvironment.ShutdownReason != None triggered.");
                Cancel();
            }
        }

        public void Stop(bool immediate)
        {
            Logger.Trace("IRegisteredObject.Stop method called.");
            Cancel();
            HostingEnvironment.UnregisterObject(this);
        }

        private void Cancel()
        {
            // Stop the timer as we don't need it anymore
            _checkForShutdownTimer?.Dispose();

            Logger.Debug($"Application instance is shutting down: {HostingEnvironment.ShutdownReason}.");

            // Trigger the cancellation token
            try
            {
                _cts.Cancel(throwOnFirstException: false);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ag)
            {
                Logger.ErrorException("One or more exceptions were thrown during app pool shutdown: ", ag);
            }
        }

        public void Dispose()
        {
            _cts.Dispose();
            _checkForShutdownTimer?.Dispose();
        }
    }
}
