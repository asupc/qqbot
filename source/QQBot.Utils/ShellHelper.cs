using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQBot.Utils
{
    public static class ShellHelper
    {

        public static void Upate()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    ExecuteShell("git", "checkout .");
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        ExecuteShell("sh", "update-linux.sh");
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        ExecuteShell("cmd", "update-windows.bat");
                    }
                }
                catch (Exception e)
                {
                }
            });
            thread.Start();
        }



        public static void ExecuteShell(string command, string path)
        {
            try
            {
                if (path.Contains("windows"))
                {
                    Thread thread = new Thread(() =>
                    {
                        var psi = new ProcessStartInfo(path);
                        psi.CreateNoWindow = true;
                        Process.Start(psi);
                    });
                    thread.Start();
                }
                else
                {
                    var psi = new ProcessStartInfo(command, path);
                    //启动
                    var proc = Process.Start(psi);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(command + ":" + path + ":" + e.Message);
            }
        }
    }
}
