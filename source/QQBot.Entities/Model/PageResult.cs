using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Model
{
    public class PageResult<T>
    {
        public int Total { get; set; }

        public List<T> Datas { get; set; }
    }
}
