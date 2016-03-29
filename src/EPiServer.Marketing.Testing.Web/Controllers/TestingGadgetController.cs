using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Gadgets;

namespace EPiServer.Marketing.Testing.Web
{
    //[Gadget(Title = "Marketing Content Testing Report")]
    class TestingGadgetController : Controller
    {
        //private IServiceLocator _serviceLocator;

        //[ExcludeFromCodeCoverage]
        //public TestingGadgetController()
        //{
        //    _serviceLocator = ServiceLocator.Current;
        //}

        //internal TestingGadgetController(IServiceLocator serviceLocator)
        //{
        //    _serviceLocator = serviceLocator;
        //}

        //public ActionResult Index()
        //{
        //    ITestRepository testRepo = _serviceLocator.GetInstance<ITestRepository>();
        //    return PartialView(testRepo.GetTestList(new TestCriteria()));
        //}

        //public ActionResult Details(string id)
        //{
        //    var testId = Guid.Parse(id);
        //    ITestRepository testRepo = _serviceLocator.GetInstance<ITestRepository>();
        //    var test = testRepo.GetTestById(testId);

        //    // will we ever show details of a list of tests?
        //    List<ABTestViewModel> list = new List<ABTestViewModel>();
        //    list.Add(test);
        //    return PartialView(list);
        //}
    }
}
