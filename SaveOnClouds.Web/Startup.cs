using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SaveOnClouds.CloudFuncs.Storage;
using SaveOnClouds.Notifications.Data;
using SaveOnClouds.Web.Data.EnvResources;
using SaveOnClouds.Web.Identity;
using SaveOnClouds.Web.Services;
using SaveOnClouds.Web.Services.DataAccess;
using SaveOnClouds.Web.Services.Notifications;

namespace SaveOnClouds.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private AppSettings AppSettings { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AppSettings = Configuration.Get<AppSettings>();
            services.Configure<IOptions<AppSettings>>(Configuration);

            services.AddControllers();
            services.AddMvc();

            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddOptions();

            ConfigureAWS(services);
            ConfigureDbContexts(services);
            ConfigureExternalAuthProviders(services);
            ConfigureCookieBasedAuth(services);
            RegisterDependencies(services);
        }

        private void ConfigureAWS(IServiceCollection services)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSimpleNotificationService>();
        }

        private void ConfigureDbContexts(IServiceCollection services)
        {
            services.AddDbContext<NotificationDbContext>(o => { o.UseSqlServer(AppSettings.ConnectionStrings.Default); })
                .AddDbContext<EnvResourcesDbContext>( o => { o.UseSqlServer(AppSettings.ConnectionStrings.Default); });
        }

        private void ConfigureExternalAuthProviders(IServiceCollection services)
        {
            services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = AppSettings.Authentication.Google.ClientId;
                    options.ClientSecret = AppSettings.Authentication.Google.ClientSecret;
                })
                .AddLinkedIn(options =>
                {
                    options.ClientId = AppSettings.Authentication.LinkedIn.ClientId;
                    options.ClientSecret = AppSettings.Authentication.LinkedIn.ClientSecret;
                    options.Events = new OAuthEvents
                    {
                        OnRemoteFailure = loginFailureHandler =>
                        {
                            var authProperties =
                                options.StateDataFormat.Unprotect(loginFailureHandler.Request.Query["state"]);
                            loginFailureHandler.Response.Redirect("/Accounts/Signin");
                            loginFailureHandler.HandleResponse();
                            return Task.FromResult(0);
                        }
                    };
                });
            ;
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddDbContext<CloudResourceStorageDbContext>(o=>o.UseSqlServer(Configuration["ConnectionStrings:Default"]));
            services.AddSingleton<IEmailSender, MailJetEmailSender>();
            services.AddScoped<IDataAccess, AdoDataAccess>();
            services.AddScoped<IChannelManager, ChannelManager>();
            services.AddScoped<IEnvResourcesQueryService, EnvResourcesQueryService>();
            services.AddSingleton<INotificationService, SnsNotificationService>();
            services.AddScoped<ICloudStorageService, EfCloudStorageService>();
            services.AddHttpClient();
        }

        private void ConfigureCookieBasedAuth(IServiceCollection services)
        {
            var authConfig = Configuration.GetSection("Authentication:Options").Get<AuthenticationOptions>();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(AppSettings.ConnectionStrings.Default));
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = authConfig.RequiredLength;
                options.Password.RequireDigit = authConfig.RequireDigit;
                options.Password.RequireNonAlphanumeric = authConfig.RequireNonAlphanumeric;
                options.Password.RequireUppercase = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.SignIn.RequireConfirmedEmail = true;
            });

            services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/Accounts/Signin";
                option.AccessDeniedPath = "/Accounts/AccessDenied";
                option.ExpireTimeSpan = TimeSpan.FromHours(authConfig.CookieExpirationHours);
            });
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}