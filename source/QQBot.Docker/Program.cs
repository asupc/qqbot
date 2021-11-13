using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QQBot.Docker
{
    class Program
    {
        static void Main(string[] args)
        {
            ShellHelper.Start();
            InstallConfig config = null;
            InstallConfig lastConfig = null;
            WSocketClientHelp wSocketClientHelp = new WSocketClientHelp();
            Task.Run(async () =>
            {
                config = GetInstallConfig();
                if (config != null && !string.IsNullOrEmpty(config.cqhttpWS))
                {
                    await wSocketClientHelp.StartGoCQHttp(config);
                }
                while (true)
                {
                    config = GetInstallConfig();
                    if ((lastConfig == null && config != null) || (lastConfig != null && config != null && lastConfig.cqhttpWS != config.cqhttpWS))
                    {
                        ShellHelper.Kill();
                        ShellHelper.Start();
                    }
                    if (lastConfig != null && config != null && !string.IsNullOrEmpty(config.cqhttpWS) && config.cqhttpWS != lastConfig.cqhttpWS)
                    {
                        await wSocketClientHelp.StartGoCQHttp(config);
                    }
                    lastConfig = config;
                    Thread.Sleep(1000 * 10);
                }
            });
            while (true)
            {
                Thread.Sleep(5000);
            }
        }

        public static InstallConfig GetInstallConfig()
        {
            var path = "/app/linux-x64/config/InstallConfig.xml";
            if (!File.Exists(path))
            {
                Console.WriteLine("QQBot 未初始化，请访问 http://ip:5010/login.html 配置数据库和用户名密码！");
                return null;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(InstallConfig));
            using (StreamReader reader = new StreamReader(path))
            {
                return (InstallConfig)serializer.Deserialize(reader);
            }
        }
    }



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
    }
}