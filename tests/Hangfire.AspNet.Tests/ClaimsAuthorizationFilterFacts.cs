using System;
using System.Security.Claims;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using Xunit;

// ReSharper disable AssignNullToNotNullAttribute

namespace Hangfire.AspNet.Tests
{
    public class ClaimsAuthorizationFilterFacts
    {
        private readonly Mock<IOwinContext> _context;
        private readonly Mock<IAuthenticationManager> _authentication;
        private readonly ClaimsPrincipal _principal;

        public ClaimsAuthorizationFilterFacts()
        {
            _context = new Mock<IOwinContext>();

            _authentication = new Mock<IAuthenticationManager>();

            _principal = new ClaimsPrincipal();

            _authentication.Setup(x => x.User).Returns(_principal);
            _context.Setup(x => x.Authentication).Returns(_authentication.Object);
        }

        [Fact]
        public void Ctor_ThrowsAnException_WhenTypeArgumentIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ClaimsAuthorizationFilter(null, "value"));

            Assert.Equal("type", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsAnException_WhenValueArgumentIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ClaimsAuthorizationFilter("type", null));

            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenUserIsNull()
        {
            _authentication.Setup(x => x.User).Returns((ClaimsPrincipal)null);
            var filter = CreateFilter();

            var result = filter.Authorize(null);

            Assert.False(result);
        }

        [Fact]
        public void Authorize_ReturnsTrue_WhenUserHasTheSpecifiedClaim()
        {
            var filter = CreateFilter("claimType", "claimValue");
            _principal.AddIdentity(new ClaimsIdentity(new [] { new Claim("claimType", "claimValue") }));

            var result = filter.Authorize(null);

            Assert.True(result);
        }

        [Fact]
        public void Authorize_ReturnsFalse_WhenUserDoesNotHaveTheSpecifiedClaim()
        {
            var filter = CreateFilter();
            _principal.AddIdentity(new ClaimsIdentity(new [] { new Claim("anotherType", "AnotherValue") }));

            var result = filter.Authorize(null);

            Assert.False(result);
        }

        private ClaimsAuthorizationFilter CreateFilter(string type = "type", string value = "value")
        {
            return new ClaimsAuthorizationFilter(type, value, x => _context.Object);
        }
    }
}
