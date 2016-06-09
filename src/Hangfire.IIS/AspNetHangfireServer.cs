using System;
using System.Collections.Generic;
using Hangfire.Annotations;
using Owin;

namespace Hangfire.IIS
{
    public static class AspNetHangfireServer
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

        public static IAppBuilder UseHangfire(
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
