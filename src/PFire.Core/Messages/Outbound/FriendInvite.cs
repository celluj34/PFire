using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class FriendInvite : IMessage
    {
        public FriendInvite(string username, string nickname, string message)
        {
            Usernames = new List<string>
            {
                username
            };

            Nicknames = new List<string>
            {
                nickname
            };

            Messages = new List<string>
            {
                message
            };
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
