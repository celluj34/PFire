using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class FriendRequest : IMessage
    {
        [XMessageField("name")]
        public string Username { get; set; }

        [XMessageField("msg")]
        public string Message { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendRequest;
    }
}
