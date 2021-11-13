using QQBot.Entities.Model;
using QQBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQBot.Application
{
    public static class JDScriptsTask
    {
        public static async Task ExecFast(JDCookie cookie, string scriptFileName, IEnumerable<Env> envs, int retryCount = 1, int retryDelay = 1000)
        {
            await Task.Run(() =>
            {
                string scriptPath = "./scripts/" + scriptFileName + ".js";
                string jdCookiePath = "./scripts/" + scriptFileName + "/jdCookie.js";
                string envPath = "./scripts/" + scriptFileName + "/env.js";

                if (!File.Exists(scriptPath))
                {
                    return;
                }
                if (!Directory.Exists("./scripts/" + scriptFileName))
                {
                    Directory.CreateDirectory("./scripts/" + scriptFileName);
                }
                if (!File.Exists(jdCookiePath))
                {
                    File.Copy("./scripts/jdCookie.js", jdCookiePath);
                }
                if (File.Exists(envPath))
                {
                    File.Delete(envPath);
                }
                File.Create(envPath).Close();

                StringBuilder envText = new StringBuilder();
                foreach (var item in envs.GroupBy(n => n.Name))
                {
                    envText.AppendLine($"process.env.{item.Key}=\"{string.Join('&', item.ToList().Select(n => n.Value)).TrimEnd('&')}\"");
                }
                envText.AppendLine($"process.env.JD_COOKIE=\"{cookie.CookieToString()}\"");
                using (StreamWriter streamWriter1 = new StreamWriter(envPath))
                {
                    streamWriter1.WriteLine(envText);
                    streamWriter1.Flush();
                }
                while (true)
                {
                    if (DateTime.Now.Second == 59 && DateTime.Now.Millisecond >= 870)
                    {
                        break;
                    }
                    Thread.Sleep(5);
                }
                for (int i = 0; i < retryCount; i++)
                {
                    var psi = new ProcessStartInfo("node", scriptPath)
                    {
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };
                    //启动
                    var proc = Process.Start(psi);
                    using (var sr = proc.StandardOutput)
                    {
                        while (!sr.EndOfStream)
                        {
                            var t = sr.ReadLine();
                            Console.WriteLine(t);
                            Thread.Sleep(10);
                        }
                        if (!proc.HasExited)
                        {
                            proc.Kill();
                        }
                    }
                    if (i + 1 < retryCount)
                    {
                        Thread.Sleep(retryDelay);
                    }
                }
            });
        }



        public static async Task<string> ExecTask(IEnumerable<JDCookie> cookies, QQBotTask task, IEnumerable<Env> envs, int count = 1, int delay = 2000)
        {
            await Task.Run(() =>
            {
                var taskId = Guid.NewGuid().ToString().Replace("-", "");
                var taskFile = $"./scripts/{taskId}.js";
                var taskDirectory = $"./scripts/{taskId}";
                var jdCookieFile = "./scripts/jdCookie.js";

                Directory.CreateDirectory(taskDirectory);
                var jsFile = $"./scripts/{task.FileName}";
                if (!File.Exists(jsFile))
                {
                    Console.WriteLine($"任务执行脚本{jsFile}不存在！");
                    return null;
                }

                var taskText = "";

                using (StreamReader streamReader = new StreamReader(jsFile))
                {
                    taskText = streamReader.ReadToEnd();
                    taskText = taskText.Replace("./jdCookie.js", $"./{taskId}/jdCookie.js");
                    taskText = taskText.Replace("./sendNotify", $"./{taskId}/sendNotify");
                    taskText = taskText.Replace("./env.js", $"./{taskId}/env.js");
                }
                File.Copy(jdCookieFile, $"./scripts/{taskId}/jdCookie.js");
                File.Copy("./scripts/sendNotify.js", $"./scripts/{taskId}/sendNotify.js");
                if (File.Exists(taskFile))
                {
                    File.Delete(taskFile);
                }
                File.Create(taskFile).Close();
                using (StreamWriter streamWriter = new StreamWriter(taskFile))
                {
                    streamWriter.WriteLine(taskText);
                    streamWriter.Flush();
                }
                StringBuilder envText = new StringBuilder();
                var systemEnvs = new List<Env>();
                systemEnvs.Add(new Env
                {
                    Name = "ServicePort",
                    Value = string.IsNullOrEmpty(InstallConfigHelper.Get().Port) ? "5010" : InstallConfigHelper.Get().Port
                });

                foreach (var item in envs.GroupBy(n => n.Name))
                {
                    envText.AppendLine($"process.env.{item.Key}=\"{string.Join('&', item.ToList().Select(n => n.Value)).TrimEnd('&')}\"");
                }
                StringBuilder logs = new StringBuilder();
                foreach (var item in cookies.GroupBy(n => n.QQ))
                {
                    var env = $"./scripts/{taskId}/env.js";
                    if (File.Exists(env))
                    {
                        File.Delete(env);
                    }
                    File.Create(env).Close();

                    systemEnvs.Add(new Env
                    {
                        Name = "qq",
                        Value = item.Key.ToString()
                    });
                    using (StreamWriter streamWriter1 = new StreamWriter(env))
                    {
                        streamWriter1.WriteLine($"process.env.JD_COOKIE=\"{string.Join('&', item.ToList().Select(n => n.CookieToString())).TrimEnd('&')}\"");
                        if (item.Key > 0 && task.EnablePush)
                        {
                            streamWriter1.WriteLine($"process.env.GOBOT_URL=\"{InstallConfigHelper.Get().cqhttpHttp}/send_private_msg?user_id={item.Key}\"");
                        }
                        if (!string.IsNullOrEmpty(envText.ToString()))
                        {
                            streamWriter1.WriteLine(envText);
                        }
                        foreach (var env1 in systemEnvs)
                        {
                            streamWriter1.WriteLine($"process.env.{env1.Name}=\"{env1.Value}\"");
                        }
                        streamWriter1.Flush();
                    }

                    for (int i = 0; i < count; i++)
                    {
                        var psi = new ProcessStartInfo("node", taskFile)
                        {
                            RedirectStandardOutput = true,
                            StandardOutputEncoding = Encoding.UTF8
                        };
                        //启动
                        var proc = Process.Start(psi);
                        using (var sr = proc.StandardOutput)
                        {
                            while (!sr.EndOfStream)
                            {
                                var t = sr.ReadLine();
                                Console.WriteLine(t);
                                if (!string.IsNullOrEmpty(t))
                                    logs.AppendLine(t);
                                Thread.Sleep(50);
                            }
                            if (!proc.HasExited)
                            {
                                proc.Kill();
                            }
                        }
                        Thread.Sleep(delay);
                    }

                }
                File.Delete(taskFile);
                Directory.Delete(taskDirectory, true);
                return logs.ToString();
            });
            return null;
        }

        public static async Task CheckCookie(List<JDCookie> cookies, bool IsAdd)
        {
            List<Env> envs = new List<Env>();
            envs.Add(new Env
            {
                Name = "IsAdd",
                Value = IsAdd ? "true" : "false"
            });
            await ExecTask(cookies, new QQBotTask
            {
                FileName = "qqbot_checkCookie.js",
                EnablePush = true
            }, envs);
        }
    }
}