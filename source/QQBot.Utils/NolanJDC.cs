using Newtonsoft.Json;
using QQBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QQBot.Utils
{
    public static class NolanJDC
    {

        public static async Task<NvjdcResultModel> SendSMS(string phone)
        {
            return await Task.Run(() =>
            {
                var result = HttpClientHelper.Post<NvjdcResultModel>(InstallConfigHelper.Get().SMSService + "/api/SendSMS", JsonConvert.SerializeObject(new
                {
                    Phone = phone,
                    qlkey = 0
                }));
                return result;
            });
        }

        public static async Task<NvjdcResultModel> AutoCaptcha(string phone)
        {

            return await Task.Run(() =>
            {
                var result = HttpClientHelper.Post<NvjdcResultModel>(InstallConfigHelper.Get().SMSService + "/api/AutoCaptcha", JsonConvert.SerializeObject(new
                {
                    Phone = phone
                }));
                return result;
            });
        }

        public static async Task<NvjdcResultModel> VerifyCode(string phone, string code)
        {
            return await Task.Run(() =>
            {
                var result = HttpClientHelper.Post<NvjdcResultModel>($"{InstallConfigHelper.Get().SMSService}/api/VerifyCode", JsonConvert.SerializeObject(new
                {
                    Phone = phone,
                    code = code,
                    qlkey = 0,
                    QQ = ""
                }));
                return result;
            });
        }
    }
}
