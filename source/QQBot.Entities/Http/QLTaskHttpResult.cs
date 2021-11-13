using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Http
{
    public class QLTaskHttpResult
    {
        /// <summary>
        /// 任务
        /// </summary>
        public string command { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public int isDisabled { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 0 运行中
        /// </summary>
        public int status { get; set; }
        
        /// <summary>
        /// 任务id
        /// </summary>
        public string _id { get; set; }

        /// <summary>
        /// cron
        /// </summary>
        public string schedule { get; set; }
    }
}
