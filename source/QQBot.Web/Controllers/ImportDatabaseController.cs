using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class ImportDatabaseController : BaseController
    {

        QLPanelService QLHelper;
        BaseRepository<JDCookie> JDCookieRepository;
        public ImportDatabaseController(IDbConnection dbConnection, QLPanelService QLHelper)
        {
            JDCookieRepository = BaseRepository<JDCookie>.Init(dbConnection);
            this.QLHelper = QLHelper;
        }

        [HttpPost]
        public async Task<ResultModel> Import([FromBody] ImportData importData)
        {
            try
            {
                IDbConnection dbConnection;
                if (importData.DBType.ToLower() == "MySQL".ToLower())
                {
                    dbConnection = new MySqlConnection(importData.Address);
                }
                else
                {
                    dbConnection = new SqliteConnection("Filename=" + importData.Address);
                }

                var xddCookies = BaseRepository<XDDCookie>.Init(dbConnection).Get(new { Available = "true" }).ToList();
                var jdCookies = QQBotDbContext.Instance.JDCookies.AsNoTracking();
                foreach (var jdCookie in jdCookies)
                {
                    var t = xddCookies.SingleOrDefault(n => n.PtPin == jdCookie.PTPin);
                    if (t != null)
                    {
                        xddCookies.Remove(t);
                    }
                }
                JDCookieRepository.AddRange(xddCookies.Select(n => new JDCookie
                {
                    QQ = n.QQ,
                    PTKey = n.PtKey,
                    PTPin = n.PtPin,
                    nickname = n.Nickname,
                    Priority = (int)n.Priority,
                    Available = true
                }));
                await QLHelper.SyncJDCookies();
            }
            catch (Exception e)
            {
                return new ResultModel { Code = 500, Message = "同步失败，请确认您的配置是否正确。" };
            }
            return new ResultModel();
        }
    }
}