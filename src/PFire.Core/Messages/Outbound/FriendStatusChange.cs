using System;
using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class FriendStatusChange : IMessage
    {
        public FriendStatusChange()
        {
            SessionIds = new List<Guid>();
            Messages = new List<string>();
        }

        [XMessageField("sid")]
        public List<Guid> SessionIds { get; set; }

        [XMessageField("msg")]
        public List<string> Messages { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendStatusChange;
    }
}
