using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EPiServer.Marketing.Testing.TestPages
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {

            ////app.UseIISPlatformHandler();

            ////app.UseStaticFiles();

            ////app.UseMvc();

            app.UseMvc(routes =>
            {
                ////routes.MapRoute(name: "AB API Testing",
                ////               url: "ApiTesting/{action}/{state}",
                ////               defaults: new { controller = "ApiTesting", action = "Index", state = UrlParameter.Optional });

                routes.MapRoute(
               name: "AB API Testing",
               template: "{controller}/{action}/{id?}",
               defaults: new { controller = "ApiTesting", action = "Index" });
            });
        }
    }
}
