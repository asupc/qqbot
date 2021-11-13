using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Http
{
    public class RedPacket
    {
        /// <summary>
        /// 所有红包
        /// </summary>
        public decimal Total { get; set; }


        /// <summary>
        /// 京喜红包
        /// </summary>
        public decimal Jx { get; set; }

        /// <summary>
        /// 极速红包
        /// </summary>
        public decimal Js { get; set; }

        /// <summary>
        /// 健康红包
        /// </summary>
        public decimal Jk { get; set; }

        /// <summary>
        /// 京东红包
        /// </summary>
        public decimal Jd { get; set; }
    }
}
