using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QQBot.Docker
{
    public static class ShellHelper
    {
        public static void Start()
        {
            try
            {
                string logFile = "../linux-x64/nohup.out";
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
                var psi = new ProcessStartInfo("sh", "/app/Docker/start.sh");
                var proc = Process.Start(psi);
                proc.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine("Start:" + e.Message);
            }
        }

        public static void Kill()
        {
            var proceess = Process.GetProcesses();
            foreach (var p in proceess.Where(n => n.ProcessName.ToLower().Contains("QQBot.Web".ToLower())))
            {
                Console.WriteLine("结束进程：" + p.ProcessName);
                p.Kill();
            }
        }


        public static void Update()
        {
            Kill();
            try
            {
                var psi = new ProcessStartInfo("sh", "/app/Docker/update.sh");
                var proc = Process.Start(psi);
                proc.WaitForExit();
                Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Update:" + e.Message);
            }
        }
    }
}
