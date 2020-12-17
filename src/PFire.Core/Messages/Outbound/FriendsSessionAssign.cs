using System;
using System.Collections.Generic;
using PFire.Core.Enums;
using PFire.Core.Models;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class FriendsSessionAssign : IMessage
    {
        public static readonly Guid FriendIsOffLineSessionId = Guid.Empty;

        public FriendsSessionAssign(UserModel owner)
        {
            Owner = owner;
            UserIds = new List<int>();
            SessionIds = new List<Guid>();
        }

        public UserModel Owner { get; }

        [XMessageField("userid")]
        public List<int> UserIds { get; set; }

        [XMessageField("sid")]
        public List<Guid> SessionIds { get; set; }

        [XMessageField(0x0b)]
        public byte Unknown { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendsSessionAssign;
    }
}
