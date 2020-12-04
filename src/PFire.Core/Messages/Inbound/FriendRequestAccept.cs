using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class FriendRequestAccept : IMessage
    {
        [XMessageField("name")]
        public string FriendUsername { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendRequestAccept;
    }
}
