using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class FriendInvite : IMessage
    {
        public FriendInvite()
        {
            Usernames = new List<string>();
            Nicknames = new List<string>();
            Messages = new List<string>();
        }

        [XMessageField("name")]
        public List<string> Usernames { get; set; }

        [XMessageField("nick")]
        public List<string> Nicknames { get; set; }

        [XMessageField("msg")]
        public List<string> Messages { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendInvite;
    }
}
