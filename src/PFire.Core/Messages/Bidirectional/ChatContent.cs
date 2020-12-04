using PFire.Core.Enums;

namespace PFire.Core.Messages.Bidirectional
{
    internal sealed class ChatContent : IMessage
    {
        [XMessageField("imindex")]
        public int MessageOrderIndex { get; set; }

        [XMessageField("im")]
        public string MessageContent { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ServerChatMessage;
    }
}
