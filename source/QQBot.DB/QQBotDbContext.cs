using Dapper;
using log4net;
using Microsoft.EntityFrameworkCore;
using QQBot.Entities.Config;
using QQBot.Entities.Model;
using QQBot.Utils;
using System;
using System.Linq;

namespace QQBot.DB
{
    public class QQBotDbContext : DbContext
    {
        private readonly ILog log = LogManager.GetLogger("NETCoreRepository", typeof(QQBotDbContext));
        public QQBotDbContext(DbContextOptions<QQBotDbContext> options) : base(options)
        {
            var initDatabase = false;
            try
            {
                initDatabase = Database.EnsureCreated();
            }
            catch (Exception e)
            {
                log.Error("数据库初始化失败，请检查数据库配置后重启。", e);
            }
            try
            {
                if (!initDatabase && Database.GetPendingMigrations().Any())
                {
                    Database.Migrate(); //执行迁移
                }
            }
            catch(Exception e)
            {
                foreach (var item in Database.GetPendingMigrations())
                {
                    try
                    {
                        Database.GetDbConnection().Execute("insert into __EFMigrationsHistory values (@MigrationId,@ProductVersion)", new { MigrationId = item, ProductVersion = "5.0.9" });
                    }
                    catch { }
                }
            }
        }

        private readonly InstallConfig installConfig;



        public QQBotDbContext(InstallConfig installConfig,bool migration = false)
        {
            this.installConfig = installConfig;

            if (migration)
            {
                Database.EnsureCreated();
            }
        }

        public static QQBotDbContext Instance
        {
            get
            {
                return new QQBotDbContext(InstallConfigHelper.Get());
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (installConfig == null)
            {
                return;
            }
            if (installConfig.DBType.ToLower() == "SQLite".ToLower())
            {
                var address = "Filename=db/" + installConfig.DBAddress;
                optionsBuilder.UseSqlite(address);
            }
            else
            {
                var address = installConfig.DBAddress;
                optionsBuilder.UseMySQL(address, null);
            }
        }

        public int Delete<T>(T data) where T : class
        {
            Entry(data).State = EntityState.Deleted;
            return SaveChanges();
        }

        public DbSet<JDCookie> JDCookies { get; set; }

        public DbSet<Command> Commands { get; set; }

        public DbSet<QLConfig> QLConfigs { get; set; }

        public DbSet<QQBotTask> QQBotTasks { get; set; }

        public DbSet<QLPanelCookie> QLPanelCookies { get; set; }

        public DbSet<TaskConc> TaskConcs { get; set; }

        public DbSet<Env> Envs { get; set; }
    }
}
