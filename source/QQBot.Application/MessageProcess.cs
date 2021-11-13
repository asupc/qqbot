using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QQBot.DB;
using QQBot.Entities;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using QQBot.Entities.Socket;
using QQBot.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQBot.Application
{
    public class MessageProcess
    {
        private readonly ILog log = LogManager.GetLogger("NETCoreRepository", typeof(MessageProcess));

        public QLHttpService QLHttpHelper;
        public GoCQHttpHelper GoCQHttpHelper;
        private InstallConfig InstallConfig;
        private QLPanelService PanelService;
        private BaseRepository<JDCookie> JDCookieRepository;
        private JDCookieService jdCookieService;
        private List<SystemCommand> SystemCommands;


        public MessageProcess(QLHttpService QLHttpHelper, GoCQHttpHelper goCQHttpHelper, QLPanelService QLHelper, JDCookieService jdCookieService)
        {
            this.GoCQHttpHelper = goCQHttpHelper;
            this.QLHttpHelper = QLHttpHelper;
            this.PanelService = QLHelper;
            this.JDCookieRepository = BaseRepository<JDCookie>.Instance;
            this.jdCookieService = jdCookieService;
        }

        private static List<string> rejects = new List<string>
        {
            "[cq:"
        };

        public bool IsCommand(string message, string command, out string cc)
        {
            cc = null;
            var systemCommand = SystemCommands.FirstOrDefault(n => n.Key.ToLower() == command.ToLower());
            if (systemCommand != null)
            {
                if (string.IsNullOrEmpty(systemCommand.Command))
                {
                    systemCommand.Command = systemCommand.Key;
                }
                systemCommand.Command = systemCommand.Command.ToLower();
                var ccc = systemCommand.Command.Split(",").FirstOrDefault(n => message.ToLower().StartsWith(n));
                if (ccc != null)
                {
                    cc = ccc;
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// 收到消息处理
        /// </summary>
        /// <param name="receiveMessage"></param>
        public async Task Message(ReceiveMessage receiveMessage)
        {
            SystemCommands = SystemCommandHelper.Get();
            InstallConfig = InstallConfigHelper.Get();
            if ((receiveMessage.group_id > 0 && !InstallConfig.Groups.Contains(receiveMessage.group_id.ToString())) || string.IsNullOrEmpty(receiveMessage.message.Replace(" ", "").Replace("\r", "").Replace("\n", "")) || rejects.Any(n => receiveMessage.message.ToLower().Contains(n)))
            {
                return;
            }
            if (receiveMessage.message_type == "group" && (InstallConfig == null || string.IsNullOrEmpty(InstallConfig.Groups) || !InstallConfig.Groups.Contains(receiveMessage.group_id.ToString())))
            {
                return;
            }
            try
            {
                receiveMessage.message = receiveMessage.message.Trim();
                var cks = CookieHepler.ConvertCookie(receiveMessage.message);
                List<string> messages = new List<string>();
                if (cks.Count > 0 && receiveMessage.message_type == "group")
                {
                    GoCQHttpHelper.DeleteMessage(receiveMessage.message_id);
                    messages.Add($"为了保证您的账户安全，机器人已自动撤回您的Cookie信息。");
                }
                if (receiveMessage.user_id.ToString() == InstallConfig.ManagerQQ)
                {
                    ManagerCommand(receiveMessage);
                }
                var command = await QQBotDbContext.Instance.Commands.SingleOrDefaultAsync(n => n.Key.ToUpper() == receiveMessage.message.ToUpper());
                if (command != null)
                {
                    messages.Add(command.Message);
                }
                else if (cks.Count > 0)
                {
                    foreach (var c in cks.Where(n => n.QQ < 10000))
                    {
                        c.QQ = receiveMessage.user_id;
                    }
                    await DealCookie(cks);
                }
                string cc = "";
                foreach (var m in messages)
                {
                    GoCQHttpHelper.Send(m, receiveMessage.group_id, receiveMessage.user_id);
                }
                if (IsCommand(receiveMessage.message, "我的账号", out cc))
                {
                    MyCookie(receiveMessage);
                }
                else if (IsCommand(receiveMessage.message, "删除账号", out cc))
                {
                    UserDeleteCookie(receiveMessage);
                }
                else if (IsCommand(receiveMessage.message, "手机号登录", out cc) && (InstallConfig.SMSAllowGroup || receiveMessage.group_id == 0) && (string.IsNullOrEmpty(InstallConfig.SMSBlackQQ) || !InstallConfig.SMSBlackQQ.Contains(receiveMessage.user_id.ToString())))
                {
                    Extends.SMSLoginModels = Extends.SMSLoginModels.Where(n => n.CreateTime.AddMinutes(1) > DateTime.Now).ToList();
                    string message = "OK，请1分钟内输入你的手机号码：";
                    if (string.IsNullOrEmpty(InstallConfig.SMSService))
                    {
                        message = "机器人暂未配置nvjdc短信服务。无法使用手机号登录。";
                    }
                    else if (Extends.SMSLoginModels.Any(n => n.QQ == receiveMessage.user_id))
                    {
                        message = "已存在手机号登录任务，请不要重复使用指令。";
                    }
                    else
                    {
                        Extends.SMSLoginModels.Add(new Entities.SMSLoginModel
                        {
                            QQ = receiveMessage.user_id,
                            SMSLoginStatus = Entities.SMSLoginStatus.Tel,
                            CreateTime = DateTime.Now
                        });
                    }
                    GoCQHttpHelper.Send(message, receiveMessage.group_id, receiveMessage.user_id);
                }
                else if (receiveMessage.message.Length == 11 && Extends.CheckPhoneIsAble(receiveMessage.message))
                {
                    Extends.SMSLoginModels = Extends.SMSLoginModels.Where(n => n.CreateTime.AddMinutes(3) > DateTime.Now).ToList();
                    var loginModel = Extends.SMSLoginModels.SingleOrDefault(n => n.QQ == receiveMessage.user_id && n.SMSLoginStatus == Entities.SMSLoginStatus.Tel);
                    if (loginModel != null && !string.IsNullOrEmpty(InstallConfig.SMSService))
                    {
                        string message = "";
                        try
                        {
                            var result = await NolanJDC.SendSMS(receiveMessage.message);
                            if (result == null)
                            {
                                GoCQHttpHelper.Send("请求短信验证码失败，短信服务异常。", receiveMessage.group_id, receiveMessage.user_id);
                                return;
                            }
                            if (!result.success)
                            {
                                var success = false;
                                for (int i = 0; i < 5; i++)
                                {
                                    GoCQHttpHelper.Send($"请求短信出现验证，第{i + 1}次自动验证...", receiveMessage.group_id, receiveMessage.user_id);
                                    var autoCaptcha = await NolanJDC.AutoCaptcha(receiveMessage.message);
                                    if (autoCaptcha != null && autoCaptcha.success)
                                    {
                                        success = true;
                                        break;
                                    }
                                }
                                if (!success)
                                {
                                    GoCQHttpHelper.Send("请求短信验证码失败，请尝试其他Cookie获取方式。", receiveMessage.group_id, receiveMessage.user_id);
                                    return;
                                }
                            }
                            loginModel.Tel = receiveMessage.message;
                            loginModel.SMSLoginStatus = SMSLoginStatus.Code;
                            GoCQHttpHelper.Send("验证成功，请回复收到的验证码：", receiveMessage.group_id, receiveMessage.user_id);
                        }
                        catch (Exception e)
                        {
                            Extends.SMSLoginModels.Remove(loginModel);
                            Console.WriteLine(JsonConvert.SerializeObject(e));
                            message = e.Message;
                        }
                        GoCQHttpHelper.Send(message, receiveMessage.group_id, receiveMessage.user_id);
                    }
                }
                else if (receiveMessage.message.Length == 6)
                {
                    try
                    {
                        int.Parse(receiveMessage.message);
                    }
                    catch
                    {
                        return;
                    }
                    Extends.SMSLoginModels = Extends.SMSLoginModels.Where(n => n.CreateTime.AddMinutes(3) > DateTime.Now).ToList();
                    try
                    {
                        var loginModel = Extends.SMSLoginModels.SingleOrDefault(n => n.QQ == receiveMessage.user_id && n.SMSLoginStatus == Entities.SMSLoginStatus.Code);
                        if (loginModel != null)
                        {
                            GoCQHttpHelper.Send($"收到：{loginModel.Tel}的验证码{receiveMessage.message}，验证中请稍后。", null, receiveMessage.user_id);
                            var result = await NolanJDC.VerifyCode(loginModel.Tel, receiveMessage.message);
                            if (result == null)
                            {
                                GoCQHttpHelper.Send($"{loginModel.Tel}的验证码{receiveMessage.message}，验证中验证失败！", null, receiveMessage.user_id);
                                return;
                            }
                            Extends.SMSLoginModels.Remove(loginModel);
                            Console.WriteLine("VerifyCode Result：" + JsonConvert.SerializeObject(result));
                            if (result != null && result.success)
                            {

                                var cks1 = CookieHepler.ConvertCookie(result.data.ToString());
                                if (cks1.Count > 0)
                                {
                                    foreach (var c in cks1)
                                    {
                                        c.QQ = receiveMessage.user_id;
                                    }
                                    await DealCookie(cks1);
                                }
                                return;
                            }
                            else if (result != null && !string.IsNullOrEmpty(result.message))
                            {
                                GoCQHttpHelper.Send(result.message, null, receiveMessage.user_id);
                            }
                            else
                            {
                                GoCQHttpHelper.Send("未知错误，请尝试其他Cookie获取方法。", null, InstallConfig.ManagerQQ);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string message = e.Message;
                        GoCQHttpHelper.Send(message, receiveMessage.group_id, receiveMessage.user_id);
                    }
                }
                await ExecTask(receiveMessage);
            }
            catch (Exception e)
            {

            }
            GC.Collect();
        }



        private void MyCookie(ReceiveMessage receiveMessage)
        {
            var total = QQBotDbContext.Instance.JDCookies.AsNoTracking().Where(n => n.QQ == receiveMessage.user_id).ToList();
            if (total.Count() <= 0)
            {
                GoCQHttpHelper.Send("您没有绑定任何账号。", receiveMessage.group_id, receiveMessage.user_id);
                return;
            }
            var mess = $"您共绑定{total.Count()}个账号：";
            var i = 1;
            foreach (var item in total)
            {
                mess += $"\r\n账号🆔{item.nickname}{(item.Available ? "✅有效" : "❌失效")}";
                if (mess.Length > 2000)
                {
                    GoCQHttpHelper.Send(mess, receiveMessage.group_id, receiveMessage.user_id);
                    mess = "";
                }
                i++;
            }
            GoCQHttpHelper.Send(mess, receiveMessage.group_id, receiveMessage.user_id);
        }

        /// <summary>
        /// 删除用户Cookie
        /// </summary>
        /// <param name="receiveMessage"></param>
        private void UserDeleteCookie(ReceiveMessage receiveMessage)
        {
            if (receiveMessage.message.Split(" ").Length == 2)
            {
                var u = receiveMessage.message.Split(" ")[1];
                var c = QQBotDbContext.Instance.JDCookies.FirstOrDefault(n => n.QQ == receiveMessage.user_id && (n.PTPin == u || n.nickname == u));
                if (c == null)
                {
                    GoCQHttpHelper.Send($"指定删除的账号{u}不存在！", null, receiveMessage.user_id);
                    return;
                }
                jdCookieService.Delete(new List<string> { c.Id });
                BaseRepository<JDCookie>.Instance.DeleteByIds(new List<string> { c.Id });
                GoCQHttpHelper.Send($"指定删除的账号{u}成功！Cookie：\r\n{c.CookieToString()}", null, receiveMessage.user_id);
                GoCQHttpHelper.Send($"QQ用户{receiveMessage.user_id}，已删除指定账号：{u}。", null, InstallConfig.ManagerQQ);
                return;
            }

            var total = jdCookieService.Delete(receiveMessage.user_id);
            BaseRepository<JDCookie>.Instance.DeleteByIds(total.Select(n => n.Id));
            var mess = $"您的{total.Count()}个账号已被全部清除。";

            if (receiveMessage.group_id > 0)
            {
                mess += "\r\nCookie信息将私聊发送到您的QQ。";
            }
            GoCQHttpHelper.Send(mess, receiveMessage.group_id, receiveMessage.user_id);
            mess = null;
            foreach (var item in total)
            {
                mess += item.CookieToString() + "&";
                if (mess.Length > 2000)
                {
                    GoCQHttpHelper.Send(mess.Trim('&'), null, receiveMessage.user_id);
                    mess = "";
                }
            }
            GoCQHttpHelper.Send(mess.Trim('&'), null, receiveMessage.user_id);
            GoCQHttpHelper.Send($"QQ用户{receiveMessage.user_id}，已删除账号。", null, InstallConfig.ManagerQQ);

        }

        public async Task<string> DealCookie(List<JDCookie> cks)
        {
            await JDScriptsTask.CheckCookie(cks, true);
            await PanelService.SyncJDCookies();
            return null;
        }

        /// <summary>
        /// 请求消息处理
        /// </summary>
        /// <param name="receiveMessage"></param>
        public void Request(ReceiveMessage receiveMessage)
        {
            InstallConfig = InstallConfigHelper.Get();
            switch (receiveMessage.request_type)
            {
                case "friend":
                    if (InstallConfig.AddFriend)
                    {
                        Task.Run(() =>
                        {
                            Thread.Sleep(2000);
                            GoCQHttpHelper.AgreeAdd(receiveMessage.flag, receiveMessage.sub_type);
                            if (!string.IsNullOrEmpty(InstallConfig.AddFriendMessage))
                            {
                                Thread.Sleep(1000);
                                GoCQHttpHelper.Send(InstallConfig.AddFriendMessage, null, receiveMessage.user_id);
                            }
                        });
                    }
                    break;
                case "group":
                    if (InstallConfig.AddGroup && InstallConfig.Groups.Contains(receiveMessage.group_id.ToString()))
                    {
                        Task.Run(() =>
                        {
                            Thread.Sleep(2000);
                            GoCQHttpHelper.AgreeAdd(receiveMessage.flag, receiveMessage.sub_type, "set_group_add_request");
                            if (!string.IsNullOrEmpty(InstallConfig.AddGroupMessage))
                            {
                                Thread.Sleep(1000);
                                GoCQHttpHelper.Send(InstallConfig.AddGroupMessage, receiveMessage.group_id, receiveMessage.user_id);
                            }
                        });
                    }
                    break;
            }
        }

        private async void ManagerCommand(ReceiveMessage receiveMessage)
        {
            await Task.Run(async () =>
            {
                var QLPanelCookieRepository = BaseRepository<QLPanelCookie>.Instance;
                var command = receiveMessage.message.ToLower();
                string cc = "";
                if (IsCommand(receiveMessage.message, "更新QQBot", out cc))
                {
                    GoCQHttpHelper.Send("正在执行自动更新，稍后重启。", null, receiveMessage.user_id);
                    ShellHelper.Upate();
                    return;
                }
                if (IsCommand(receiveMessage.message, "清理过期Cookie", out cc))
                {
                    await PanelService.SyncJDCookies();

                    var qqs = QQBotDbContext.Instance.JDCookies.Where(n => !n.Available).ToList();
                    foreach (var item in qqs.Where(n => n.QQ > 10000).GroupBy(n => n.QQ))
                    {
                        GoCQHttpHelper.Send(@$"由于您的账号已过期，管理员已将您的账号移除，如需重新加入，请重新获取Cookie发送给机器人。
移除账号：{(string.Join('，', item.ToList().Select(n => n.nickname))).Trim('，')} ", null, item.Key);
                    }
                    var count = JDCookieRepository.DeleteByIds(qqs.Select(n => n.Id));
                    GoCQHttpHelper.Send($"清理完成，本次清理{qqs.Count()}个过期Cookie。", null, receiveMessage.user_id);
                    return;
                }
                if (IsCommand(receiveMessage.message, "检查Cookie", out cc))
                {
                    GoCQHttpHelper.Send("正在执行检查Cookie，如Cookie过期将向用户发送过期提醒，并自动禁用Cookie。", null, receiveMessage.user_id);
                    await PanelService.CheckCookie();

                    return;
                }
                if (IsCommand(receiveMessage.message, "重新分配", out cc))
                {
                    try
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        QLPanelCookieRepository.DeleteByIds(QLPanelCookieRepository.Get(new { Mode = QLPanelCookieMode.Auto }).Select(n => n.Id));
                        var messages = await PanelService.SyncJDCookies();
                        stopwatch.Stop();
                        foreach (var message in messages)
                        {
                            GoCQHttpHelper.Send(message, null, receiveMessage.user_id);
                        }
                        GoCQHttpHelper.Send($"Cookie重新分配成功，手动指定的Cookie将不受此次分配影响，同步用时：{stopwatch.Elapsed.TotalSeconds:F2}秒", null, receiveMessage.user_id);
                    }
                    catch (Exception e)
                    {
                        GoCQHttpHelper.Send($"Cookie重新分配失败：{e.Message}", null, receiveMessage.user_id);
                        log.Error("Cookie重新分配失败", e);
                    }
                    return;
                }
                if (IsCommand(receiveMessage.message, "同步Cookie", out cc))
                {
                    try
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        var messages = await PanelService.SyncJDCookies();
                        stopwatch.Stop();
                        foreach (var message in messages)
                        {
                            GoCQHttpHelper.Send(message, null, receiveMessage.user_id);
                        }
                        GoCQHttpHelper.Send($"Cookie同步到青龙成功，同步用时：{stopwatch.Elapsed.TotalSeconds:F2}秒", null, receiveMessage.user_id);
                    }
                    catch (Exception e)
                    {
                        GoCQHttpHelper.Send($"Cookie同步失败：{e.Message}", null, receiveMessage.user_id);
                        log.Error("Cookie同步失败", e);
                    }
                    return;
                }
                if (IsCommand(receiveMessage.message, "消息推送", out cc))
                {
                    var qqs = QQBotDbContext.Instance.JDCookies.AsNoTracking().Where(n => n.QQ > 0).Select(n => n.QQ).Distinct().ToList();
                    foreach (var item in qqs)
                    {
                        GoCQHttpHelper.Send(receiveMessage.message.Replace(cc + " ", ""), null, item);
                        Thread.Sleep(3000);
                    }
                    return;
                }
                if (IsCommand(receiveMessage.message, "统计cookie", out cc))
                {
                    var count = JDCookieRepository.Count();
                    var gq = JDCookieRepository.Count("Available=0");
                    try
                    {
                        var todayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        var today = JDCookieRepository.Count("UpdateTime>=@TM", new { TM = todayDate });
                        //var top = JDCookieRepository.Count(new { Top = true });
                        var mess = $@"账号总数：{count}个
过期数量：{gq}个
今日更新：{today}个";
                        var qls = QQBotDbContext.Instance.QLConfigs.AsNoTracking().OrderBy(n => n.Name).ToList();
                        foreach (var ql in qls)
                        {
                            mess += @$"
{ql.Name}：{ql.CookieCount}个";
                            if (ql.MaxCount > 0)
                            {
                                mess += $"（上限：{ql.MaxCount}）";
                            }
                        }
                        GoCQHttpHelper.Send(mess, null, receiveMessage.user_id);
                    }
                    catch (Exception e)
                    {
                    }
                    return;
                }
                if (IsCommand(receiveMessage.message, "导出cookie", out cc))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    var cookies = QQBotDbContext.Instance.JDCookies.AsNoTracking().Where(n => n.Available).ToList();

                    for (int i = 0; i < cookies.Count(); i++)
                    {
                        var c = cookies[i];
                        stringBuilder.AppendLine($"pt_key={c.PTKey};pt_pin={c.PTPin};{(c.QQ > 0 ? $"qq={c.QQ};" : "")}&");
                    }
                    var fileName1 = $"config/cks_{DateTime.Now:yyMMddHHmmssfff}.txt";
                    var fileName2 = $"config/cks_{DateTime.Now:yyMMddHHmmssfff}.json";
                    File.Create(fileName1).Close();
                    File.Create(fileName2).Close();
                    using (StreamWriter streamWriter = new StreamWriter(fileName1))
                    {
                        streamWriter.WriteLine(stringBuilder.ToString());
                        streamWriter.Flush();
                    }
                    using (StreamWriter streamWriter = new StreamWriter(fileName2))
                    {
                        streamWriter.WriteLine(JsonConvert.SerializeObject(cookies.Select(n => new
                        {
                            value = n.CookieToString(),
                            name = "JD_COOKIE",
                            remarks = n.nickname
                        })));
                        streamWriter.Flush();
                    }
                    GoCQHttpHelper.Send(@$"所有Cookie已导出到服务器
{fileName1}内容可发给机器人导入。
{fileName2}内容可post提交到青龙。", null, receiveMessage.user_id);
                    return;
                }
            });
        }

        private async Task<bool> ExecTask(ReceiveMessage receiveMessage)
        {
            bool haveTask = false;
            await Task.Run(async () =>
            {
                bool all = false;


                if (receiveMessage.message.StartsWith("推送 ") && receiveMessage.user_id.ToString() == InstallConfig.ManagerQQ)
                {
                    all = true;
                    receiveMessage.message = receiveMessage.message.Replace("推送 ", "");
                }

                var cs = receiveMessage.message.Trim().ToLower().Split(" ");
                var command = cs[0];

                var task = QQBotDbContext.Instance.QQBotTasks.AsNoTracking().FirstOrDefault(n => n.Enable && n.Command == command);


                if (task != null)
                {

                    var cookies = QQBotDbContext.Instance.JDCookies.Where(n => n.QQ > 0 && (n.QQ == receiveMessage.user_id || all)).ToList();

                    if (cs.Length > 1)
                    {
                        cookies = cookies.Where(n => n.QQ.ToString() == cs[1] || n.PTPin == cs[1] || n.nickname == cs[1]).ToList();
                        cookies.ForEach((t) => t.QQ = receiveMessage.user_id);
                    }

                    haveTask = true;
                    var message = $"开始执行{task.Name}任务，账户数量：{cookies.Count}";
                    var tCookies = cookies.Where(n => !n.Available);
                    if (cookies.Count == 0)
                    {
                        GoCQHttpHelper.Send("未绑定任何账户信息，请发送“教程”查看获取狗东Cookie方法。", receiveMessage.group_id, receiveMessage.user_id);
                        return;
                    }
                    else if (tCookies.Count() > 0)
                    {
                        message += $"\r\n过期账号：{string.Join('，', tCookies.Select(n => n.nickname)).Trim('，')}，共计：{tCookies.Count()}个。请按获取教程重新获取";
                    }
                    if (!cookies.Any(n => n.Available))
                    {
                        message += "\r\n您当前无可用账号。";
                        GoCQHttpHelper.Send(message, receiveMessage.group_id, receiveMessage.user_id);
                        return;
                    }
                    if (!task.EnablePush)
                    {
                        message += $"\r\n管理员已经关闭该任务的消息通知，您将无法收到执行结果。";
                    }
                    string limitMessage = "";
                    if (task.MaxCount > 0 && receiveMessage.user_id.ToString() != InstallConfig.ManagerQQ)
                    {
                        string fileName = "./scripts/limitRecord/" + DateTime.Now.ToString("yyyyMMdd") + task.Name + ".json";
                        if (!File.Exists(fileName))
                        {
                            File.Create(fileName).Close();
                        }
                        Dictionary<string, int> pairs = new Dictionary<string, int>();
                        using (StreamReader streamReader = new StreamReader(fileName))
                        {
                            var str = streamReader.ReadToEnd();
                            if (!string.IsNullOrEmpty(str))
                            {
                                pairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(str);
                            }
                        }
                        int queryCount = 0;
                        if (pairs.ContainsKey(receiveMessage.user_id.ToString()))
                        {
                            queryCount = pairs[receiveMessage.user_id.ToString()];
                            if (queryCount >= task.MaxCount)
                            {
                                limitMessage = $@"机器人已开启指令限制，该指令今日次数已达到{task.MaxCount}次，请明天再试。";
                                GoCQHttpHelper.Send(limitMessage, receiveMessage.group_id, receiveMessage.user_id);
                                return;
                            }
                            pairs[receiveMessage.user_id.ToString()] = queryCount + 1;
                        }
                        else
                        {
                            pairs.Add(receiveMessage.user_id.ToString(), 1);
                        }
                        using (StreamWriter writer = new StreamWriter(fileName))
                        {
                            writer.WriteLine(JsonConvert.SerializeObject(pairs));
                            writer.Flush();
                        }
                        limitMessage = $@"机器人已开启指令限制，该指令今日剩余次数{task.MaxCount - queryCount}次。";
                    }
                    GoCQHttpHelper.Send(message + "\r\n" + limitMessage, receiveMessage.group_id, receiveMessage.user_id);
                    if (cookies.Any(n => n.Available))
                    {
                        var envs = QQBotDbContext.Instance.Envs.AsNoTracking().Where(n => n.Enable).ToList();
                        envs.Add(new Env { Name = "Push", Value = "true" });
                        try
                        {
                            var stopwatch = new Stopwatch();
                            stopwatch.Start();
                            if (task.EnableConc)
                            {
                                Console.WriteLine($"{task.Name}已开启并发。");
                                List<Task> tasks = new List<Task> { };
                                foreach (var cookie in cookies.Where(n => n.Available))
                                {
                                    tasks.Add(JDScriptsTask.ExecTask(new List<JDCookie> { cookie }, task, envs));
                                }
                                Task.WaitAll(tasks.ToArray());
                            }
                            else
                            {
                                await JDScriptsTask.ExecTask(cookies.Where(n => n.Available), task, envs);
                            }
                            stopwatch.Stop();
                            if (!all && cookies.Count(n => n.Available) > 1)
                            {
                                GoCQHttpHelper.Send($"{task.Name}任务执行完成，任务执行用时：{stopwatch.Elapsed.TotalSeconds.ToString("F2")}秒。", receiveMessage.group_id, receiveMessage.user_id);
                            }
                            else if (all)
                            {
                                GoCQHttpHelper.Send($"执行{task.Name}推送任务完成，推送账号数：{cookies.Count(n => n.Available)}个，任务执行用时：{stopwatch.Elapsed.TotalSeconds.ToString("F2")}秒。", receiveMessage.group_id, receiveMessage.user_id);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error($"执行任务{task.Name}出错", e);
                        }
                    }
                }
            });
            return haveTask;
        }
    }
}