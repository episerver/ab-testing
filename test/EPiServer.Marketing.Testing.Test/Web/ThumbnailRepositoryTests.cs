using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ThumbnailRepositoryTests
    {
        Mock<IServiceLocator> _mockServiceLocator = new Mock<IServiceLocator>();
        Mock<IHttpContextHelper> _mockContextHelper = new Mock<IHttpContextHelper>();
        private ThumbnailRepository GetUnitUnderTest()
        {
            _mockServiceLocator.Setup(sl => sl.GetInstance<IHttpContextHelper>()).Returns(_mockContextHelper.Object);
            return new ThumbnailRepository(_mockServiceLocator.Object);
        }

        [Fact]
        public void GetRandomFileName_ReturnsGuidBasedFileName()
        {
            var thumbRepo = GetUnitUnderTest();
            Guid guidOut;
            FileInfo fi = new FileInfo(thumbRepo.GetRandomFileName());
            Assert.True(Guid.TryParse(fi.na(fi.Extension), out guidOut));
            Assert.True(fi.Extension == ".png");
        }

    }
}

   
 