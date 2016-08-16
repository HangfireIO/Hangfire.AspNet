using System;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.Owin;

// ReSharper disable once CheckNamespace
namespace Hangfire
{
    /// <summary>
    /// Represents a generic <i>authorization filter</i> that allows to restrict 
    /// the access to Dashboard UI only for users who has the claim of the specified
    /// type with the given value.
    /// </summary>
    public class ClaimsAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private static readonly Func<DashboardContext, IOwinContext> DefaultContextAccessor
            = context => new OwinContext(context.GetOwinEnvironment());

        private readonly string _type;
        private readonly string _value;
        private readonly Func<DashboardContext, IOwinContext> _contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsAuthorizationFilter"/> class
        /// with the given claim type and value.
        /// </summary>
        /// <param name="type">The type of a claim that should be available to a user.</param>
        /// <param name="value">The value of a claim that should be set for a user.</param>
        public ClaimsAuthorizationFilter([NotNull] string type, [NotNull] string value)
            : this(type, value, DefaultContextAccessor)
        {
        }

        internal ClaimsAuthorizationFilter(
            [NotNull] string type,
            [NotNull] string value,
            [NotNull] Func<DashboardContext, IOwinContext> contextAccessor)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (contextAccessor == null) throw new ArgumentNullException(nameof(contextAccessor));

            _type = type;
            _value = value;
            _contextAccessor = contextAccessor;
        }

        /// <inheritdoc />
        public bool Authorize(DashboardContext dashboardContext)
        {
            var context = _contextAccessor(dashboardContext);
            
            return context.Authentication.User != null &&
                   context.Authentication.User.HasClaim(_type, _value);
        }
    }
}
