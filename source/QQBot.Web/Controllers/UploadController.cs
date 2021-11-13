using Microsoft.AspNetCore.Mvc;
using QQBot.Entities.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class UploadController : BaseController
    {
        [HttpPost]
        public async Task<ResultModel<object>> Index()
        {
            var file = Request.Form.Files[0];
            string basePath = "./db/import/";
            //if (!Directory.Exists(basePath))
            //{
            //    Directory.CreateDirectory(basePath);
            //}
            var filePath = basePath + file.FileName;

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return new ResultModel<object>()
            {
                Data = new
                {
                    Path = filePath,
                    FileName = file.FileName
                }
            };
        }

        [HttpPost("scripts")]
        public async Task<ResultModel<object>> UploadScripts([FromQuery] string dir)
        {
            var file = Request.Form.Files[0];
            string basePath = "./scripts/";
            if (!string.IsNullOrEmpty(dir))
            {
                basePath = $"./scripts/{dir}/";
            }
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            var filePath = basePath + file.FileName;

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return new ResultModel<object>()
            {
                Data = new
                {
                    Path = filePath,
                    FileName = file.FileName
                }
            };
        }
    }
}
