using log4net;
using Newtonsoft.Json;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using System;
using System.Threading.Tasks;

namespace QQBot.Utils
{
    public class GoCQHttpHelper
    {

        private readonly ILog log = LogManager.GetLogger("NETCoreRepository", typeof(GoCQHttpHelper));

        private InstallConfig InstallConfig;

        public GoCQHttpHelper()
        {
            InstallConfig = InstallConfigHelper.Get();
        }


        public void Send(string message, long? group_id, string user_id)
        {
            Send(message, group_id, Convert.ToInt64(user_id));
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="group_id"></param>
        /// <param name="user_id"></param>
        public void Send(string message, long? group_id, long? user_id)
        {
            if (message == null || string.IsNullOrEmpty(message.Trim()))
            {
                return;
            }
            string l = $"消息发送：user_id{user_id} Message：{message}";
            try
            {
                if (group_id.HasValue && group_id.Value > 0 && !message.Contains("[CQ:"))
                {
                    message = @$"[CQ:at,qq={user_id}]
{message}";
                }
                var m = new
                {
                    message_type = (group_id.HasValue && group_id.Value > 0) ? "group" : "private",
                    user_id = (group_id.HasValue && group_id.Value > 0) ? null : user_id,
                    group_id = group_id,
                    message = message
                };
                Task.Run(() =>
                {
                    var result = HttpClientHelper.Post<GoCQHttpResult>($"{InstallConfig.cqhttpHttp}/send_msg", JsonConvert.SerializeObject(m));
                });
            }
            catch (Exception e)
            {
                log.Error(l, e);
            }
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="message_id"></param>
        public void DeleteMessage(int message_id)
        {
            try
            {
                Task.Run(() =>
                {
                    var result = HttpClientHelper.Get<GoCQHttpResult>($"{InstallConfig.cqhttpHttp}/delete_msg?message_id={message_id}");
                });
            }
            catch (Exception e)
            {
                log.Error($"撤回消息ID:{message_id}", e);
            }
        }


        /// <summary>
        /// 群公告
        /// </summary>
        /// <param name="content"></param>
        /// <param name="group_id"></param>
        public void Notice(string content, long group_id)
        {
            try
            {
                HttpClientHelper.Get<GoCQHttpResult>($"{InstallConfig.cqhttpHttp}/_send_group_notice?content={content}&group_id={group_id}");
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 同意添加好友
        /// </summary>
        public void AgreeAdd(string flag, string sub_type, string type = "set_friend_add_request")
        {
            try
            {
                var result = HttpClientHelper.Get<GoCQHttpResult>($"{InstallConfig.cqhttpHttp}/{type}?approve=1&flag={flag}&sub_type={sub_type}");
            }
            catch (Exception e)
            {
                log.Error($"AgreeAdd:{type} Exception", e);
            }
        }
    }
}
