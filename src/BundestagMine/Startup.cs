using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BundestagMine
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BundestagMineDbContext>(
                option => option.UseSqlServer(ConfigManager.GetConnectionString(), o => o.CommandTimeout(600)));
           
            services.AddControllersWithViews();
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddTransient<AnnotationService>();
            services.AddTransient<ViewRenderService>();
            services.AddTransient<GraphService>();
            services.AddTransient<MetadataService>();
            services.AddTransient<BundestagScraperService>();
            services.AddTransient<TopicAnalysisService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
