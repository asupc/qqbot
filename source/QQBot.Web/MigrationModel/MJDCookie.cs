using System;

namespace QQBot.Web.MigrationModel
{
    [System.ComponentModel.DataAnnotations.Schema.Table("t_JDCookie")]
    public class MJDCookie
    {
        public string Id { get; set; }

        public int Priority { get; set; } = 0;

        public string Remark { get; set; }

        public string PTKey { get; set; }

        public string PTPin { get; set; }

        public string nickname { get; set; }

        public long QQ { get; set; }

        public bool Available { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
