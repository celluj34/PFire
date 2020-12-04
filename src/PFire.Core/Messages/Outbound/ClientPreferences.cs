using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class ClientPreferences : IMessage
    {
        [XMessageField(0x4c)]
        public Dictionary<byte, string> Preferences { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ClientPreferences;
    }
}
