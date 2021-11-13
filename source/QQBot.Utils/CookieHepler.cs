using QQBot.Entities.Http;
using QQBot.Entities.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace QQBot.Utils
{
    public static class CookieHepler
    {
        /// <summary>
        /// cookie 字符串自动提取pt_key ,pt_pin，并对 pt_pin 进行转码
        /// </summary>
        public static List<JDCookie> ConvertCookie(string cookie)
        {
            cookie = cookie.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            var list = cookie.Split("&");
            List<JDCookie> results = new List<JDCookie>();
            foreach (var s in list)
            {
                Regex regKey = new Regex("pt_key=(?<pt_key>.*?);");
                Regex regPin = new Regex("pt_pin=(?<pt_pin>.*?);");
                Match matchKey = regKey.Match(s);
                Match matchPin = regPin.Match(s);
                if (matchKey.Success && matchPin.Success)
                {
                    var pt_pin = matchPin.Groups["pt_pin"].Value;
                    pt_pin = pt_pin.Encode();
                    var c = new JDCookie
                    {
                        PTKey = matchKey.Groups["pt_key"].Value,
                        PTPin = pt_pin
                    };
                    Regex regQQ = new Regex("qq=(?<qq>.*?);");
                    Match matchQQ = regQQ.Match(s.ToLower());
                    if (matchQQ.Success)
                    {
                        try
                        {
                            c.QQ = Convert.ToInt64(matchQQ.Groups["qq"].Value);
                        }
                        catch
                        {
                            
                        }
                    }
                    results.Add(c);
                }
            }
            return results;
        }


        public static string CookieToString(this (string, string) data)
        {
            return $"pt_key={data.Item1};pt_pin={data.Item2}";
        }

        public static string CookieToString(this JDCookie cookie)
        {
            if (cookie == null)
            {
                return "";
            }
            cookie.PTPin = cookie.PTPin.Encode();
            return $"pt_key={cookie.PTKey}; pt_pin={cookie.PTPin};";
        }

        public static string CookieToString(this QLenv cookie)
        {
            return ConvertCookie(cookie.value)[0].CookieToString();
        }
    }
}
