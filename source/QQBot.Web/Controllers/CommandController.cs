using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Http;

namespace QQBot.Web.Controllers
{
    public class CommandController : BaseController
    {

        BaseRepository<Command> CommandRepository;

        public CommandController(IDbConnection dbConnection)
        {
            CommandRepository = BaseRepository<Command>.Init(dbConnection);
        }

        [HttpGet]
        public ResultModel<IEnumerable<Command>> Get()
        {
            return new ResultModel<IEnumerable<Command>>
            {
                Code = 200,
                Data = QQBotDbContext.Instance.Commands.AsNoTracking()
            };
        }

        [HttpDelete("{id}")]
        public ResultModel Delete([FromRoute] string id)
        {
            CommandRepository.DeleteById(id);
            return new ResultModel();
        }

        [HttpPost]
        public ResultModel Add([FromBody] Command command)
        {
            var c = CommandRepository.GetById(command.Id);
            if (c != null)
            {
                if (c.Key != command.Key && CommandRepository.IsExist(new { command.Key }))
                {
                    return ResultModel<bool>.Error("快捷回复指令重复，请修改后保存。");
                }
                CommandRepository.Update(command);
            }
            else
            {
                if (CommandRepository.IsExist(new { command.Key }))
                {
                    return ResultModel<bool>.Error("快捷回复指令重复，请修改后保存。");
                }
                command.Id = Guid.NewGuid().ToString().Replace("-", "");
                CommandRepository.Add(command);
            }
            return new ResultModel();
        }


        [HttpGet("{id}")]
        public ResultModel<Command> GetById([FromRoute] string id)
        {
            return new ResultModel<Command>
            {
                Code = 200,
                Data = CommandRepository.GetById(id)
            };
        }
    }
}
