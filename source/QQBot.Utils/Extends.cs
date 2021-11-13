using QQBot.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace QQBot.Utils
{
    public static class Extends
    {
        public static string Version = "V0.11.12 Beta 1";

        static Extends()
        {
            SMSLoginModels = new List<SMSLoginModel>();
        }

        public static string BrowserPath { get; set; }
        public static List<SMSLoginModel> SMSLoginModels { get; set; }

        public static bool CheckPhoneIsAble(string input)
        {
            if (input.Length != 11)
            {
                return false;
            }
            Regex regex = new Regex("^1\\d{10}$");
            return regex.IsMatch(input);
        }

        public static void Restart()
        {
            try
            {
                var t = RuntimeInformation.OSDescription;
                Thread thread = new Thread(() =>
                {
                    var psi = new ProcessStartInfo("sh", "restart.sh")
                    {
                        RedirectStandardOutput = false
                    };
                    Thread.Sleep(200);
                    Process.Start(psi);
                });
                thread.Start();
            }
            catch
            {
            }
        }
        public static long ToUnix(this DateTime dateTime)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(dateTime - startTime).TotalSeconds; // 相差秒数
            return timeStamp;
        }

        public static DateTime UnixToDateTime(long unixTimeStamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(unixTimeStamp);
            return dt;
        }

        /// <summary>
        /// 中文UrlCode编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Encode(this string str)
        {
            str = str.Decode();
            Regex reg2 = new Regex("^[a-zA-Z0-9_%-]+$");
            //如果包含中文则需要转码
            if (!reg2.Match(str).Success)
            {
                str = HttpUtility.UrlEncode(str);
            }
            return str;
        }

        public static string Decode(this string str)
        {
            var y = HttpUtility.UrlDecode(str);
            if (y == str)
            {
                return str;
            }
            return y.Decode();
        }
    }
}
