using PFire.Core.Enums;

namespace PFire.Core.Messages.Bidirectional
{
    internal sealed class ChatAcknowledgement : IMessage
    {
        public XFireMessageType MessageTypeId => XFireMessageType.ServerChatMessage;
    }
}
