using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Model;
using QQBot.Utils;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQBot.Job
{
    public class FaskRequestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var cookies = QQBotDbContext.Instance.JDCookies.OrderByDescending(n => n.Priority).Take(1).ToList();

            var url_111 = "https://m.jingxi.com/jxbfd/user/ExchangePrize?strZone=jxbfd&bizCode=jxbfd&source=jxbfd&dwEnv=7&_cfd_t=1636023683177&ptag=7155.9.47&dwType=3&dwLvl=2&ddwPaperMoney=111000&strPoolName=jxcfd2_exchange_hb_202111&strPgtimestamp=1636023683174&strPhoneID=4feb05767d52bbd4c646ecb45dbe39c15dad474c&strPgUUNum=47bd4f62f17e0c219161ed88ab17aa0b&_stk=_cfd_t%2CbizCode%2CddwPaperMoney%2CdwEnv%2CdwLvl%2CdwType%2Cptag%2Csource%2CstrPgUUNum%2CstrPgtimestamp%2CstrPhoneID%2CstrPoolName%2CstrZone&_ste=1&h5st=20211104190123177%3B6686047562101162%3B10032%3Btk01w99b21be230nAOFAcwt3R%2Bwt8Su43SjWQlkq%2BcxNoBemMdWWMh%2FKw75GbDG7yjXJ8Ws8uVkM8o9Q2u%2BSqjEOyWzA%3B37abbc6cb83c35ba31294c414ea74b136595c42119bc2854146ca011b5cb91a5&_=1636023683179&sceneval=2&g_login_type=1&callback=jsonpCBKFF&g_ty=ls";
            var url_100 = "https://m.jingxi.com/jxbfd/user/ExchangePrize?strZone=jxbfd&bizCode=jxbfd&source=jxbfd&dwEnv=7&_cfd_t=1636023613229&ptag=7155.9.47&dwType=3&dwLvl=3&ddwPaperMoney=100000&strPoolName=jxcfd2_exchange_hb_202111&strPgtimestamp=1636023613225&strPhoneID=4feb05767d52bbd4c646ecb45dbe39c15dad474c&strPgUUNum=9f05849196bcfc39d9fc0b169d7f3318&_stk=_cfd_t%2CbizCode%2CddwPaperMoney%2CdwEnv%2CdwLvl%2CdwType%2Cptag%2Csource%2CstrPgUUNum%2CstrPgtimestamp%2CstrPhoneID%2CstrPoolName%2CstrZone&_ste=1&h5st=20211104190013229%3B6686047562101162%3B10032%3Btk01w99b21be230nAOFAcwt3R%2Bwt8Su43SjWQlkq%2BcxNoBemMdWWMh%2FKw75GbDG7yjXJ8Ws8uVkM8o9Q2u%2BSqjEOyWzA%3B8dd91826aa47d94e73b75ee02cab2bb44bbe511ccf7db32b512ce63b932a30dd&_=1636023613231&sceneval=2&g_login_type=1&callback=jsonpCBKEE&g_ty=ls";

            var url_11 = "https://m.jingxi.com/jxbfd/user/ExchangePrize?strZone=jxbfd&bizCode=jxbfd&source=jxbfd&dwEnv=7&_cfd_t=1636023968842&ptag=7155.9.47&dwType=3&dwLvl=4&ddwPaperMoney=11000&strPoolName=jxcfd2_exchange_hb_202111&strPgtimestamp=1636023968834&strPhoneID=4feb05767d52bbd4c646ecb45dbe39c15dad474c&strPgUUNum=7739085fa2b28d151be8dc935f82413f&_stk=_cfd_t%2CbizCode%2CddwPaperMoney%2CdwEnv%2CdwLvl%2CdwType%2Cptag%2Csource%2CstrPgUUNum%2CstrPgtimestamp%2CstrPhoneID%2CstrPoolName%2CstrZone&_ste=1&h5st=20211104190608842%3B6686047562101162%3B10032%3Btk01w99b21be230nAOFAcwt3R%2Bwt8Su43SjWQlkq%2BcxNoBemMdWWMh%2FKw75GbDG7yjXJ8Ws8uVkM8o9Q2u%2BSqjEOyWzA%3Bbca02616847fc243a6671800ebd470f83802865437780c76c08570b669d6a812&_=1636023968844&sceneval=2&g_login_type=1&callback=jsonpCBKJJ&g_ty=ls";


            List<Thread> threads = new List<Thread>();

            foreach (var cookie in cookies)
            {
                Task.Run(() =>
                {
                    JDScriptsTask.ExecFast(cookie, "qqbot_jx_fcd", new List<Env>
                          {
                            new Env{  Name = "URL",Value = url_100}
                          });
                });
            }
            return Task.CompletedTask;
        }
    }
}