using EPiServer.Marketing.Testing.Web.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class ABAuthorizeAttributeTests : ABAuthorizeAttribute
    {
        public static List<string> userRoles = new List<string> { "CmsAdmin", "CmsEditor", "ConstructorRole" };

        static ABAuthorizeAttributeTests()
        {
            ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"] = "AppSettingsRole";
            ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Users"] = "AppSettingsUser";
        }

        public ABAuthorizeAttributeTests() : base(roles: "ConstructorRole", users: "ConstructorUser")
        {
        }

        [Fact]
        public void Constructor_AddsExpectedUsers_And_Authorizes()
        {
            this.Roles.Clear();

            Assert.True(AuthorizeCore(GetContext()));
        }

        [Fact]
        public void Constructor_AddsExpectedRoles()
        {
            Assert.True(AuthorizeCore(GetContext()));
        }

        [Fact]
        public void Constructor_AddsExpectedRoles_FromAppSettings()
        {
            userRoles.Clear();
            userRoles.Add("AppSettingsRole");

            Assert.True(AuthorizeCore(GetContext()));
            Assert.Equal(2, this.Roles.Count);
            Assert.True(this.Roles.Contains("AppSettingsRole") && this.Roles.Contains("ConstructorRole"));
        }

        [Fact]
        public void Constructor_AddsExpectedUsers_FromAppSettings()
        {
            userRoles.Clear();

            Assert.True(AuthorizeCore(GetContext()));
            Assert.Equal(2, this.Users.Count);
            Assert.True(this.Users.Contains("AppSettingsUser") && this.Users.Contains("ConstructorUser"));
        }

        [Fact]
        public void AuthorizeCore_ThrowsException_When_HttpContext_IsNull()
        {
            Assert.Throws<ArgumentNullException>(() => AuthorizeCore(null));
        }

        [Fact]
        public void AuthorizeCore_ReturnsTrue_WhenUserInRole()
        {
            this.Roles.Clear();
            this.Roles.Add("CmsAdmin");

            Assert.True(AuthorizeCore(GetContext()));
        }

        [Fact]
        public void AuthorizeCore_ReturnsFalse_WhenUserNotInRole()
        {
            this.Roles.Clear();
            this.Users.Clear();
            this.Roles.Add("NotAMember");

            Assert.False(AuthorizeCore(GetContext()));
        }

        [Fact]
        public void SplitString_Returns_EmptyArray()
        {
            Assert.Empty(SplitString(null));
            Assert.Empty(SplitString(""));
        }

        private HttpContextBase GetContext()
        {
            // Arrange
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var winIdentity = new Mock<IIdentity>();
            winIdentity.Setup(i => i.IsAuthenticated).Returns(() => true);
            winIdentity.Setup(i => i.Name).Returns(() => "ConstructorUser");
            httpContext.SetupGet(c => c.User).Returns(() => new ImdPrincipal(winIdentity.Object));
            var requestBase = new Mock<HttpRequestBase>();
            var headers = new NameValueCollection
        {
           {"Special-Header-Name", "false"}
        };
            requestBase.Setup(x => x.Headers).Returns(headers);
            requestBase.Setup(x => x.HttpMethod).Returns("GET");
            requestBase.Setup(x => x.Url).Returns(new Uri("http://localhost/"));
            requestBase.Setup(x => x.RawUrl).Returns("~/Maintenance/UnExistingMaster");
            requestBase.Setup(x => x.AppRelativeCurrentExecutionFilePath).Returns(() => "~/Maintenance/UnExistingMaster");
            requestBase.Setup(x => x.IsAuthenticated).Returns(() => true);
            httpContext.Setup(x => x.Request).Returns(requestBase.Object);

            return httpContext.Object;
        }

        public class ImdPrincipal : IPrincipal
        {
            IIdentity identity;

            public ImdPrincipal(IIdentity identity)
            {
                Identity = identity;
            }

            public string Name { get; set; }

            public string AuthenticationType => throw new NotImplementedException();

            public bool IsAuthenticated => throw new NotImplementedException();

            public IIdentity Identity { get; set; }

            public bool IsInRole(string role)
            {
                return ABAuthorizeAttributeTests.userRoles.Contains(role);
            }
        }
    }
}
