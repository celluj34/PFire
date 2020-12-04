using System;
using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class FriendStatusChange : IMessage
    {
        public FriendStatusChange(Guid sessionId, string message)
        {
            SessionIds = new List<Guid>
            {
                sessionId
            };

            Messages = new List<string>
            {
                message
            };
        }

        [XMessageField("sid")]
        public List<Guid> SessionIds { get; set; }

        [XMessageField("msg")]
        public List<string> Messages { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.FriendStatusChange;
    }
}
