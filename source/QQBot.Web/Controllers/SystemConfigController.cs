using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using QQBot.Utils;
using Quartz;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class SystemConfigController : BaseController
    {
        WSocketClientHelp socketClientHelp;
        public SystemConfigController(WSocketClientHelp socketClientHelp)
        {
            this.socketClientHelp = socketClientHelp;
        }

        [HttpPost]
        public async Task<ResultModel<bool>> Update([FromBody] InstallConfig systemConfig)
        {
            var currentConfig = InstallConfigHelper.Get();
            if (currentConfig.cqhttpHttp != systemConfig.cqhttpHttp)
            {
                try
                {
                    var ws = new ClientWebSocket();
                    await ws.ConnectAsync(new Uri(systemConfig.cqhttpWS), CancellationToken.None);
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                catch (Exception e)
                {
                    return new ResultModel<bool> { Code = 500, Message = "尝试连接cqhttp WS协议失败，请修改后重试。" };
                }
            }
            if (currentConfig.cqhttpWS != systemConfig.cqhttpWS)
            {
                try
                {
                    var m = new
                    {
                        message_type = "private",
                        user_id = systemConfig.ManagerQQ,
                        message = "配置go-cqhttp 协议测试消息。"
                    };
                    var result = HttpClientHelper.Post<GoCQHttpResult>($"{systemConfig.cqhttpHttp}/send_msg", JsonConvert.SerializeObject(m));
                }
                catch (Exception e)
                {
                    return new ResultModel<bool> { Code = 500, Message = "尝试连接cqhttp Http协议失败，请修改后重试。" };
                }
            }
            if (!string.IsNullOrEmpty(systemConfig.SMSService) && systemConfig.SMSService != "http://" && systemConfig.SMSService != currentConfig.SMSService)
            {
                var s = HttpClientHelper.Get($"{systemConfig.SMSService}/login");
                if (string.IsNullOrEmpty(s))
                {
                    return new ResultModel<bool> { Code = 500, Message = "尝试连接nolanjdc短信验证服务，请修改后重试。" };
                }
            }
            currentConfig.cqhttpWS = systemConfig.cqhttpWS;
            currentConfig.cqhttpHttp = systemConfig.cqhttpHttp;
            currentConfig.ManagerQQ = systemConfig.ManagerQQ;
            currentConfig.Groups = systemConfig.Groups;
            currentConfig.AddFriend = systemConfig.AddFriend;
            currentConfig.AddFriendMessage = systemConfig.AddFriendMessage;
            currentConfig.AddGroup = systemConfig.AddGroup;
            currentConfig.AddGroupMessage = systemConfig.AddGroupMessage;
            currentConfig.SMSService = systemConfig.SMSService;
            currentConfig.SMSAllowGroup = systemConfig.SMSAllowGroup;
            currentConfig.SMSBlackQQ = systemConfig.SMSBlackQQ;
            currentConfig.SMSMaxCount = systemConfig.SMSMaxCount;
            InstallConfigHelper.Set(currentConfig);
            socketClientHelp.StartGoCQHttp();
            return new ResultModel<bool> { Data = true };
        }

        [HttpGet]
        public ResultModel<InstallConfig> Get()
        {
            var currentConfig = InstallConfigHelper.Get();
            currentConfig.PassWord = "";
            currentConfig.UserName = "";
            return new ResultModel<InstallConfig> { Data = currentConfig };
        }


        [HttpPost("commands")]
        public async Task<ResultModel<bool>> UpdateSystemCommand([FromBody] List<SystemCommand> commands)
        {
            SystemCommandHelper.Set(commands);
            return new ResultModel<bool> { Data = true };
        }

        [HttpGet("commands")]
        public ResultModel<List<SystemCommand>> GetSystemCommand()
        {
            return new ResultModel<List<SystemCommand>> { Data = SystemCommandHelper.Get() };
        }
    }
}
