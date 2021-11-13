using System;

namespace QQBot.Entities
{
    public class SMSLoginModel
    {
        public long QQ { get; set; }

        public string Tel { get; set; }

        public string VerifyCode { get; set; }

        public string Cookie { get; set; }

        public SMSLoginStatus SMSLoginStatus { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public enum SMSLoginStatus
    {
        Tel = 1,
        Code = 2
    }
}
