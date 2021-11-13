using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQBot.DB;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using QQBot.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QQBot.Web.Controllers
{
    public class EnvController : BaseController
    {

        BaseRepository<Env> EnvRepository;
        public EnvController(IDbConnection dbConnection)
        {
            EnvRepository = BaseRepository<Env>.Init(dbConnection);
        }

        [HttpGet]
        public ResultModel<IEnumerable<Env>> Index([FromQuery] string key)
        {
            var datas = QQBotDbContext.Instance.Envs.AsNoTracking();
            return new ResultModel<IEnumerable<Env>>
            {
                Data = datas.Where(n => string.IsNullOrEmpty(key) || n.Name.Contains(key) || n.Value.Contains(key) || n.Remark.Contains(key))
            };
        }

        [HttpGet("{id}")]
        public ResultModel<Env> GetById([FromRoute] string id)
        {
            return new ResultModel<Env>
            {
                Data = EnvRepository.GetById(id)
            };
        }

        [HttpPost("UpdateEnable/{status}")]
        public ResultModel<bool> UpdateEnable([FromBody] List<string> ids, [FromRoute] bool status)
        {
            EnvRepository.DeleteByIds(ids);
            return new ResultModel<bool> { Data = true };
        }


        [HttpPost]
        public ResultModel<bool> Update([FromBody] Env env)
        {
            var t = EnvRepository.GetById(env.Id);
            if (t != null)
            {
                t.Name = env.Name;
                t.Enable = env.Enable;
                t.Value = env.Value;
                t.Remark = env.Remark;

                EnvRepository.Update(env);
            }
            else
            {
                env.Id = Guid.NewGuid().ToString().Replace("-", "");
                EnvRepository.Add(env);
            }
            return new ResultModel<bool> { Data = true };
        }

        [HttpPost("deletes")]
        public ResultModel Delete([FromBody] List<string> ids)
        {
            EnvRepository.DeleteByIds(ids);
            return new ResultModel();
        }

        [HttpDelete("{id}")]
        public ResultModel Delete([FromRoute] string id)
        {
            EnvRepository.DeleteById(id);
            return new ResultModel();
        }
    }
}
