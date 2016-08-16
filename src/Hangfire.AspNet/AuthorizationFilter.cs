using System;
using System.Linq;
using System.Security.Claims;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.Owin;

// ReSharper disable once CheckNamespace
namespace Hangfire
{
    /// <summary>
    /// Represents a basic <i>authorization filter</i> that allows to restrict the 
    /// access to Dashboard UI only to authenticated users, as well as restrict the
    /// access only to specified users or roles.
    /// </summary>
    public class AuthorizationFilter : IDashboardAuthorizationFilter
    {
        private static readonly Func<DashboardContext, IOwinContext> DefaultContextAccessor
            = context => new OwinContext(context.GetOwinEnvironment());

        private static readonly string[] EmptyArray = new string[0];

        private readonly Func<DashboardContext, IOwinContext> _contextAccessor;
        
        private string _roles;
        private string[] _rolesSplit = EmptyArray;
        private string _users;
        private string[] _usersSplit = EmptyArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationFilter"/> class.
        /// By default, it enables <i>all authenticated users</i> to access the Dashboard 
        /// UI, please use <see cref="Users"/> and <see cref="Roles"/> properties to 
        /// configure this behavior.
        /// </summary>
        public AuthorizationFilter() : this(DefaultContextAccessor)
        {
        }

        internal AuthorizationFilter([NotNull] Func<DashboardContext, IOwinContext> contextAccessor)
        {
            if (contextAccessor == null) throw new ArgumentNullException(nameof(contextAccessor));
            _contextAccessor = contextAccessor;
        }
        
        /// <summary>
        /// Gets or sets the roles that are authorized to access the Dashboard UI.
        /// Multiple values are allowed by separating them with commas: "Staff,Admin",
        /// set it to <see langword="null"/> to remove the roles restriction.
        /// </summary>
        public string Roles
        {
            get { return _roles ?? String.Empty; }
            set
            {
                _roles = value;
                _rolesSplit = SplitString(value);
            }
        }

        /// <summary>
        /// Gets or sets the users that are authorized to access the Dashboard UI.
        /// Multiple values are allowed by separating them with commas: "Alice,Bob",
        /// set it to <see langword="null"/> to remove the users restriction.
        /// </summary>
        public string Users
        {
            get { return _users ?? String.Empty; }
            set
            {
                _users = value;
                _usersSplit = SplitString(value);
            }
        }

        /// <inheritdoc />
        public bool Authorize(DashboardContext context)
        {
            var owinContext = _contextAccessor(context);
            var user = owinContext.Authentication.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (_usersSplit.Length > 0 && !_usersSplit.Contains(user.Identity.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                return false;
            }

            if (_rolesSplit.Length > 0 && !_rolesSplit.Any(user.IsInRole))
            {
                return false;
            }

            return true;
        }

        private static string[] SplitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return EmptyArray;
            }

            return original.Split(',')
                .Select(x => x.Trim())
                .Where(x => !String.IsNullOrEmpty(x))
                .ToArray();
        }
    }
}
