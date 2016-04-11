using System.IO;
using EPiServer.ClientTDDSupport.DependencyInjection;
using EPiServer.ClientTDDSupport.Models;
using EPiServer.Cms.Shell;
using EPiServer.Framework.Localization;
using EPiServer.Shell.UI;
using EPiServer.UI;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace EPiServer.Marketing.Testing.Web.Test
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTestRunner();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder builder, IHostingEnvironment environment, DojoConfiguration dojoConfiguration, ProviderBasedLocalizationService localizationService)
        {
            builder.UseTestRunner(environment);

            // Create a physical file provider to serve the javascript source as static files
            var sourcePath = Path.Combine(environment.WebRootPath, @"..\..\..\src\EPiServer.Marketing.Testing.Web\ClientResources");
            builder.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(sourcePath)
            });

            builder.UseLocalization(localizationService, "epi", typeof(InitializationModule).Assembly);
            builder.UseLocalization(localizationService, "epi-cms", typeof(InitializableModule).Assembly, typeof(EPiServerUIInitialization).Assembly);
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
