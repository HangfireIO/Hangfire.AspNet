using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using Hangfire.Annotations;
using Hangfire.AspNet;
using Hangfire.Logging;
using Hangfire.Server;
using Owin;

// ReSharper disable once CheckNamespace
namespace Hangfire
{
    /// <summary>
    /// Provides thread-safe idempotent methods for running <see cref="BackgroundJobServer"/>
    /// and <see cref="BackgroundProcessingServer"/> instances in ASP.NET application with
    /// automatic disposal on AppDomain shutdown, without waiting for pending HTTP requests
    /// to complete.
    /// </summary>
    /// <remarks>
    /// Methods of this class use <see cref="AppDomainShutdownToken"/> to get notified, when
    /// ASP.NET application's app domain is going to shutdown. It uses extended checks to
    /// get this notifications, comparing to the <see cref="IRegisteredObject"/> interface,
    /// please see the source code for details.
    /// </remarks>
    public static class HangfireAspNet
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(HangfireAspNet));
        private static readonly object SyncRoot = new object();

        private static ShutdownDetector _detector;
        private static bool _started;
        private static IDisposable[] _disposables;

        /// <summary>
        /// Gets an instance of the <see cref="CancellationToken"/> class that is canceled, 
        /// when ASP.NET application is going to shutdown, without wait for pending HTTP 
        /// requests to complete, as opposed to the <see cref="IRegisteredObject"/> interface 
        /// implementations.
        /// </summary>
        public static CancellationToken AppDomainShutdownToken =>
            LazyInitializer.EnsureInitialized(ref _detector, InitShutdownDetector).Token;

        /// <summary>
        /// Calls the given <paramref name="configuration"/> delegate, and registers all the
        /// returned instances to be disposed, when ASP.NET application's app domain is going 
        /// to shutdown, using the <see cref="AppDomainShutdownToken"/> property.
        /// </summary>
        /// <remarks>
        /// <para>This method is designed to run in implementations of the <see cref="IProcessHostPreloadClient"/>
        /// interface, in Global.asax class, or anywhere else, where OWIN's <see cref="IAppBuilder"/>
        /// interface is not accessible.</para>
        /// <para>This method is thread-safe and idempotent, the <paramref name="configuration"/>
        /// delegate is called <b>only once</b>, regardless of the number of <see cref="Use"/>
        /// or <see cref="UseHangfireAspNet"/> method calls.</para> 
        /// </remarks>
        /// <seealso cref="UseHangfireAspNet"/>
        public static void Use([NotNull] Func<IEnumerable<IDisposable>> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            StartInstances(configuration);
        }

        /// <summary>
        /// Calls the given <paramref name="configuration"/> delegate, and registers all the
        /// returned instances to be disposed, when ASP.NET application's app domain is going 
        /// to shutdown,  using the <see cref="AppDomainShutdownToken"/> property.
        /// </summary>
        /// <remarks>
        /// <para>This method is designed to run in OWIN's Startup class, using the <see cref="IAppBuilder"/> 
        /// interface. Use the <see cref="Use"/> method for non-OWIN setups.</para>
        /// <para>This method is thread-safe and idempotent, the <paramref name="configuration"/>
        /// delegate called <b>only once</b>, regardless of the number of <see cref="Use"/>
        /// or <see cref="UseHangfireAspNet"/> method calls.</para>
        /// </remarks>
        public static IAppBuilder UseHangfireAspNet(
            [NotNull] this IAppBuilder builder,
            [NotNull] Func<IEnumerable<IDisposable>> configuration)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            Use(configuration);

            return builder;
        }

        private static void StartInstances(Func<IEnumerable<IDisposable>> configuration)
        {
            lock (SyncRoot)
            {
                if (_started) return;
                _started = true;

                _disposables = configuration()?.ToArray();

                AppDomainShutdownToken.Register(StopInstances);
            }
        }

        private static void StopInstances()
        {
            lock (SyncRoot)
            {
                if (_disposables == null || _disposables.Length == 0) return;

                foreach (var disposable in _disposables)
                {
                    StopInstance(disposable);
                }

                foreach (var disposable in _disposables)
                {
                    DisposeInstance(disposable);
                }
            }
        }

        private static void StopInstance(IDisposable disposable)
        {
            try
            {
                var processingServer = disposable as BackgroundProcessingServer;
                processingServer?.SendStop();

                var jobServer = disposable as BackgroundJobServer;
                jobServer?.SendStop();
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occurred while sending 'Stop' signal: ", ex);
                throw;
            }
        }

        private static void DisposeInstance(IDisposable disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An exception occurred while disposing processing server: ", ex);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", 
            Justification = "Only cleaned up on shutdown")]
        private static ShutdownDetector InitShutdownDetector()
        {
            var detector = new ShutdownDetector();
            detector.Initialize();
            return detector;
        }
    }
}
