using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace EpiServer.Multivariate.TestPages
{
    public class EPiServerApplication : EPiServer.Global
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            
            //Tip: Want to call the EPiServer API on startup? Add an initialization module instead (Add -> New Item.. -> EPiServer -> Initialization Module)
        }
        
        
    }
}