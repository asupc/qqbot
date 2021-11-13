using System;
using System.Collections.Generic;
using System.Text;

namespace QQBot.Entities.Http
{

    /// <summary>
    /// 处理加好友请求
    /// </summary>
    public class FriendAddRequest
    {
        /// <summary>
        /// 加好友请求的 flag（需从上报的数据中获得）
        /// </summary>
        public string flag { get; set; }

        public bool approve { get; set; } = true;

    }
}
