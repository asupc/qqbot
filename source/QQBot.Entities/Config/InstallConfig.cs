using System;
using System.Collections.Generic;

namespace QQBot.Entities.Config
{
    [Serializable]
    public class InstallConfig
    {
        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string DBType { get; set; }

        public string DBAddress { get; set; }

        public string Port { get; set; }

        public string cqhttpWS { get; set; }

        public string cqhttpHttp { get; set; }
        public string ManagerQQ { get; set; }
        public string Groups { get; set; }

        public bool AddFriend { get; set; }
        public string AddFriendMessage { get; set; }
        public bool AddGroup { get; set; }
        public string AddGroupMessage { get; set; }

        public string SMSService { get; set; }

        public string SMSBlackQQ { get; set; }
        
        public bool SMSAllowGroup { get; set; }

        public int SMSMaxCount { get; set; }
    }
}