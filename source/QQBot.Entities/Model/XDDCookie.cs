using Dapper.Contrib.Extensions;

namespace QQBot.Entities.Model
{
    [Table("jd_cookies")]
    public class XDDCookie
    {
        public long Priority { get; set; }

        public string PtKey { get; set; }

        public string PtPin { get; set; }

        public string Nickname { get; set; }

        public long QQ { get; set; }
    }
}
