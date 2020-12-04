using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class ClientVersion : IMessage
    {
        [XMessageField("version")]
        public int Version { get; set; }

        [XMessageField("major_version")]
        public int MajorVersion { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ClientVersion;
    }
}
