using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities
{
    public class QLTaskRun
    {
        /// <summary>
        /// 青龙配置Id
        /// </summary>
        public string QLId { get; set; }

        /// <summary>
        /// 任务Id
        /// </summary>
        public List<string> TaskIds { get; set; }
    }
}
