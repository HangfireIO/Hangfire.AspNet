using System;
using System.Collections.Generic;
using Hangfire.Annotations;
using Hangfire.AspNet;
using Owin;

// ReSharper disable once CheckNamespace
namespace Hangfire
{
    public static class HangfireAspNet
    {
        private static readonly RegisteredObjectWrapper Instance = new RegisteredObjectWrapper();
        
        /// <summary>
        /// Call this in global.asax for debugging purposes
        /// </summary>
        public static void Use([NotNull] Func<IEnumerable<IDisposable>> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            Instance.Start(configuration);
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
    }
}
