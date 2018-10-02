using System.IO;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace IGSLControlPanel
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<IGSLContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("IGSLConnection")));
            services.AddSingleton<FolderDataHelper>();
            services.AddSingleton<ProductsHelper>();
            services.AddSingleton<TariffsHelper>();
            services.AddSingleton<InsuranceRulesHelper>();
            services.AddSingleton<RiskFactorHelper>();
            services.AddSingleton<FilesHelper>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ExcelFiles")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "ExcelFiles"));
            }
            services.AddSingleton<IFileProvider>(
                new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "ExcelFiles")));
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStatusCodePages();

            app.UseStaticFiles();

            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
