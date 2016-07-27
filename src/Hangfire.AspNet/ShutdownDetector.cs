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
    internal class ShutdownDetector : IRegisteredObject, IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<ShutdownDetector>();

        private readonly CancellationTokenSource _cts;
        private IDisposable _checkAppPoolTimer;

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

                // Normally when the AppDomain shuts down IRegisteredObject.Stop gets called, except that
                // ASP.NET waits for requests to end before calling IRegisteredObject.Stop. This can be
                // troublesome for some frameworks like SignalR that keep long running requests alive.
                // These are more aggressive checks to see if the app domain is in the process of being shutdown and
                // we trigger the same cts in that case.
                if (HttpRuntime.UsingIntegratedPipeline)
                {
                    RegisterForStopListeningEvent();

                    if (UnsafeIISMethods.CanDetectAppDomainRestart)
                    {
                        // Create a timer for polling when the app pool has been requested for shutdown.
                        _checkAppPoolTimer = new Timer(CheckForAppDomainRestart, state: null,
                            dueTime: TimeSpan.FromSeconds(10), period: TimeSpan.FromSeconds(10));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Shutdown detection setup failed:", ex);
            }
        }

        // Note: When we have a compilation that targets .NET 4.5.1, implement IStopListeningRegisteredObject
        // instead of reflecting for HostingEnvironment.StopListening.
        private bool RegisterForStopListeningEvent()
        {
            var stopEvent = typeof(HostingEnvironment).GetEvent("StopListening");
            if (stopEvent == null)
            {
                return false;
            }
            stopEvent.AddEventHandler(null, new EventHandler(StopListening));
            return true;
        }

        private void StopListening(object sender, EventArgs e)
        {
            Cancel();
        }

        private void CheckForAppDomainRestart(object state)
        {
            if (UnsafeIISMethods.RequestedAppDomainRestart)
            {
                Cancel();
            }
        }

        public void Stop(bool immediate)
        {
            Cancel();
            HostingEnvironment.UnregisterObject(this);
        }

        private void Cancel()
        {
            // Stop the timer as we don't need it anymore
            _checkAppPoolTimer?.Dispose();

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
                Logger.ErrorException("One or more exceptions were thrown during app pool shutdown:", ag);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts.Dispose();
                _checkAppPoolTimer?.Dispose();
            }
        }
    }
}
