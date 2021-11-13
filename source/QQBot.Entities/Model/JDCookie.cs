using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace QQBot.Entities.Model
{
    /// <summary>
    /// 京东Cookie
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.Table("t_JDCookie")]
    public class JDCookie : BaseModel
    {
        //public bool Top { get; set; }

        //public string ContainerId { get; set; }

        public int Priority { get; set; } = 0;

        public string Remark { get; set; }

        ///// <summary>
        ///// 青龙面板env id
        ///// </summary>
        //public string QLId { get; set; }

        public string PTKey { get; set; }

        public string PTPin { get; set; }

        public string nickname { get; set; }

        ///// <summary>
        ///// 是否plus会员
        ///// </summary>
        //public bool isPlusVip { get; set; }

        ///// <summary>
        ///// 是否实名
        ///// </summary>
        //public bool isRealNameAuth { get; set; }

        /// <summary>
        /// 绑定QQ
        /// </summary>
        public long QQ { get; set; }

        ///// <summary>
        ///// 京豆数量
        ///// </summary>
        //public long beanNum { get; set; }

        ///// <summary>
        ///// 过期时间
        ///// </summary>
        //public DateTime? ExpriseTime { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Available { get; set; }

        //public string userLevel { get; set; }

        //public string levelName { get; set; }

        [Write(false)]
        public virtual IEnumerable<QLPanelCookie> QLPanelCookies { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 京东农场助力码
        /// </summary>
        public string JDNCShareCode { get; set; }

        /// <summary>
        /// 京东萌宠助力码
        /// </summary>
        public string JDMCShareCode { get; set; }

        /// <summary>
        /// 种豆得豆助力码
        /// </summary>
        public string ZDDDShareCode { get; set; }

        /// <summary>
        /// 东东工厂助力码
        /// </summary>
        public string DDGCShareCode { get; set; }

        /// <summary>
        /// 惊喜工厂助力码
        /// </summary>
        public string JXGCShareCode { get; set; }

        /// <summary>
        /// 惊喜农场助力码
        /// </summary>
        public string JXNCShareCode { get; set; }

        /// <summary>
        /// 闪购盲盒助力码
        /// </summary>
        public string SGMHShareCode { get; set; }

        /// <summary>
        /// 财富岛助力码
        /// </summary>
        public string CFDShareCode { get; set; }

        /// <summary>
        /// 签到领现金助力码
        /// </summary>
        public string QDLXJShareCode { get; set; }

    }
}