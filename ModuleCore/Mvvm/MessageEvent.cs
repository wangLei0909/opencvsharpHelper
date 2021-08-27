using Prism.Events;

namespace ModuleCore.Mvvm
{
    public class MessageEvent : PubSubEvent<Message>
    {
    }

    public struct Message
    {
        /// <summary>
        /// 消息目标
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Content { get; set; }
    }

    public class BytesEvent : PubSubEvent<BytesEventData>
    {
    }

    public struct BytesEventData
    {
        /// <summary>
        /// 事件目标
        /// </summary>
        public string EventTarget { get; set; }

        /// <summary>
        /// 数据目标
        /// </summary>
        public string DataTarget { get; set; }

        public byte[] Data { get; set; }
    }
}