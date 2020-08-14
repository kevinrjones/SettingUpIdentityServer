using System.Reflection;
using Ids;
using IdSWithIdentity.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdS
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public IWebHostEnvironment Environment { get; set; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            
            services.AddIdentityServer()
                .AddAspNetIdentity<IdentityUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlite(connectionString, 
                        opt => opt.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlite(connectionString, 
                        opt => opt.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                })

                .AddDeveloperSigningCredential();
            
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}
