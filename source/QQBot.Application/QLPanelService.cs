using Microsoft.EntityFrameworkCore;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Model;
using QQBot.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Application
{
    public class QLPanelService
    {
        private QLHttpService QLHttpHelper;
        public GoCQHttpHelper goCQHttpHelper;


        public QLPanelService(QLHttpService QLHttpHelper, GoCQHttpHelper goCQHttpHelper)
        {
            this.QLHttpHelper = QLHttpHelper;
            this.goCQHttpHelper = goCQHttpHelper;
        }

        public async Task CheckCookie()
        {
            var cookies = QQBotDbContext.Instance.JDCookies.AsNoTracking().Where(n => n.Available).ToList().GroupBy(n => n.QQ);
            foreach (var cookie in cookies)
            {
                await JDScriptsTask.CheckCookie(cookie.ToList(), false);
            }
            await SyncJDCookies();
        }

        /// <summary>
        /// 同步青龙的Cookie 到机器人
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<List<string>> SyncJDCookies()
        {
            Random random = new Random();
            List<string> messages = new List<string>();
            await Task.Run(async () =>
            {
                lock (this)
                {

                    var dbContext = QQBotDbContext.Instance;
                    var qls = dbContext.QLConfigs.Where(n => n.Weigth > 0 || n.EnableAll).AsNoTracking().ToList();

                    var botCookies = dbContext.JDCookies.AsNoTracking().OrderByDescending(n => n.Priority).ToList();

                    var qlPanelCookies = dbContext.QLPanelCookies.AsNoTracking().ToList();



                    #region 删除qqbot 中的重复cookie



                    var groupCookies = botCookies.GroupBy(n => n.PTPin.Decode().Encode()).Where(n => n.Count() > 1);
                    var botDeleteCookies = new List<JDCookie>();

                    foreach (var item in groupCookies)
                    {
                        for (int i = 1; i < item.Count(); i++)
                        {
                            botDeleteCookies.Add(item.ToList()[i]);
                        }
                    }
                    if (botDeleteCookies.Any())
                    {
                        BaseRepository<JDCookie>.Instance.DeleteByIds(botDeleteCookies.Select(n => n.Id));
                        botCookies = dbContext.JDCookies.AsNoTracking().ToList();
                    }
                    #endregion

                    if (!qls.Any())
                    {
                        messages.Add("未配置青龙面板或青龙面板权重小于1，同步取消！");
                        return;
                    }


                    List<Task> tasks = new List<Task>();


                    Dictionary<QLConfig, List<JDCookie>> qlCookies = new Dictionary<QLConfig, List<JDCookie>>();
                    bool refereshCookie = false;
                    foreach (QLConfig config in qls)
                    {
                        var result = QLHttpHelper.GetCookies(config);
                        var qllAllCookies = result.data.Where(n => n.value.Contains("pt_key") && n.value.Contains("pt_pin")).ToList();
                        var qlAddToBots = new List<JDCookie>();
                        var deleteQLIds = qllAllCookies.Select(n => n._id).ToList();

                        QLHttpHelper.DeleteEnv(config, deleteQLIds);
                        var QLCookies = qllAllCookies.Where(n => n.status == 0);
                        foreach (var item in QLCookies)
                        {
                            var tee = CookieHepler.ConvertCookie(item.value);
                            if (tee == null || tee.Count <= 0)
                            {
                                return;
                            }
                            var ck = tee[0];
                            if (!botCookies.Any(n => n.PTPin.Decode() == ck.PTPin.Decode()))
                            {
                                //Console.WriteLine($"n.PTPin.Decode()：{n.PTPin.Decode()}, ck.PTPin.Decode()：{ ck.PTPin.Decode()}");
                                refereshCookie = true;
                                qlAddToBots.Add(new JDCookie
                                {
                                    PTPin = ck.PTPin,
                                    PTKey = ck.PTKey,
                                    Available = true
                                });
                            }
                        }
                        if (qlAddToBots.Any())
                        {
                            BaseRepository<JDCookie>.Instance.AddRange(qlAddToBots);
                        }
                        //所有的cookie 同步到全量模式的青龙
                        if (config.EnableAll)
                        {
                            QLHttpHelper.AddEnv(config, botCookies.Where(n => n.Available).OrderByDescending(n => n.Priority).ThenByDescending(n => random.Next()).Select(n => new
                            {
                                value = n.CookieToString(),
                                name = "JD_COOKIE",
                                remarks = n.nickname ?? n.PTPin.Decode()
                            }));
                            config.CookieCount = botCookies.Count();
                            continue;
                        }
                        else
                        {
                            var syncCookies = botCookies.Where(n => n.Available && qlPanelCookies.Where(b => b.QLPanelId == config.Id).Select(v => v.CookieId).Contains(n.Id)).OrderByDescending(n => n.Priority).ThenByDescending(n => random.Next()).Take(config.MaxCount).ToList();
                            config.CookieCount = syncCookies.Count();
                            qlCookies.Add(config, syncCookies);
                        }
                    }
                    if (refereshCookie)
                    {
                        botCookies = dbContext.JDCookies.AsNoTracking().Where(n => n.Available).OrderByDescending(n => n.Priority).ToList();
                    }
                    else
                    {
                        botCookies = botCookies.Where(n => n.Available).ToList();
                    }
                    Task.WaitAll(tasks.ToArray());
                    tasks.Clear();

                    #region 同步未分配的账号到青龙

                    var wfpCookies = botCookies.Where(n => !qlPanelCookies.Select(u => u.CookieId).Contains(n.Id)).OrderByDescending(n => n.Priority).ThenBy(n => random.Next()).ToList();

                    //存在未分配的Cookie
                    if (wfpCookies.Count() > 0)
                    {
                        var kfpQL = qlCookies.Where(n => n.Key.CookieCount < n.Key.MaxCount).ToList();

                        if (kfpQL.Any())
                        {
                            int i = 0;
                            foreach (var item in wfpCookies)
                            {
                                var qlConfig = kfpQL.Where(n => n.Key.MaxCount > n.Key.CookieCount).OrderBy(n => n.Key.CookieCount / n.Key.Weigth).FirstOrDefault();
                                if (qlConfig.Key == null)
                                {
                                    messages.Add($"因无可用青龙容器，未同步Cookie剩余{wfpCookies.Count - i}个。");
                                    break;
                                }
                                qlConfig.Value.Add(item);
                                qlConfig.Key.CookieCount += 1;
                                i++;
                            }
                        }
                        else
                        {
                            messages.Add($"因无可用青龙容器，未同步Cookie剩余{wfpCookies.Count}个。");
                        }
                    }
                    ConcurrentQueue<QLPanelCookie> addArrayList = new ConcurrentQueue<QLPanelCookie>();
                    ConcurrentQueue<QLPanelCookie> updateArrayList = new ConcurrentQueue<QLPanelCookie>();
                    //arrayList.ji
                    foreach (var item in qlCookies)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            var results = QLHttpHelper.AddEnv(item.Key, item.Value.OrderByDescending(n => n.Priority).ThenBy(n => random.Next()).Select(n => new
                            {
                                value = n.CookieToString(),
                                name = "JD_COOKIE",
                                remarks = n.nickname ?? n.PTPin.Decode()
                            }));
                            if (results != null && results.code == 200 && results.data != null)
                            {
                                foreach (var env in results.data)
                                {
                                    try
                                    {
                                        var t = item.Value.SingleOrDefault(n => n.PTPin == CookieHepler.ConvertCookie(env.value)[0].PTPin);
                                        var qpc = qlPanelCookies.FirstOrDefault(n => n.CookieId == t.Id && n.QLPanelId == item.Key.Id && n.Mode == QLPanelCookieMode.User);

                                        if (qpc == null)
                                        {
                                            addArrayList.Enqueue(new QLPanelCookie
                                            {
                                                CookieId = t.Id,
                                                QLPanelId = item.Key.Id,
                                                Mode = QLPanelCookieMode.Auto,
                                                _id = env._id
                                            });
                                        }
                                        else
                                        {
                                            qpc._id = env._id;
                                            updateArrayList.Enqueue(qpc);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        messages.Add($"QQBot中{env.value}的pt_pin有重复，请通过账号管理，检查并删除重复。");
                                    }
                                }
                            }
                        }));
                    }

                    BaseRepository<QLPanelCookie> QLPanelCookieRepository = BaseRepository<QLPanelCookie>.Instance;
                    Task.WaitAll(tasks.ToArray());
                    tasks.Clear();
                    QLPanelCookieRepository.DeleteByIds(QLPanelCookieRepository.Get(new { Mode = QLPanelCookieMode.Auto }).Select(n => n.Id));
                    QLPanelCookieRepository.UpdateRange(updateArrayList);
                    dbContext.QLPanelCookies.AddRange(addArrayList.ToList());
                    dbContext.SaveChanges();
                    BaseRepository<QLConfig>.Instance.UpdateRange(qls);
                    #endregion
                    string qlMessage = "青龙容器情况：";
                    foreach (var item in qls.Where(n => !n.EnableAll))
                    {
                        item.CookieCount = QLPanelCookieRepository.Count(new
                        {
                            QLPanelId = item.Id
                        });
                        qlMessage += $"\r\n{item.Name}：{item.CookieCount}个（最大：{item.MaxCount}）";
                        BaseRepository<QLConfig>.Instance.Update(new { Id = item.Id }, new
                        {
                            CookieCount = item.CookieCount
                        });
                    }
                    messages.Add(qlMessage);
                }
            });
            return messages;
        }
    }
}