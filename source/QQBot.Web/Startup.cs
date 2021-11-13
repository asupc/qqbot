using Autofac;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Model;
using QQBot.Job;
using QQBot.Utils;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace QQBot.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public static ILoggerRepository repository { get; set; }



        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            InstallConfig InstallConfig = InstallConfigHelper.Get();

            services.AddControllers()
            .AddControllersAsServices(); //属性注入必须加上这个      

            if (InstallConfig != null && !string.IsNullOrWhiteSpace(InstallConfig.DBType) && !string.IsNullOrWhiteSpace(InstallConfig.DBAddress))
            {
                var address = "";
                IDbConnection dbConnection;
                if (InstallConfig.DBType.ToLower() == "SQLite".ToLower())
                {
                    address = "Filename=db/" + InstallConfig.DBAddress;
                    dbConnection = new SqliteConnection(address);
                    services.AddDbContext<QQBotDbContext>(options =>
                    {
                        options.UseSqlite(address);
                        options.EnableSensitiveDataLogging(false);
                    });
                }
                else
                {
                    address = InstallConfig.DBAddress;
                    dbConnection = new MySqlConnection(address);
                    services.AddDbContext<QQBotDbContext>(options => options.UseMySQL(address, null));
                }

                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    //q.AddJobAndTrigger<FaskRequestJob>("59 59 * * * ?");
                });

                services.AddTransient((dd) => dbConnection);

                services.AddQuartzHostedService(
                    q => q.WaitForJobsToComplete = true);
            }

            services.AddCors(options =>
            {
                options.AddPolicy("Any", o =>
                {
                    o.AllowAnyOrigin() //允许任何来源的主机访问
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,//是否验证Issuer
                        ValidateAudience = true,//是否验证Audience
                        ValidateLifetime = true,//是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30 * 1000),
                        ValidateIssuerSigningKey = true,//是否验证SecurityKey
                        ValidAudience = "http//:localhost",//Audience
                        ValidIssuer = "http//:localhost",//Issuer，这两项和前面签发jwt的设置一致
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QQBot.JWT.Token.SecurityKey"))//拿到SecurityKey
                    };
                });

            services.AddSingleton(typeof(WSocketClientHelp));
            services.AddTransient(typeof(QLPanelService));
            services.AddTransient(typeof(GoCQHttpHelper));
            services.AddTransient(typeof(MessageProcess));
            services.AddScoped(typeof(QLHttpService));
            services.AddTransient(typeof(JDCookieService));

            //services.addre
            //配置Mvc
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            })
            .AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.ContractResolver = new DefaultContractResolver();
                option.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
                option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                option.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            repository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
        }

        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("Any");
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDefaultFiles();
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Add(".yml", "application/x-yaml");
            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider
            }); ;
            app.UseRouting();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = app.ApplicationServices.GetService<QQBotDbContext>();
                if (dbContext != null)
                {
                    var wsocketClientHelp = app.ApplicationServices.GetRequiredService<WSocketClientHelp>();
                    QLPanelService helper = app.ApplicationServices.GetService<QLPanelService>();
                    await wsocketClientHelp.StartGoCQHttp();

                    var ComandScriptsFile = "./scripts/ComandScripts.json";

                    List<QQBotTask> systemTasks = new List<QQBotTask>();
                    if (File.Exists(ComandScriptsFile))
                    {
                        using (StreamReader commandReader = new StreamReader(ComandScriptsFile))
                        {
                            var text = commandReader.ReadToEnd();
                            systemTasks = JsonConvert.DeserializeObject<List<QQBotTask>>(text);
                        }
                    }
                    var allTasks = dbContext.QQBotTasks.AsNoTracking().ToList();
                    var addTasks = systemTasks.Where(n => !allTasks.Any(m => m.Command == n.Command));

                    if (addTasks.Any())
                    {
                        dbContext.QQBotTasks.AddRange(addTasks);
                        dbContext.SaveChanges();
                    }
                    var tasks = allTasks.Where(n => !string.IsNullOrEmpty(n.Cron)).ToList();
                    if (addTasks.Any(n => !string.IsNullOrEmpty(n.Cron)))
                    {
                        tasks.AddRange(addTasks.Where(n => !string.IsNullOrEmpty(n.Cron)));

                    }
                    foreach (var task in tasks)
                    {
                        if (CronExpression.IsValidExpression(task.Cron))
                        {
                            await task.CreateJob();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("未指定数据库，初始化后重启应用。");
                }
            }
        }
    }
}
;