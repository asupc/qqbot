using QQBot.DB;
using QQBot.Utils;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Job
{
    public class QueryJob : IJob
    {
        GoCQHttpHelper goCQHttpHelper;
        public QueryJob(GoCQHttpHelper goCQHttpHelper)
        {
            this.goCQHttpHelper = goCQHttpHelper;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
//                var cookies = dbContext.JDCookies.Where(n => n.Available && n.QQ > 0).ToList();
//                foreach (var cookie in cookies)
//                {
//                     await Task.Run(async () =>
//                     {
//                         var result = await scriptsTask.BeanChange(cookie.CookieToString(), "push_huodong");
//                         if (result != null && !string.IsNullOrEmpty(result.Trim()))
//                         {
//                             goCQHttpHelper.Send(@$"京东账号：{cookie.nickname ?? cookie.PTPin}
//{result}", null, cookie.QQ);
//                         }
//                     });
//                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
