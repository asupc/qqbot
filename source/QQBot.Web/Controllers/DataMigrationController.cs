using Microsoft.AspNetCore.Mvc;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using QQBot.Utils;
using QQBot.Web.MigrationModel;
using System;
using System.Linq;
using System.Text;

namespace QQBot.Web.Controllers
{
    public class DataMigrationController : BaseController
    {
        [HttpPost]
        public ResultModel<string> DataMigration([FromBody] InstallConfig installConfig)
        {
            StringBuilder stringBuilder = new StringBuilder("迁移成功，请重新启动QQBot！");

            if (installConfig.DBType.ToLower() == "sqlite" && System.IO.File.Exists("./db/" + installConfig.DBAddress))
            {
                return ResultModel<string>.Error($"db文件夹中已存在名称为{installConfig.DBAddress}的文件，请修改后重试！");
            }
            var config = new InstallConfig
            {
                DBType = installConfig.DBType,
                DBAddress = installConfig.DBAddress
            };
            QQBotDbContext newDBContext = null;
            try
            {
                newDBContext = new QQBotDbContext(config, true);
            }
            catch (Exception e)
            {
                return ResultModel<string>.Error("重新构建数据库失败，请修改配置后重试！\r\n" + e.Message);
            }
            try
            {
                var mCookie = BaseRepository<MJDCookie>.Instance.GetAll();
                newDBContext.JDCookies.AddRange(mCookie.Select(n => new Entities.Model.JDCookie
                {
                    Id = n.Id,
                    Available = n.Available,
                    CreateTime = n.CreateTime,
                    PTKey = n.PTKey,
                    PTPin = n.PTPin,
                    nickname = n.nickname,
                    Priority = n.Priority,
                    Remark = n.Remark,
                    QQ = n.QQ
                }));
                stringBuilder.AppendLine($"<br/>迁移Cookie数据成功！条数：{newDBContext.SaveChanges()}条");
            }
            catch (Exception e)
            {
                return ResultModel<string>.Error("迁移Cookie数据失败，错误信息！\r\n" + e.Message);
            }
            try
            {
                var envs = BaseRepository<Env>.Instance.GetAll();
                newDBContext.Envs.AddRange(envs);
                stringBuilder.AppendLine($"<br/>迁移环境变量数据成功！条数：{newDBContext.SaveChanges()}条");
            }
            catch (Exception e)
            {
                stringBuilder.AppendLine($"<br/>迁移环境变量数据失败！错误信息：{e.Message}");
            }
            try
            {
                var commands = BaseRepository<Command>.Instance.GetAll();
                newDBContext.Commands.AddRange(commands);
                stringBuilder.AppendLine($"<br/>迁移快捷回复数据成功！条数：{newDBContext.SaveChanges()}条");
            }
            catch (Exception e)
            {
                stringBuilder.AppendLine($"<br/>迁移快捷回复数据失败！错误信息：{e.Message}");
            }
            try
            {
                var QLConfigs = BaseRepository<QLConfig>.Instance.GetAll();
                newDBContext.QLConfigs.AddRange(QLConfigs);
                stringBuilder.AppendLine($"<br/>迁移青龙面板数据成功！条数：{newDBContext.SaveChanges()}条");
            }
            catch (Exception e)
            {
                stringBuilder.AppendLine($"<br/>迁移青龙面板数据失败！错误信息：{e.Message}");
            }
            try
            {
                var QLPanelCookies = BaseRepository<QLPanelCookie>.Instance.GetAll();
                newDBContext.QLPanelCookies.AddRange(QLPanelCookies);
                stringBuilder.AppendLine($"<br/>迁移Cookie和容器关系数据成功！条数：{newDBContext.SaveChanges()}条");
            }
            catch (Exception e)
            {
                stringBuilder.AppendLine($"<br/>迁移Cookie和容器关系数据失败！错误信息：{e.Message}");
            }
            try
            {
                var QQBotTasks = BaseRepository<QQBotTask>.Instance.GetAll();
                newDBContext.QQBotTasks.AddRange(QQBotTasks);
                stringBuilder.AppendLine($"<br/>迁移脚本指令数据成功！条数：{newDBContext.SaveChanges()}条");
            }
            catch (Exception e)
            {
                stringBuilder.AppendLine($"<br/>迁移脚本指令数据失败！错误信息：{e.Message}");
            }
            var n = InstallConfigHelper.Get();
            n.DBAddress = installConfig.DBAddress;
            n.DBType = installConfig.DBType;
            InstallConfigHelper.Set(n);
            return ResultModel<string>.Success(stringBuilder.ToString());
        }
    }
}
