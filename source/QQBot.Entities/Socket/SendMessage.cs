namespace QQBot.Entities.Socket
{
    public class SendMessage
    {
        /// <summary>
        /// 消息类型, 支持 private、group , 分别对应私聊、群组, 如不传入, 则根据传入的 *_id 参数判断
        /// </summary>
        public string message_type { get; set; }

        /// <summary>
        /// 对方 QQ 号 ( 消息类型为 private 时需要 )
        /// </summary>
        public long user_id { get; set; }

        /// <summary>
        /// 群号 ( 消息类型为 group 时需要 )
        /// </summary>
        public string group_id { get; set; }

        /// <summary>
        /// 要发送的内容
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 消息内容是否作为纯文本发送 ( 即不解析 CQ 码 ) , 只在 message 字段是字符串时有效
        /// </summary>
        public bool auto_escape { get; set; }
    }
}