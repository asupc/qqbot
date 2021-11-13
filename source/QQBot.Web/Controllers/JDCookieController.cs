using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using QQBot.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class JDCookieController : BaseController
    {

        private readonly ILog log = LogManager.GetLogger("NETCoreRepository", typeof(JDCookieController));
        BaseRepository<JDCookie> JDCookieRepository;
        QLPanelService qlHelper;
        QQBotDbContext DbContext;
        BaseRepository<QLPanelCookie> QLPanelCookieRepository;
        JDCookieService jDCookieService;
        public JDCookieController(QLPanelService QLHelper, QLHttpService qLHttpHelper, JDCookieService jDCookieService)
        {
            JDCookieRepository = BaseRepository<JDCookie>.Instance;
            this.qlHelper = QLHelper;
            DbContext = QQBotDbContext.Instance;
            QLPanelCookieRepository = BaseRepository<QLPanelCookie>.Instance;
            this.jDCookieService = jDCookieService;
        }

        [HttpPost("UpdateEnable/{status}")]
        public ResultModel<bool> UpdateEnable([FromBody] List<string> ids, [FromRoute] bool status)
        {
            JDCookieRepository.Execute("update t_JDCookie set Available=@status where Id in @ids", new { status, ids });
            return new ResultModel<bool> { Data = true };
        }


        [HttpPost("deletes")]
        public ResultModel Delete([FromBody] List<string> ids)
        {
            JDCookieRepository.DeleteRange(jDCookieService.Delete(ids));
            return new ResultModel();
        }


        [HttpDelete("{id}")]
        public ResultModel Delete([FromRoute] string id)
        {
            JDCookieRepository.DeleteRange(jDCookieService.Delete(new List<string> { id }));
            return new ResultModel();
        }

        [HttpGet]
        public ResultModel<List<JDCookie>> Index(string key, string containerId, bool? available)
        {
            return new ResultModel<List<JDCookie>>
            {
                Code = 200,
                Data = jDCookieService.GetJDCookies(key, containerId, available)
            };
        }

        [HttpGet("{id}")]
        public ResultModel<JDCookie> GetById([FromRoute] string id)
        {
            var cookie = JDCookieRepository.GetById(id);
            cookie.QLPanelCookies = QLPanelCookieRepository.Get(new { CookieId = id });

            return new ResultModel<JDCookie>
            {
                Data = cookie
            };
        }

        [HttpPut]
        public ResultModel Update([FromBody] JDCookie jDCookie)
        {
            JDCookieRepository.Update(new { jDCookie.Id }, new
            {
                jDCookie.QQ,
                jDCookie.Priority,
                jDCookie.Remark
            });
            if (jDCookie.QLPanelCookies.Any(n => n.Mode == QLPanelCookieMode.User))
            {
                QLPanelCookieRepository.DeleteRange(QLPanelCookieRepository.Get(new { CookieId = jDCookie.Id }));
                DbContext.QLPanelCookies.AddRange(jDCookie.QLPanelCookies.Select(n => new QLPanelCookie
                {
                    CookieId = jDCookie.Id,
                    Mode = QLPanelCookieMode.User,
                    QLPanelId = n.QLPanelId,
                    Id = Guid.NewGuid().ToString().Replace("-", "")
                }));
                DbContext.SaveChanges();
            }
            return new ResultModel<bool> { Data = true };
        }

        [HttpGet("Sync")]
        public async Task<ResultModel> Sync()
        {
            await qlHelper.SyncJDCookies();
            return ResultModel.Success();
        }

        [HttpPost("add")]
        [AllowAnonymous]
        public async Task<bool> Add([FromBody] JDCookie cookie)
        {
            try
            {
                var cc = JDCookieRepository.Get(new { cookie.PTPin }).FirstOrDefault();
                cookie.nickname = string.IsNullOrEmpty(cookie.nickname) ? cookie.PTPin.Decode() : cookie.nickname;
                if (cc != null)
                {
                    cc.PTKey = cookie.PTKey;
                    cc.QQ = cookie.QQ;
                    cc.Available = true;
                    cc.nickname = cookie.nickname;
                    JDCookieRepository.Update(cc);
                }
                else
                {
                    cookie.Id = Guid.NewGuid().ToString().Replace("-", "");
                    cookie.Available = true;
                    JDCookieRepository.Add(cookie);
                }
                //await qlHelper.SyncJDCookies();
            }
            catch (Exception e)
            {
                log.Error("CheckCookie 调用 Add 异常：" + e.Message, e);
            }
            return true;
        }

        [HttpPost("Disable")]
        [AllowAnonymous]
        public bool Disable([FromBody] JDCookie cookie)
        {
            var cc = JDCookieRepository.Get(new
            {
                cookie.PTKey,
                cookie.PTPin
            }).FirstOrDefault();
            if (cc != null)
            {
                cc.Available = false;
                JDCookieRepository.Update(cc);
                jDCookieService.Delete(new List<string> { cc.Id });
            }
            return true;
        }
    }
}
