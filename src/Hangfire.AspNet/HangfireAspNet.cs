using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Hangfire.Annotations;
using Hangfire.AspNet;
using Hangfire.Logging;
using Hangfire.Server;
using Owin;

// ReSharper disable once CheckNamespace
namespace Hangfire
{
    public static class HangfireAspNet
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(HangfireAspNet));
        private static readonly object SyncRoot = new object();

        private static ShutdownDetector _detector;
        private static bool _started;
        private static IDisposable[] _disposables;

        public static CancellationToken ShutdownToken =>
            LazyInitializer.EnsureInitialized(ref _detector, InitShutdownDetector).Token;

        /// <summary>
        /// Call this in global.asax for debugging purposes
        /// </summary>
        public static void Use([NotNull] Func<IEnumerable<IDisposable>> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            StartInstances(configuration);
        }

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

                ShutdownToken.Register(StopInstances);
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
