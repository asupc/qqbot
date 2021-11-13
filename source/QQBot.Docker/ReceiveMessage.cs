using System;

namespace QQBot.Docker
{
    /// <summary>
    /// QQ接受消息
    /// </summary>
    public class ReceiveMessage
    {
        /// <summary>
        /// 上报类型
        /// message: 消息
        /// request：上报
        /// </summary>
        public string post_type { get; set; }

        /// <summary>
        /// 请求类型
        /// friend 添加好友
        /// group 加群/邀请
        /// </summary>
        public string request_type { get; set; }

        /// <summary>
        /// 消息类型
        /// private 私聊
        /// </summary>
        public string message_type { get; set; }


        /// <summary>
        /// 消息子类型, 如果是好友则是 friend, 如果是群临时会话则是 group, 如果是在群中自身发送则是 group_self
        /// </summary>
        public string sub_type { get; set; }

        /// <summary>
        /// 发送者 QQ 号
        /// </summary>
        public long user_id { get; set; }

        /// <summary>
        /// 临时会话来源
        /// </summary>
        public MessageSource? temp_source { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// QQ群
        /// </summary>
        public long group_id { get; set; }

        /// <summary>
        /// 消息id
        /// </summary>
        public int message_id { get; set; }

        /// <summary>
        /// 请求flag
        /// </summary>
        public string flag { get; set; }


        /// <summary>
        /// 验证信息
        /// </summary>
        public string comment { get; set; }
    }

    public enum MessageSource
    {
        群聊 = 0,
        QQ咨询 = 1,
        查找 = 2,
        QQ电影 = 3,
        热聊 = 4,
        验证消息 = 5,
        多人聊天 = 7,
        约会 = 8,
        通讯录 = 9
    }
}
