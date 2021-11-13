using QQBot.Entities.Model;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace QQBot.Entities.Config
{
    [Table("t_QLConfig")]
    public class QLConfig : BaseModel
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        //public string UserName { get; set; }
        //public string PassWord { get; set; }

        public string TokeType { get; set; }

        public int CookieCount { get; set; }

        public int Weigth { get; set; }
        public int MaxCount { get; set; }

        public string Token { get; set; }

        public bool EnableAll { get; set; }

        public DateTime? TokenExprise { get; set; }
    }
}
