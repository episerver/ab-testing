using System;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.KPI.DataAccess;
using EPiServer.Marketing.KPI.Model;
using EPiServer.Marketing.KPI.Model.Enums;
using Xunit;

namespace EPiServer.Marketing.KPI.Test
{
    public class KpiDataAccessTests : KpiTestBase
    {
        //private KpiTestContext _context;
        //private DbConnection _dbConnection;
        //private KpiDataAccess _mtm;
        //public KpiDataAccessTests()
        //{
        //    _dbConnection = Effort.DbConnectionFactory.CreateTransient();
        //    _context = new KpiTestContext(_dbConnection);
        //    _mtm = new KpiDataAccess(new KpiTestRepository(_context));
        //}

        //[Fact]
        //public void AddMultivariateTests()
        //{
        //    var newTests = AddMultivariateTests(_mtm, 2);
        //    _context.SaveChanges();
            
        //    Assert.Equal(_mtm.Get(newTests[0].Id), newTests[0]);
        //    Assert.Equal(_context.Kpis.Count(), 2);
        //}

        //[Fact]
        //public void TestManagerGet()
        //{
        //    var id = Guid.NewGuid();

        //    var test = new Kpi()
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = "test",
        //        Weight = 100,
        //        ClientScripts = "testing...",
        //        RunAt = RunAt.Server,
        //        Value = "TestValue",
        //        LandingPage = Guid.NewGuid()
        //    };

        //    _mtm.Save(test);

        //    Assert.Equal(_mtm.Get(id), test);
        //}

        //[Fact]
        //public void TestManagerGetTestListNoFilter()
        //{
        //    var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
        //    var tests = AddMultivariateTests(_context, 2);

        //    var list = _mtm.Get(tests[0].Id);
        //    Assert.Equal(list.Id, tests[0].Id);
        //}

        //[Fact]
        //public void TestManagerSave()
        //{
        //    var tests = AddMultivariateTests(_mtm, 1);
        //    var newTitle = "newTitle";
        //    tests[0].Title = newTitle;
        //    _mtm.Save(tests[0]);

        //    Assert.Equal(_mtm.Get(tests[0].Id).Title, newTitle);
        //}

        //[Fact]
        //public void TestManagerDelete()
        //{
        //    var tests = AddMultivariateTests(_mtm, 3);

        //    _mtm.Delete(tests[0].Id);
        //    _mtm._repository.SaveChanges();

        //    Assert.Equal(_mtm._repository.GetAll().Count(), 2);
        //}

    }
}
