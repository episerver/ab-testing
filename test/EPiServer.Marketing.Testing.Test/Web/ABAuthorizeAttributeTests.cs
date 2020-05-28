using EPiServer.Marketing.Testing.Web.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ABAuthorizeAttributeTests : ABAuthorizeAttribute
    {
        public static List<string> userRoles = new List<string> { "CmsAdmin", "CmsEditor", "BaseRole" };

        static ABAuthorizeAttributeTests()
        {
            ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"] = "LocalAdmins";
        }

        public ABAuthorizeAttributeTests() : base(roles: "BaseRole", users: "UserName")
        {
        }

        [Fact]
        public void Constructor_AddsExpectedUsers_And_Authorizes()
        {
            this.DefaultRoles.Clear();

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
            userRoles.Add("LocalAdmins");

            Assert.True(AuthorizeCore(GetContext()));
            Assert.Equal(2, this.DefaultRoles.Count);
            Assert.True(this.DefaultRoles.Contains("LocalAdmins") && this.DefaultRoles.Contains("BaseRole"));
        }

        [Fact]
        public void AuthorizeCore_ReturnsTrue_WhenUserInRole()
        {
            this.DefaultRoles.Clear();
            this.DefaultRoles.Add("CmsAdmin");

            Assert.True(AuthorizeCore(GetContext()));
        }

        [Fact]
        public void AuthorizeCore_ReturnsFalse_WhenUserNotInRole()
        {
            this.DefaultRoles.Clear();
            this.DefaultUsers.Clear();
            this.DefaultRoles.Add("NotAMember");

            Assert.False(AuthorizeCore(GetContext()));
        }

        private HttpContextBase GetContext()
        {
            // Arrange
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var winIdentity = new Mock<IIdentity>();
            winIdentity.Setup(i => i.IsAuthenticated).Returns(() => true);
            winIdentity.Setup(i => i.Name).Returns(() => "UserName");
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
            IIdentity identiy;

            public ImdPrincipal(IIdentity identiy)
            { 
                Identity = identiy;
                Name = "UserName";
            }

            public string Name  { get; set; }

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
