using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using Xunit;

// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable AssignNullToNotNullAttribute

namespace Hangfire.AspNet.Tests
{
    public class AuthorizationFilterFacts
    {
        private readonly AuthorizationFilter _filter;
        private readonly Mock<IOwinContext> _context;
        private readonly Mock<IAuthenticationManager> _authentication;
        private readonly ClaimsPrincipal _principal;

        public AuthorizationFilterFacts()
        {
            _context = new Mock<IOwinContext>();

            _authentication = new Mock<IAuthenticationManager>();

            _principal = new ClaimsPrincipal();
            
            _authentication.Setup(x => x.User).Returns(_principal);
            _context.Setup(x => x.Authentication).Returns(_authentication.Object);

            _filter = new AuthorizationFilter(x => _context.Object);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenUserInstance_IsNull()
        {
            _authentication.Setup(x => x.User).Returns((ClaimsPrincipal)null);

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenUserIdentity_IsNull()
        {
            _authentication.Setup(x => x.User).Returns(new ClaimsPrincipal());

            var result = _filter.Authorize(CreateContext());

            Assert.Null(_authentication.Object.User.Identity);
            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenUserIdentity_IsNotAuthenticated()
        {
            _principal.AddIdentity(CreateIdentity());

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WhenUserIsAuthenticated_AndThereAreNoExplicitUsersAndRolesSpecified()
        {
            _principal.AddIdentity(CreateIdentity("Admin"));

            var result = _filter.Authorize(CreateContext());
            
            Assert.True(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenCurrentUser_IsNotEqualToTheGivenOne()
        {
            _filter.Users = "Admin";
            _principal.AddIdentity(CreateIdentity("Vasya"));

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WhenCurrentUser_EqualsToTheGivenOne()
        {
            _filter.Users = "Admin";
            _principal.AddIdentity(CreateIdentity("Admin"));

            var result = _filter.Authorize(CreateContext());

            Assert.True(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenCurrentUser_IsNotInTheGivenList()
        {
            _filter.Users = "Admin, Root";
            _principal.AddIdentity(CreateIdentity("Vasya"));

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WhenCurrentUser_IsInTheGivenList()
        {
            _filter.Users = "Admin, Root";
            _principal.AddIdentity(CreateIdentity("Root"));

            var result = _filter.Authorize(CreateContext());

            Assert.True(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WhenCurrentUserEqualsToTheGivenOne_ButHasAnotherCase()
        {
            _filter.Users = "AdMiN";
            _principal.AddIdentity(CreateIdentity("admin"));

            var result = _filter.Authorize(CreateContext());

            Assert.True(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenCurrentUserRole_IsNotEqualToTheGivenOne()
        {
            _filter.Roles = "Admin";
            _principal.AddIdentity(CreateIdentity("Vasya", "Staff"));

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WhenCurrentUserRole_EqualsToTheGivenOne()
        {
            _filter.Roles = "Admin";
            _principal.AddIdentity(CreateIdentity("Vasya", "Admin"));

            var result = _filter.Authorize(CreateContext());

            Assert.True(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenCurrentUserRole_IsNotInTheGivenList()
        {
            _filter.Roles = "Admin, Root";
            _principal.AddIdentity(CreateIdentity("Vasya", "Staff"));

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WhenCurrentUserRole_IsInTheGivenList()
        {
            _filter.Roles = "Admin, Root";
            _principal.AddIdentity(CreateIdentity("Vasya", "Root"));

            var result = _filter.Authorize(CreateContext());

            Assert.True(result);
        }

        [Fact]
        public void Authorize_ReturnsFALSE_WhenCurrentUserRoleEqualsToTheGivenOne_ButHasAnotherCase()
        {
            _filter.Roles = "AdMiN";
            _principal.AddIdentity(CreateIdentity("Vasya", "admin"));

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WithEmptyUserOptions_AndNonAuthenticatedUser()
        {
            _filter.Users = "";
            _principal.AddIdentity(CreateIdentity(""));

            var result = _filter.Authorize(CreateContext());

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WithEmptyRolesOptions_AndUserWithNoRoles()
        {
            _filter.Roles = "";
            _principal.AddIdentity(CreateIdentity("Vasya"));

            var result = _filter.Authorize(CreateContext());

            Assert.True(result);
        }

        [DebuggerStepThrough]
        private static DashboardContext CreateContext()
        {
            // It doesn't matter what context we are using, because converter 
            // will always return the correct IOwinContext instance.
            return null;
        }

        private static ClaimsIdentity CreateIdentity(string name = "", string role = null)
        {
            var identity = new GenericIdentity(name, "Basic");

            if (role != null)
            {
                identity.AddClaim(new Claim(identity.RoleClaimType, role));
            }

            return identity;
        }
    }
}



