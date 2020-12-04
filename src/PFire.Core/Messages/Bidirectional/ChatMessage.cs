using System;
using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Bidirectional
{
    // the id of this message is the one that the original code base used
    // technically this is a server routed chat message

    internal sealed class ChatMessage : IMessage
    {
        [XMessageField("sid")]
        public Guid SessionId { get; set; }

        [XMessageField("peermsg")]
        public Dictionary<string, dynamic> MessagePayload { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ServerChatMessage;
    }
}
