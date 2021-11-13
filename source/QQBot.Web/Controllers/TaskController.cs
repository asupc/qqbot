using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQBot.DB;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using QQBot.Job;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class TaskController : BaseController
    {
        IDbConnection dbConnection;
        BaseRepository<QQBotTask> QQBotTaskRepository;
        public TaskController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
            QQBotTaskRepository = BaseRepository<QQBotTask>.Init(dbConnection);
        }

        [HttpGet]
        public ResultModel<IEnumerable<QQBotTask>> Index([FromQuery] string key)
        {
            return new ResultModel<IEnumerable<QQBotTask>>
            {
                Data = QQBotDbContext.Instance.QQBotTasks.AsNoTracking().Where(n => string.IsNullOrEmpty(key) || n.Command.Contains(key) || n.FileName.Contains(key) || n.Name.Contains(key))
            };
        }

        [HttpGet("{id}")]
        public ResultModel<QQBotTask> GetById([FromRoute] string id)
        {
            return new ResultModel<QQBotTask>
            {
                Data = QQBotTaskRepository.GetById(id)
            };
        }

        [HttpPost]
        public async Task<ResultModel<bool>> Update([FromBody] QQBotTask task)
        {
            var cron = CronExpression.IsValidExpression(task.Cron);
            if (!string.IsNullOrEmpty(task.Cron) && !cron)
            {
                return ResultModel<bool>.Error("您填写了定时执行表达式，但该值无法通过验证，请修改后保存。");
            }
            var t = QQBotTaskRepository.GetById(task.Id);
            if (t != null)
            {
                if (t.Name != task.Name && QQBotTaskRepository.IsExist(new { Name = task.Name }))
                {
                    return ResultModel<bool>.Error("您修改了指令，该指令重复。");
                }
                await task.DeleteJob();
                QQBotTaskRepository.Update(new { task.Id }, new
                {
                    task.Name,
                    task.Enable,
                    task.Cron,
                    task.ExecAllCookie,
                    task.EnableConc,
                    task.Command,
                    task.FileName,
                    task.EnablePush,
                    task.MaxCount,
                    task.ConcCount
                });
            }
            else
            {
                if (QQBotTaskRepository.IsExist(new { Name = task.Name }))
                {
                    return ResultModel<bool>.Error("执行指令重复，请修改后保存。");
                }
                task.Id = Guid.NewGuid().ToString().Replace("-", "");
                task.TaskSource = TaskSource.QQBot;
                QQBotTaskRepository.Add(task);
            }
            await task.CreateJob();
            return new ResultModel<bool> { Data = true };
        }

        [HttpDelete("{id}")]
        public ResultModel Delete([FromRoute] string id)
        {
            QQBotTaskRepository.DeleteById(id);
            return new ResultModel();
        }
    }
}