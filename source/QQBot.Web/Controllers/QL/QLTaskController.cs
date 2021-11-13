using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class QLTaskController : BaseController
    {

        private QLHttpService QLHttpHelper;

        public QLTaskController(QLHttpService httpHelper)
        {
            QLHttpHelper = httpHelper;
        }

        [HttpGet]
        public async Task<ResultModel<List<QLongTask>>> Get([FromQuery] QLTaskQuery query)
        {

            var qls = await QQBotDbContext.Instance.QLConfigs.AsNoTracking().Where(n => string.IsNullOrEmpty(query.ContainerId) || n.Id == query.ContainerId).ToListAsync();

            List<Task> tasks = new List<Task>();

            List<QLongTask> qLongTasks = new List<QLongTask>();
            foreach (var item in qls)
            {
                var results = QLHttpHelper.GetQLTasks(item, query.Key);
                foreach (var result in results)
                {
                    try
                    {
                        var t = qLongTasks.SingleOrDefault(n => n.command == result.command);
                        if ((query.isDisabled == null || query.isDisabled == result.isDisabled) && (query.status == null || query.status == result.status))
                        {
                            var qlt = new QLTask
                            {
                                QLId = item.Id,
                                isDisabled = result.isDisabled,
                                schedule = result.schedule,
                                status = result.status,
                                _id = result._id,
                                QLName = item.Name
                            };
                            if (t != null)
                            {
                                t.QLTasks.Add(qlt);
                            }
                            else
                            {
                                qLongTasks.Add(new QLongTask
                                {
                                    command = result.command,
                                    schedule = result.schedule,
                                    name = result.name,
                                    QLTasks = new List<QLTask> { qlt }
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            qLongTasks.ForEach((d) =>
            {
                d.QLTasks = d.QLTasks.OrderBy(n => n.QLName).ToList();
            });
            return ResultModel<List<QLongTask>>.Success(qLongTasks);
        }


        /// <summary>
        /// 运行任务
        /// </summary>
        /// <returns></returns>
        [HttpPost("run")]
        public async Task<ResultModel> Run([FromBody] List<QLTaskRun> taskRuns)
        {
            return await ChangeTaskStatus("run", taskRuns);
        }

        [HttpPost("stop")]
        public async Task<ResultModel> Stop([FromBody] List<QLTaskRun> taskRuns)
        {
            return await ChangeTaskStatus("stop", taskRuns);
        }


        /// <summary>
        /// 运行任务
        /// </summary>
        /// <returns></returns>
        [HttpPost("enable")]
        public async Task<ResultModel> Enable([FromBody] List<QLTaskRun> taskRuns)
        {
            return await ChangeTaskStatus("enable", taskRuns);
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <returns></returns>
        [HttpPost("disable")]
        public async Task<ResultModel> Disable([FromBody] List<QLTaskRun> taskRuns)
        {
            return await ChangeTaskStatus("disable", taskRuns);
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete")]
        public async Task<ResultModel> Delete([FromBody] List<QLTaskRun> taskRuns)
        {
            var qls = await QQBotDbContext.Instance.QLConfigs.AsNoTracking().Where(n => taskRuns.Select(n => n.QLId).Contains(n.Id)).ToListAsync();
            List<Task> tasks = new List<Task>();
            foreach (var item in qls)
            {
                QLHttpHelper.DeleteTask(item, taskRuns.FirstOrDefault(n => n.QLId == item.Id).TaskIds);
            }
            return ResultModel.Success();
        }

        /// <summary>
        /// 修改青龙任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<ResultModel<bool>> Update([FromBody] QLongTask task)
        {
            var qls = await QQBotDbContext.Instance.QLConfigs.AsNoTracking().Where(n => task.QLTasks.Select(n => n.QLId).Contains(n.Id)).ToListAsync();
            List<Task> tasks = new List<Task>();
            foreach (var item in qls)
            {
                tasks.Add(Task.Run(() =>
                {
                    var temps = task.QLTasks.Where(n => n.QLId == item.Id).ToList();
                    temps.ForEach((temp) =>
                    {
                        QLHttpHelper.UpdateTask(item, new
                        {
                            name = task.name,
                            schedule = temp.schedule,
                            command = task.command,
                            _id = temp._id
                        });
                    });
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return ResultModel<bool>.Success(true);
        }

        [HttpGet("log/{qlId}/{scriptId}")]
        public ResultModel<string> Log([FromRoute] string qlId, [FromRoute] string scriptId)
        {
            var ql = QQBotDbContext.Instance.QLConfigs.AsNoTracking().SingleOrDefault(n => n.Id == qlId);
            if (ql != null)
            {
                return ResultModel<string>.Success(QLHttpHelper.ScriptLog(ql, scriptId));
            }
            return ResultModel<string>.Success("");
        }

        /// <summary>
        /// 添加青龙任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<ResultModel<bool>> Add([FromBody] QLongTask task)
        {
            var qls = await QQBotDbContext.Instance.QLConfigs.AsNoTracking().Where(n => task.QLTasks.Select(n => n.QLId).Contains(n.Id)).ToListAsync();
            List<Task> tasks = new List<Task>();
            foreach (var item in qls)
            {
                tasks.Add(Task.Run(() =>
                {
                    var temps = task.QLTasks.Where(n => n.QLId == item.Id).ToList();
                    string sriptsFile = "./scripts/ql/" + task.File;
                    if (System.IO.File.Exists(sriptsFile))
                    {
                        Console.WriteLine($"开始上传{task.File}到容器：{item.Name}。");
                        using (StreamReader streamReader = new StreamReader(sriptsFile))
                        {
                            QLHttpHelper.UpdateScript(item, task.File, streamReader.ReadToEnd());
                        }
                    }

                    temps.ForEach((temp) =>
                    {
                        QLHttpHelper.AddTask(item, new
                        {
                            name = task.name,
                            schedule = task.schedule,
                            command = task.command
                        });
                    });
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return ResultModel<bool>.Success(true);
        }

        private async Task<ResultModel> ChangeTaskStatus(string status, [FromBody] List<QLTaskRun> taskRuns)
        {
            var qls = await QQBotDbContext.Instance.QLConfigs.AsNoTracking().Where(n => taskRuns.Select(n => n.QLId).Contains(n.Id)).ToListAsync();
            List<Task> tasks = new List<Task>();
            foreach (var item in qls)
            {
                QLHttpHelper.ChangeTaskStatus(item, taskRuns.FirstOrDefault(n => n.QLId == item.Id).TaskIds, status);
            }
            return ResultModel.Success();
        }
    }
}
