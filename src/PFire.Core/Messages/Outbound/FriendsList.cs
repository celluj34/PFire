using System.Collections.Generic;
using PFire.Core.Enums;
using PFire.Core.Models;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class FriendsList : IMessage
    {
        public FriendsList(UserModel owner)
        {
            Owner = owner;

            UserIds = new List<int>();
            Usernames = new List<string>();
            Nicks = new List<string>();
        }

        public UserModel Owner { get; set; }

        [XMessageField("userid")]
        public List<int> UserIds { get; set; }

        [XMessageField("friends")]
        public List<string> Usernames { get; set; }

        [XMessageField("nick")]
        public List<string> Nicks { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendsList;
    }
}
