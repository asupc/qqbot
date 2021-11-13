using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class QLPanelController : BaseController
    {
        BaseRepository<QLConfig> QLConfigRepository;
        QLPanelService qlHelper;
        QLHttpService qLHttpHelper;
        IDbConnection DbConnection;
        public QLPanelController(IDbConnection dbConnection, QLPanelService QLHelper, QLHttpService qLHttpHelper)
        {
            DbConnection = dbConnection;
            QLConfigRepository = BaseRepository<QLConfig>.Init(dbConnection);
            this.qlHelper = QLHelper;
            this.qLHttpHelper = qLHttpHelper;
        }

        [HttpGet]
        public ResultModel<IEnumerable<QLConfig>> Index()
        {
            return new ResultModel<IEnumerable<QLConfig>>
            {
                Data = QQBotDbContext.Instance.QLConfigs.AsNoTracking().OrderBy(n => n.Name)
            };
        }

        [HttpGet("{id}")]
        public ResultModel<QLConfig> GetById([FromRoute] string id)
        {
            return new ResultModel<QLConfig>
            {
                Data = QLConfigRepository.GetById(id)
            };
        }

        [HttpPost]
        public ResultModel<bool> Update([FromBody] QLConfig qLConfig)
        {
            qLConfig.Token = null;
            qLHttpHelper.GetToken(qLConfig);
            if (string.IsNullOrEmpty(qLConfig.Token))
            {
                return new ResultModel<bool>
                {
                    Code = 201,
                    Message = "面板信息认证失败，请检查后重试！",
                    Data = false
                };
            }
            var t = QLConfigRepository.GetById(qLConfig.Id);
            if (t != null)
            {
                QLConfigRepository.Update(new { qLConfig.Id }, new
                {
                    qLConfig.Name,
                    //qLConfig.PassWord,
                    //qLConfig.UserName,
                    qLConfig.Address,
                    qLConfig.ClientSecret,
                    qLConfig.ClientID,
                    qLConfig.EnableAll,
                    qLConfig.Weigth,
                    qLConfig.MaxCount
                });
            }
            else
            {
                qLConfig.Id = Guid.NewGuid().ToString().Replace("-", "");
                QLConfigRepository.Add(qLConfig);
            }
            return new ResultModel<bool> { Data = true };
        }

        [HttpDelete("{id}")]
        public ResultModel Delete([FromRoute] string id)
        {
            QLConfigRepository.DeleteById(id);
            return new ResultModel();
        }
    }
}
