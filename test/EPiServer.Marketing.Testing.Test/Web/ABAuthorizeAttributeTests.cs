using EPiServer.Marketing.Testing.Web.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ABAuthorizeAttributeTests : ABAuthorizeAttribute
    {
        public ABAuthorizeAttributeTests() : base(Roles: "BaseRole")
        {
        }

        [Fact]
        public void Constructor_AddsExpectedRoles()
        {
             Assert.True(AuthorizeCore(GetContext()));
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
            this.DefaultRoles.Add("NotAMember");

            Assert.False(AuthorizeCore(GetContext()));
        }

        private HttpContextBase GetContext()
        {
            // Arrange
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var winIdentity = new Mock<IIdentity>();
            winIdentity.Setup(i => i.IsAuthenticated).Returns(() => true);
            winIdentity.Setup(i => i.Name).Returns(() => "WHEEEE");
            httpContext.SetupGet(c => c.User).Returns(() => new ImdPrincipal(winIdentity.Object)); // This is my implementation of IIdentity
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
    }

    public class ImdPrincipal : IPrincipal
    {
        IIdentity identiy;

        public ImdPrincipal(IIdentity identiy) 
        { Identity = identiy;  }

        public string Name => throw new NotImplementedException();

        public string AuthenticationType => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();

        public IIdentity Identity { get; set; }

        public bool IsInRole(string role)
        {
            return role == "CmsAdmin" || role == "CmsEditor" || role == "BaseRole";
        }
    }
}
