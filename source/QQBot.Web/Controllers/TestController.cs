using Microsoft.AspNetCore.Mvc;
using QQBot.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QQBot.Web.Controllers
{
    public class TestController : Controller
    {
        MessageProcess messageProcess;
        public TestController(MessageProcess messageProcess)
        {
            this.messageProcess = messageProcess;
        }
        public bool Index()
        {
            messageProcess.Message(new Entities.Socket.ReceiveMessage { message = "我的账号", user_id = 731579396 });

            return true;
        }
    }
}
