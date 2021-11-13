using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Http
{
    public class QLenv
    {
        public string _id { get; set; }

        public string remarks { get; set; }

        public string value { get; set; }

        public string name { get; set; }

        /// <summary>
        /// 状态 1 禁用，0 启用
        /// </summary>
        public int status { get; set; }
    }
}
