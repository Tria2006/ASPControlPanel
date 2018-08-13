using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton<EntityStateHelper>();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

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
                //app.UseBrowserLink();
                //app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
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
