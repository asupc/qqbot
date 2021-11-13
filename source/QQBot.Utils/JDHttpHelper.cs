using Newtonsoft.Json;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace QQBot.Utils
{
    //public static class JDHttpHelper
    //{

    //    public static Dictionary<string, string> headers { get; set; }

    //    static JDHttpHelper()
    //    {
    //        headers = new Dictionary<string, string>();
    //        headers.Add("Accept", "*/*");
    //        headers.Add("Accept-Language", "zh-cn,");
    //        headers.Add("Connection", "keep-alive,");
    //        headers.Add("Host", "me-api.jd.com");
    //    }

    //    public static Dictionary<string, string> InitHeaders(string cookie)
    //    {
    //        if (headers.ContainsKey("Cookie"))
    //        {
    //            headers.Remove("Cookie");
    //        }
    //        if (!string.IsNullOrEmpty(cookie))
    //        {
    //            headers.Add("Cookie", cookie);
    //        }
    //        return headers;
    //    }

    //    public static JDCookie CheckCookie(string cookie)
    //    {
    //        //string url = "https://me-api.jd.com/user_new/info/GetJDUserInfoUnion";
    //        //var result = HttpClientHelper.Get<JDUserInfoUnionResult>(url, null, InitHeaders(cookie));
    //        //if (result == null)
    //        //{
    //        //    return new JDCookie { Available = true };
    //        //}
    //        //if (result != null && result.msg == "success")
    //        //{
    //        //    return new JDCookie
    //        //    {
    //        //        Available = true,
    //        //        //isPlusVip = result.data.userInfo.isPlusVip == 1,
    //        //        //isRealNameAuth = result.data.userInfo.isRealNameAuth == 1,
    //        //        //levelName = result.data.userInfo.baseInfo.levelName,
    //        //        //userLevel = result.data.userInfo.baseInfo.userLevel,
    //        //        //nickname = result.data.userInfo.baseInfo.nickname,
    //        //        //beanNum = result.data.assetInfo.beanNum
    //        //    };
    //        //}
    //        return null;
    //    }
    //}
}