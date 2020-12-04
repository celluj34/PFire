using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class Groups : IMessage
    {
        [XMessageField(0x19)]
        public List<int> GroupIds { get; set; }

        [XMessageField(0x1a)]
        public List<string> GroupNames { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.Groups;
    }
}
