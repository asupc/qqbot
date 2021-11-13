using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using QQBot.Utils;
using System;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading;


namespace QQBot.Web.Controllers
{
    public class InstallController : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public ResultModel<bool> GetInstallStatus()
        {
            ResultModel<bool> resultModel = new ResultModel<bool>
            {
                Data = InstallConfigHelper.Get() != null
            };
            return resultModel;
        }

        [HttpPost]
        [AllowAnonymous]
        public ResultModel<string> Install([FromForm] InstallConfig userConfig)
        {
            if (!Directory.Exists("./config"))
            {
                Directory.CreateDirectory("./config");
            }
            //if()
            if (System.IO.File.Exists(InstallConfigHelper.installConfigPath))
            {
                return new ResultModel<string>
                {
                    Code = 500,
                    Message = "已初始化，需要重新初始化请删除InstallConfig.xml后重试。"
                };
            }
            InstallConfigHelper.Set(userConfig);
            ResultModel<string> resultModel = new ResultModel<string>();
            return resultModel;
        }
    }
}