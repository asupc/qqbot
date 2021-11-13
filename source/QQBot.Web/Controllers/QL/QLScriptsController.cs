using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQBot.Application;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers.QL
{
    public class QLScriptsController : BaseController
    {
        private QLHttpService QLHttpHelper;

        public QLScriptsController(QLHttpService httpHelper)
        {
            QLHttpHelper = httpHelper;
        }

        [HttpGet("{qlId}")]
        public ResultModel<List<QLFiles>> QLFiles([FromRoute] string qlId)
        {
            var s = BaseRepository<QLConfig>.Instance.GetById(qlId);
            if (s == null)
            {
                return ResultModel<List<QLFiles>>.Error("获取青龙配置文件错误，青龙容器信息异常.");
            }
            return ResultModel<List<QLFiles>>.Success(QLHttpHelper.GetQLScriptFiles(s));
        }

        [HttpGet("{qlId}/{file}")]
        public ResultModel<string> QLFile([FromRoute] string qlId, [FromRoute] string file)
        {
            var s = BaseRepository<QLConfig>.Instance.GetById(qlId);
            if (s == null)
            {
                return ResultModel<string>.Error("获取青龙配置文件错误，青龙容器信息异常.");
            }
            return ResultModel<string>.Success(QLHttpHelper.GetQLScriptFile(s, file));
        }

        [HttpPost("save")]
        public ResultModel SaveFile([FromBody] SaveQLFile file)
        {
            var s = QQBotDbContext.Instance.QLConfigs.AsNoTracking().Where(n => file.QLIds.Contains(n.Id));
            foreach (var item in s)
            {
                QLHttpHelper.UpdateScript(item, file.name, file.content);
            }
            return ResultModel.Success();
        }
    }
}
