using System;
using Newtonsoft.Json;
using QQBot.Entities.Socket;
using log4net;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using QQBot.Entities.Config;
using QQBot.Utils;

namespace QQBot.Application
{
    public class WSocketClientHelp
    {

        private readonly ILog log = LogManager.GetLogger("NETCoreRepository", typeof(WSocketClientHelp));

        private MessageProcess MessageProcess;

        public GoCQHttpHelper goCQHttpHelper;


        public WSocketClientHelp(MessageProcess MessageProcess, GoCQHttpHelper goCQHttpHelper)
        {
            this.MessageProcess = MessageProcess;
            this.goCQHttpHelper = goCQHttpHelper;
        }
        public static ClientWebSocket ws = null;
        bool isUserClose = false;//是否最后由用户手动关闭
        /// <summary>
        /// 包含一个数据的事件
        /// </summary>
        public delegate void MessageEventHandler(object sender, string data);
        public delegate void ErrorEventHandler(object sender, Exception ex);

        private InstallConfig installConfig;

        public async Task StartGoCQHttp()
        {
            await Close(WebSocketCloseStatus.NormalClosure, "配置更新重新链接。");
            if (ws == null)
                ws = new ClientWebSocket();
            installConfig = InstallConfigHelper.Get();
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
                    await ws.ConnectAsync(new Uri(installConfig.cqhttpWS), CancellationToken.None);

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
                    //netErr = " WS发生错误：" + ex.Message;
                    //log.Error(netErr);
                    WSocketClientHelp_OnError(ws, ex);
                }
                finally
                {
                    if (!isUserClose)
                        Close(ws.CloseStatus.Value, ws.CloseStatusDescription + netErr);
                }
            });
        }

        private void WSocketClientHelp_OnError(object sender, Exception ex)
        {
            Console.WriteLine("go-cqhttp 连接错误，将在10秒后重新连接。");
            Thread.Sleep(1000 * 10);
            StartGoCQHttp();
        }

        private void WSocketClientHelp_OnOpen(object sender, EventArgs e)
        {
            //Console.WriteLine("go-cqhttp 通讯服务连接成功！");
            if (!string.IsNullOrEmpty(installConfig.ManagerQQ))
                goCQHttpHelper.Send(@$"QQBot启动完成，开始愉快的玩耍吧！
当前时间：{DateTime.Now:HH时mm分ss秒}
当前版本：{Extends.Version}
GITHUB主页：https://github.com/asupc。
TG Group：https://t.me/asupc_qqbot", null, Convert.ToInt64(installConfig.ManagerQQ));
        }

        private void WSocketClientHelp_OnMessage(object sender, string data)
        {
            try
            {
                ReceiveMessage receiveMessage = JsonConvert.DeserializeObject<ReceiveMessage>(data);
                switch (receiveMessage.post_type)
                {
                    case "message":
                        MessageProcess.Message(receiveMessage);
                        break;
                    case "request":
                        MessageProcess.Request(receiveMessage);
                        break;
                }
            }
            catch (Exception e)
            {
                //log.Error("处理接受消息失败，", e);
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
                //关闭WebSocket（客户端发起）
                await ws?.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {

            }
            ws.Abort();
            ws.Dispose();
        }
    }
}
