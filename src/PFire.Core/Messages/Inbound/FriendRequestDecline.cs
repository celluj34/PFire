using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class FriendRequestDecline : IMessage
    {
        [XMessageField("name")]
        public string RequesterUsername { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendRequestDecline;
    }
}
