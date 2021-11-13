using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Http
{

    public class QLResult
    {
        public int code { get; set; }
    }
    public class QLResult<T> : QLResult
    {

        public T data { get; set; }
    }
}
