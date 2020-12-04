using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class ChatRooms : IMessage
    {
        public ChatRooms()
        {
            ChatIds = new List<int>();
        }

        [XMessageField(0x04)]
        public List<int> ChatIds { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ChatRooms;
    }
}
