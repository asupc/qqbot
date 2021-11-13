using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace QQBot.Docker
{
    public class WSocketClientHelp
    {

        public static ClientWebSocket ws = null;
        bool isUserClose = false;//是否最后由用户手动关闭
        /// <summary>
        /// 包含一个数据的事件
        /// </summary>
        public delegate void MessageEventHandler(object sender, string data);
        public delegate void ErrorEventHandler(object sender, Exception ex);
        private InstallConfig systemConfig;

        public async Task StartGoCQHttp(InstallConfig config)
        {
            await Close(WebSocketCloseStatus.NormalClosure, "配置更新重新链接。");
            if (config == null || string.IsNullOrEmpty(config.cqhttpWS))
            {
                return;
            }
            systemConfig = config;
            if (ws == null)
                ws = new ClientWebSocket();
            Task.Run(async () =>
            {
                if (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open)
                    return;

                string netErr = string.Empty;
                try
                {
                    //初始化链接
                    isUserClose = false;
                    ws = new ClientWebSocket();
                    await ws.ConnectAsync(new Uri(config.cqhttpWS), CancellationToken.None);

                    WSocketClientHelp_OnOpen(ws, new EventArgs());
                    //全部消息容器
                    List<byte> bs = new List<byte>();
                    //缓冲区
                    var buffer = new byte[1024 * 4];
                    //监听Socket信息
                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    //是否关闭
                    while (!result.CloseStatus.HasValue)
                    {
                        //文本消息
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            bs.AddRange(buffer.Take(result.Count));

                            //消息是否已接收完全
                            if (result.EndOfMessage)
                            {
                                //发送过来的消息
                                string userMsg = Encoding.UTF8.GetString(bs.ToArray(), 0, bs.Count);
                                WSocketClientHelp_OnMessage(ws, userMsg);
                                bs = new List<byte>();
                            }
                        }
                        //继续监听Socket信息
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    ////关闭WebSocket（服务端发起）
                    //await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" WS发生错误：" + ex.Message);
                }
                finally
                {
                    if (!isUserClose)
                        Close(ws.CloseStatus.Value, ws.CloseStatusDescription + netErr);
                }
            });

        }

        private void WSocketClientHelp_OnClose(object sender, EventArgs e)
        {
        }

        private void WSocketClientHelp_OnError(object sender, Exception ex)
        {
        }

        private void WSocketClientHelp_OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("WS连接成功");
        }

        private async Task WSocketClientHelp_OnMessage(object sender, string data)
        {
            ;
            try
            {
                ReceiveMessage receiveMessage = JsonConvert.DeserializeObject<ReceiveMessage>(data);
                var message = receiveMessage.message.ToLower();
                if (receiveMessage.user_id.ToString() == systemConfig.ManagerQQ && message == "更新qqbot")
                {
                    Console.WriteLine($"接受{receiveMessage.message}指令，开始处理！");
                    ShellHelper.Update();
                }
                if (receiveMessage.user_id.ToString() == systemConfig.ManagerQQ && message == "启动qqbot")
                {
                    Console.WriteLine($"接受{receiveMessage.message}指令，开始处理！");
                    ShellHelper.Kill();
                    ShellHelper.Start();
                }
                if (receiveMessage.user_id.ToString() == systemConfig.ManagerQQ && message == "结束qqbot")
                {
                    Console.WriteLine($"接受{receiveMessage.message}指令，开始处理！");
                    ShellHelper.Kill();
                }
                if (receiveMessage.user_id.ToString() == systemConfig.ManagerQQ && message == "重启qqbot")
                {
                    Console.WriteLine($"接受{receiveMessage.message}指令，开始处理！");
                    ShellHelper.Kill();
                    ShellHelper.Start();
                }
            }
            catch (Exception e)
            {
            }
        }

        public async Task Close(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            if (ws == null)
            {
                return;
            }
            try
            {
                await ws.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ws.Abort();
                ws.Dispose();
                WSocketClientHelp_OnClose(ws, new EventArgs());
            }
        }
    }
}
