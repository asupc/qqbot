namespace QQBot.Entities.Http
{
    public class GroupAddRequest
    {
        /// <summary>
        /// 加群请求的 flag（需从上报的数据中获得）
        /// </summary>
        public string flag { get; set; }

        /// <summary>
        /// add 或 invite, 请求类型（需要和上报消息中的 sub_type 字段相符）
        /// </summary>
        public string sub_type { get; set; }

        public bool approve { get; set; } = true;
    }
}
