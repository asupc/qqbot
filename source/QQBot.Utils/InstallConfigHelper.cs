using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using QQBot.Entities.Config;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Serialization;

namespace QQBot.Utils
{
    public static class InstallConfigHelper
    {
        public static string installConfigPath = "config/InstallConfig.xml";

        public static void Set(InstallConfig config)
        {
            XmlHelper<InstallConfig>.Set(config, installConfigPath);
        }

        public static InstallConfig Get()
        {
            return XmlHelper<InstallConfig>.Get(installConfigPath);
        }

        public static IDbConnection GetDbConnection
        {
            get
            {
                var installConfig = Get();
                IDbConnection dbConnection;
                string address;
                if (installConfig.DBType.ToLower() == "SQLite".ToLower())
                {
                    address = "Filename=db/" + installConfig.DBAddress;
                    dbConnection = new SqliteConnection(address);
                }
                else
                {
                    address = installConfig.DBAddress;
                    dbConnection = new MySqlConnection(address);
                }
                return dbConnection;
            }
        }
    }


    public static class SystemCommandHelper
    {
        public static void Set(List<SystemCommand> commands)
        {
            XmlHelper<List<SystemCommand>>.Set(commands, "config/CommandConfig.xml");
        }

        public static List<SystemCommand> Get()
        {
            var commands = XmlHelper<List<SystemCommand>>.Get("config/CommandConfig.xml");
            if (commands == null || commands.Count == 0)
            {
                commands = new List<SystemCommand>();
                commands.Add(new SystemCommand
                {
                    Key = "我的账号",
                    Command = "我的账号"
                });

                commands.Add(new SystemCommand
                {
                    Key = "删除账号",
                    Command = "删除账号"
                });
                commands.Add(new SystemCommand
                {
                    Key = "手机号登录",
                    Command = "手机号登录"
                });

                commands.Add(new SystemCommand
                {
                    Key = "更新QQBot",
                    Command = "更新QQBot",
                    IsManager = true
                });
                commands.Add(new SystemCommand
                {
                    Key = "清理过期Cookie",
                    Command = "清理过期Cookie",
                    IsManager = true
                });
                commands.Add(new SystemCommand
                {
                    Key = "检查Cookie",
                    Command = "检查Cookie",
                    IsManager = true
                });
                commands.Add(new SystemCommand
                {
                    Key = "重新分配",
                    Command = "重新分配",
                    IsManager = true
                });
                commands.Add(new SystemCommand
                {
                    Key = "同步Cookie",
                    Command = "同步Cookie",
                    IsManager = true
                });
                commands.Add(new SystemCommand
                {
                    Key = "消息推送",
                    Command = "消息推送",
                    IsManager = true
                });
                commands.Add(new SystemCommand
                {
                    Key = "统计Cookie",
                    Command = "统计Cookie",
                    IsManager = true
                });
                commands.Add(new SystemCommand
                {
                    Key = "导出Cookie",
                    Command = "导出Cookie",
                    IsManager = true
                });
                Set(commands);
            }

            return commands;
        }
    }


    public class SystemCommand
    {
        public string Key { get; set; }

        public string Command { get; set; }

        public bool IsManager { get; set; }
    }


    public static class XmlHelper<T>
    {
        public static T Get(string path)
        {
            if (!File.Exists(path))
            {
                return default;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamReader reader = new StreamReader(path))
            {
                var config = (T)serializer.Deserialize(reader);
                return config;
            }
        }


        public static void Set(T config, string path)
        {
            XmlSerializer serializer = new XmlSerializer(config.GetType());
            string content = string.Empty;
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, config);
                content = writer.ToString();
            }
            using (StreamWriter stream_writer = new StreamWriter(path))
            {
                stream_writer.Write(content);
            }
        }
    }

}
