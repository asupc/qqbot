using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Http
{
    public class SaveQLFile
    {
        public string name { get; set; }

        public string content { get; set; }

        public List<string> QLIds { get; set; }
    }
}
