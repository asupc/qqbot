using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using QQBot.Utils;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QQBot.Web.Controllers
{
    public class LoginController : BaseController
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ResultModel<string> Login([FromForm] InstallConfig login)
        {
            var resultModel = new ResultModel<string>();
            var installConfig = InstallConfigHelper.Get();
            if (installConfig == null)
            {
                resultModel.Code = 500;
                resultModel.Message = "用户，数据库未初始化，请初始化后重启容器登录！";
            }
            if (!string.IsNullOrEmpty(installConfig.PassWord) && login.PassWord == installConfig.PassWord && login.UserName == installConfig.UserName)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                    new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddMinutes(60*24*7)).ToUnixTimeSeconds()}"),
                    new Claim("Name", installConfig.UserName)
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QQBot.JWT.Token.SecurityKey"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                         "http//:localhost",//Audience
                         "http//:localhost",//Issuer，这两项和前面签发jwt的设置一致
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60 * 24 * 7),
                    signingCredentials: creds);
                resultModel.Data = new JwtSecurityTokenHandler().WriteToken(token);
            }
            else
            {
                resultModel.Code = 401;
                resultModel.Message = "登录失败，用户名密码错误！";
            }
            return resultModel;
        }

        [HttpGet("Version")]
        [AllowAnonymous]
        public ResultModel<string> Version()
        {
            return ResultModel<string>.Success(Extends.Version);
        }
    }
}
