using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundestagMine.Data;
using BundestagMine.Services;
using BundestagMine.SqlDatabase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            services.AddDbContext<IdentityContext>(
                option => option.UseSqlServer(ConfigManager.GetConnectionString(), o => o.CommandTimeout(600)));
            services.AddLogging(c => c.ClearProviders());
            
            // Identity Configurations.
            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<IdentityContext>()
                    .AddDefaultUI()
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

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
            services.AddTransient<ImportService>();
            services.AddTransient<GlobalSearchService>();
            services.AddTransient<DownloadCenterService>();

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
            app.UseStaticFiles();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
