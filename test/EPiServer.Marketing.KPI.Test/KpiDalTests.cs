using System.Data.Common;
using System.Linq;
using Xunit;

namespace EPiServer.Marketing.KPI.Test
{
    internal class KpiDalTests : KpiTestBase
    {
        private KpiTestContext _context;
        private DbConnection _dbConnection;


        public KpiDalTests()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new KpiTestContext(_dbConnection);
        }

        [Fact]
        public void AddMultivariateTest()
        {
            var newTests = AddKpis(_context, 2);
            _context.SaveChanges();

            Assert.Equal(_context.Kpis.Count(), 2);
        }

        [Fact]
        public void DeleteMultivariateTest()
        {
            var newTests = AddKpis(_context, 3);
            _context.SaveChanges();

            
            _context.Kpis.Remove(newTests[0]);
            _context.SaveChanges();

            Assert.Equal(_context.Kpis.Count(), 2);
        }

       
    }
}
