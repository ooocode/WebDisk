using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;
using WebDisk.Models;
using WebDisk.Services;

namespace WebDisk
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(env.ContentRootPath)
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            //    .AddServiceFabricConfiguration() // Add Service Fabric configuration settings.
            //    .AddEnvironmentVariables();
            //Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceFabricConfig();
            services.Configure<KestrelServerOptions>(e => { e.Limits.MaxRequestBodySize = long.MaxValue;});

            services.AddControllers();
            services.AddRazorPages(opt=> {
                opt.Conventions.AuthorizeFolder("/", Permissons.Permisson.WebDisk);
            }).AddRazorRuntimeCompilation();

            // configure basic authentication 
            //services.AddAuthentication(nameof(QueryAuthentication))
            //    .AddScheme<AuthenticationSchemeOptions, QueryAuthentication>(nameof(QueryAuthentication),null)
            //    .AddCookie("Cookies");

            services.AddIdentityServerCenterConnect(new IdentityServerCenterConnect.ConfigurationOptions
            {
                AuthorityUrl = Configuration["Authority"],
                ClientId = Configuration["ClientId"],
                ClientSecret = "",
                GRpcUrl = Configuration["RpcUserClientUri"],
                Scopes = new System.Collections.Generic.List<string> { "openid", "profile" }
            },TimeSpan.FromMinutes(2));
        

            if (env.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddDistributedMemoryCache();
                //services.AddDistributedRedisCache(e => e.Configuration = "127.0.0.1:6379");
            }


            services.AddDbContext<FileDbContext>(option => option.UseMySql(Configuration.GetConnectionString("WebDisk"),opt=>opt.EnableRetryOnFailure()));
            services.BuildServiceProvider().GetService<FileDbContext>().Database.EnsureCreated();

            services.AddHttpContextAccessor();

            services.AddScoped<DirectoryService>();
            services.AddScoped<FileService>();
            services.AddScoped<FileSharedService>();

            //上传的文件保存路径
            var fileSavePhysicPath = Configuration["TusFileSavePhysicPath"];
            if (!System.IO.Directory.Exists(fileSavePhysicPath))
            {
                System.IO.Directory.CreateDirectory(fileSavePhysicPath);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {
                var fileSaveDir = System.IO.Path.Combine(env.WebRootPath, "UploadFiles");
                if (!System.IO.Directory.Exists(fileSaveDir))
                {
                    System.IO.Directory.CreateDirectory(fileSaveDir);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            //必须加 不然有些浏览器没有cookie
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseTus(httpContext => new DefaultTusConfiguration
            {
                // c:\tusfiles is where to store files
                Store = new TusDiskStore(Configuration["TusFileSavePhysicPath"]),
                // On what url should we listen for uploads?
                UrlPath = "/TusFiles",
                //允许上传大小
                MaxAllowedUploadSizeInBytes = int.Parse(Configuration["TusFileSizeOfM"]) * 1024 * 1024,
                MaxAllowedUploadSizeInBytesLong = long.Parse(Configuration["TusFileSizeOfM"]) * 1024 * 1024,
                Events = new Events
                {
                    //OnFileCompleteAsync = async eventContext =>
                    //{
                    //    ITusFile file = await eventContext.GetFileAsync();
                    //},
                    OnAuthorizeAsync = async eventContext =>
                    {
                        bool? isAuthenticated = eventContext.HttpContext?.User?.Identity?.IsAuthenticated;
                        if (!isAuthenticated.HasValue || isAuthenticated.Value == false)
                        {
                            eventContext.FailRequest(System.Net.HttpStatusCode.Unauthorized);
                        }

                        //var userId = eventContext.HttpContext.User?.Claims?.FirstOrDefault(e => e.Type == "sub")?.Value;
                        //if (string.IsNullOrEmpty(userId))
                        //{
                        //    eventContext.FailRequest(System.Net.HttpStatusCode.Unauthorized);
                        //}
                        //else
                        //{
                        //    eventContext.FileId = $"{userId}_{eventContext.FileId}";
                        //}
                    },

                    // OnBeforeCreateAsync = async eventContext =>
                    //{

                    //}

                    OnDeleteCompleteAsync = async eventContext =>
                    {

                    }
                },
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
