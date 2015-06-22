using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net;
using Moq;

namespace EPiServer.Marketing.Multivariate.Test.Core
{
    [TestClass]
    public class MultivariateTestTests
    {
        private Mock<ILog> log;
        private MultivariateTest GetUnitUnderTest()
        {
            log = new Mock<ILog>();
            return new MultivariateTest(log.Object);
        }

        [TestMethod]
        public void Test1()
        {
        }
    }
}
