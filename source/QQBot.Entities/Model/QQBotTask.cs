using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace QQBot.Entities.Model
{
    [Table("t_qqbot_task")]
    public class QQBotTask : BaseModel
    {
        public string Command { get; set; }

        public int ConcCount { get; set; }

        public string Name { get; set; }

        public string Cron { get; set; }

        public string FileName { get; set; }

        public bool EnableConc { get; set; }

        public bool Enable { get; set; }

        public int MaxCount { get; set; }

        public TaskSource TaskSource { get; set; }

        public bool ExecAllCookie { get; set; }

        [Write(false)]
        public IEnumerable<TaskConc> TaskConcs { get; set; }

        public bool EnablePush { get; set; }
    }

    public enum TaskSource
    {
        QL = 1,
        QQBot = 2
    }

    [Table("t_task_conc")]
    public class TaskConc : BaseModel
    {
        public string QQBotTaskId { get; set; }

        public string CookieId { get; set; }
    }
}
