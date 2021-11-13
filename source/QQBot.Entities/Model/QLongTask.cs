using System.Collections.Generic;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace QQBot.Entities.Model
{
    /// <summary>
    /// 青龙面板任务
    /// </summary>
    public class QLongTask
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 执行脚本
        /// </summary>
        public string command { get; set; }

        public int pid { get; set; }

        public string schedule { get; set; }

        public List<QLTask> QLTasks { get; set; }

        public string File { get; set; }
    }

    /// <summary>
    /// 青龙任务和青龙面板关系
    /// </summary>
    public class QLTask
    {
        /// <summary>
        /// Cron表达式
        /// </summary>
        public string schedule { get; set; }

        /// <summary>
        /// 0 运行中
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 青龙面板Id
        /// </summary>
        public string QLId { get; set; }

        /// <summary>
        /// 青龙面板名称
        /// </summary>
        public string QLName { get; set; }

        /// <summary>
        /// 青龙面板任务Id
        /// </summary>
        public string _id { get; set; }

        /// <summary>
        /// 1 禁用，0 启用
        /// </summary>
        public int isDisabled { get; set; }
    }
}