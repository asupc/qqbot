using Microsoft.EntityFrameworkCore;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Model;
using QQBot.Utils;
using Quartz;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Job
{
    public class QQBotTaskJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            IDbConnection dbConnection = InstallConfigHelper.GetDbConnection;
            QQBotTask taskInfo = (QQBotTask)context.MergedJobDataMap.Get("TaskInfo");
            BaseRepository<JDCookie> JDCookieRepository = BaseRepository<JDCookie>.Init(dbConnection);
            BaseRepository<Env> EnvRepository = BaseRepository<Env>.Init(dbConnection);
            var cookies = QQBotDbContext.Instance.JDCookies.AsNoTracking().Where(n => (taskInfo.ExecAllCookie || n.QQ > 0) && n.Available);
            var envs = EnvRepository.Get(new { Enable = true });
            return Task.Run(() =>
            {
                JDScriptsTask.ExecTask(cookies, taskInfo, envs);
            });
        }
    }
}
