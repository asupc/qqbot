using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QQBot.DB;
using QQBot.Entities.Config;
using QQBot.Entities.Http;
using QQBot.Entities.Model;
using QQBot.Utils;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Application
{
    public class QLHttpService
    {
        private readonly ILog log = LogManager.GetLogger("NETCoreRepository", typeof(QLHttpService));

        private BaseRepository<QLConfig> QLConfigRepository;
        public QLHttpService()
        {
            this.QLConfigRepository = BaseRepository<QLConfig>.Instance;
        }

        /// <summary>
        /// 获取青龙授权Token
        /// </summary>
        /// <param name="config"></param>
        public void GetToken(QLConfig config)
        {
            try
            {
                if (!string.IsNullOrEmpty(config.ClientID) && !string.IsNullOrEmpty(config.ClientSecret))
                {
                    var openToken = JsonConvert.DeserializeObject<JsonObject>(HttpClientHelper.Get<JsonObject>($"{config.Address}/open/auth/token?client_id={config.ClientID}&client_secret={config.ClientSecret}")["data"].ToString());
                    config.Token = openToken["token"].ToString();
                    config.TokeType = "open";
                }
                QLConfigRepository.Update(config);
            }
            catch (Exception e)
            {
                log.Error("GetQLToken", e);
            }
        }

        public QLResult<List<QLenv>> GetCookies(QLConfig config, int tryCount = 1)
        {
            var result = HttpClientHelper.Get<QLResult<List<QLenv>>>($"{config.Address}/{config.TokeType}/envs?searchValue=JD_COOKIE&t={DateTime.Now.ToUnix()}", config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("GetCookies 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return GetCookies(config, --tryCount);
                }
            }
            return result ?? new QLResult<List<QLenv>> { data = new List<QLenv>() };
        }

        /// <summary>
        /// 添加青龙环境变量
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public QLResult<List<QLenv>> AddEnv(QLConfig config, IEnumerable<object> cookies, int tryCount = 2)
        {
            var result = HttpClientHelper.Post<QLResult<List<QLenv>>>($"{config.Address}/{config.TokeType}/envs?t=1631518978531", JsonConvert.SerializeObject(cookies), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("AddEnv 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return AddEnv(config, cookies, --tryCount);
                }
            }
            return result;
        }

        /// <summary>
        /// 添加青龙环境变量
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public QLResult<QLenv> UpdateEnv(QLConfig config, QLenv cookie, int tryCount = 2)
        {
            var result = HttpClientHelper.Put<QLResult<QLenv>>($"{config.Address}/{config.TokeType}/envs?t=1631518978531", new
            {
                cookie._id,
                cookie.name,
                cookie.remarks,
                cookie.value
            }, config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("UpdateEnv 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return UpdateEnv(config, cookie, --tryCount);
                }
            }
            return result;
        }

        public QLResult DeleteEnv(QLConfig config, IEnumerable<string> ids, int tryCount = 2)
        {
            if (!ids.Any())
            {
                return new QLResult();
            }
            var result = HttpClientHelper.Delete<QLResult>($"{config.Address}/{config.TokeType}/envs?t=" + DateTime.Now.ToUnix(), ids, config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("DeleteEnv 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return DeleteEnv(config, ids, --tryCount);
                }
            }
            return result;
        }

        public List<QLTaskHttpResult> GetQLTasks(QLConfig config, string searchValue, int tryCount = 2)
        {
            var result = HttpClientHelper.Get<QLResult<List<QLTaskHttpResult>>>($"{config.Address}/{config.TokeType}/crons?searchValue={searchValue}&t=" + DateTime.Now.ToUnix(), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("GetQLTasks 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return GetQLTasks(config, searchValue, --tryCount);
                }
            }
            return result.data;
        }

        public bool DeleteTask(QLConfig config, List<string> ids, int tryCount = 2)
        {
            var result = HttpClientHelper.Delete<QLResult>($"{config.Address}/{config.TokeType}/crons?&t=" + DateTime.Now.ToUnix(), ids, config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("DeleteTask 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return DeleteTask(config, ids, --tryCount);
                }
            }
            return true;
        }
        public bool UpdateTask(QLConfig config, object task, int tryCount = 2)
        {
            var result = HttpClientHelper.Put<QLResult>($"{config.Address}/{config.TokeType}/crons?&t=" + DateTime.Now.ToUnix(), task, config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("UpdateTask 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return UpdateTask(config, task, --tryCount);
                }
            }
            return true;
        }

        public string ScriptLog(QLConfig config, string scriptId, int tryCount = 2)
        {
            var result = HttpClientHelper.Get<QLResult<string>>($"{config.Address}/{config.TokeType}/crons/{scriptId}/log/?&t=" + DateTime.Now.ToUnix(), config.Token);

            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("ScriptLog 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return ScriptLog(config, scriptId, --tryCount);
                }
            }
            return result.data;
        }

        public bool UpdateScript(QLConfig config, string fileName, string content, int tryCount = 2)
        {
            var result = HttpClientHelper.Post<QLResult>($"{config.Address}/{config.TokeType}/scripts?&t=" + DateTime.Now.ToUnix(), JsonConvert.SerializeObject(new
            {
                filename = fileName,
                content = content
            }), config.Token);

            Console.WriteLine($"上传{fileName}到容器：{config.Name}结果：{JsonConvert.SerializeObject(result)}");
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("UpdateScript 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return UpdateScript(config, fileName, content, --tryCount);
                }
            }
            return true;
        }

        public string GetQLScriptFile(QLConfig config, string file, int tryCount = 2)
        {
            var result = HttpClientHelper.Get<QLResult<string>>($"{config.Address}/{config.TokeType}/scripts/{file}?&t=" + DateTime.Now.ToUnix(), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("GetQLScriptFile 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return GetQLScriptFile(config, file, --tryCount);
                }
            }
            return result.data;
        }


        public List<QLFiles> GetQLScriptFiles(QLConfig config, int tryCount = 2)
        {
            var result = HttpClientHelper.Get<QLResult<List<QLFiles>>>($"{config.Address}/{config.TokeType}/scripts/files?&t=" + DateTime.Now.ToUnix(), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("GetQLScriptFiles 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return GetQLScriptFiles(config, --tryCount);
                }
            }
            return result.data;
        }


        public string GetQLConfigFile(QLConfig config, string file, int tryCount = 2)
        {
            var result = HttpClientHelper.Get<QLResult<string>>($"{config.Address}/{config.TokeType}/configs/{file}?&t=" + DateTime.Now.ToUnix(), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("GetQLConfigFile 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return GetQLConfigFile(config, file, --tryCount);
                }
            }
            return result.data;
        }


        public List<QLFiles> GetQLConfigFiles(QLConfig config, int tryCount = 2)
        {
            var result = HttpClientHelper.Get<QLResult<List<QLFiles>>>($"{config.Address}/{config.TokeType}/configs/files?&t=" + DateTime.Now.ToUnix(), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("GetQLConfigFiles 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return GetQLConfigFiles(config, --tryCount);
                }
            }
            return result.data;
        }



        public bool SaveConfigFile(QLConfig config, SaveQLFile file, int tryCount = 2)
        {
            var result = HttpClientHelper.Post<QLResult>($"{config.Address}/{config.TokeType}/configs/save?t=" + DateTime.Now.ToUnix(), JsonConvert.SerializeObject(new
            {
                file.name,
                file.content
            }), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("UpdateScript 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return SaveConfigFile(config, file, --tryCount);
                }
            }
            return true;
        }



        public bool AddTask(QLConfig config, object task, int tryCount = 2)
        {
            var result = HttpClientHelper.Post<QLResult>($"{config.Address}/{config.TokeType}/crons?&t=" + DateTime.Now.ToUnix(), JsonConvert.SerializeObject(task), config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("AddTask 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return AddTask(config, task, --tryCount);
                }
            }
            return true;
        }

        /// <summary>
        /// disable/enable/stop/run
        /// </summary>
        /// <param name="config"></param>
        /// <param name="ids"></param>
        /// <param name="status"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        public bool ChangeTaskStatus(QLConfig config, List<string> ids, string status, int tryCount = 2)
        {
            var result = HttpClientHelper.Put<QLResult>($"{config.Address}/{config.TokeType}/crons/{status}?t=" + DateTime.Now.ToUnix(), ids, config.Token);
            if (result == null || result.code == 401)
            {
                if (tryCount > 0)
                {
                    Console.WriteLine("ChangeTaskStatus 青龙授权超时，重试一次，剩余重试次数：" + tryCount);
                    GetToken(config);
                    return DeleteTask(config, ids, --tryCount);
                }
            }
            return true;
        }
    }
}
