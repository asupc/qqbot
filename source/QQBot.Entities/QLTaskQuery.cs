using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities
{
    public class QLTaskQuery
    {
        public string Key { get; set; }
        public string ContainerId { get; set; }

        public int? isDisabled { get; set; }

        public int? status { get; set; }
    }
}
