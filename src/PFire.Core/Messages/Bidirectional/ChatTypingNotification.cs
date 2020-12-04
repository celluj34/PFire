using PFire.Core.Enums;

namespace PFire.Core.Messages.Bidirectional
{
    // the typing notification is a sub message from the chat message and 
    // not a separate message in of itself

    internal sealed class ChatTypingNotification : IMessage
    {
        [XMessageField("imindex")]
        public int OrderIndex { get; set; }

        [XMessageField("typing")]
        public int Typing { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ServerChatMessage;
    }
}
