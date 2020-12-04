using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class GroupsFriends : IMessage
    {
        public GroupsFriends()
        {
            UserIds = new List<int>();
            GroupIds = new List<int>();
        }

        [XMessageField(0x01)]
        public List<int> UserIds { get; set; }

        [XMessageField(0x19)]
        public List<int> GroupIds { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.GroupsFriends;
    }
}
