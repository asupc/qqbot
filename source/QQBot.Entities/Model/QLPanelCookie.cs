using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace QQBot.Entities.Model
{
    [System.ComponentModel.DataAnnotations.Schema.Table("t_ql_panel_cookie")]
    public class QLPanelCookie : BaseModel
    {
        public string QLPanelId { get; set; }

        public string CookieId { get; set; }

        /// <summary>
        /// 青龙容器中的环境变量id
        /// </summary>
        public string _id { get; set; }

        public QLPanelCookieMode Mode { get; set; }

        [Write(false)]
        [NotMapped]
        public string QLPanelName { get; set; }
    }

    public enum QLPanelCookieMode
    {
        Auto = 1,
        User = 2
    }
}
