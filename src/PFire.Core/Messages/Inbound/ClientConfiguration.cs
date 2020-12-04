using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class ClientConfiguration : IMessage
    {
        [XMessageField("lang")]
        public string Language { get; set; }

        [XMessageField("skin")]
        public string Skin { get; set; }

        [XMessageField("theme")]
        public string Theme { get; set; }

        [XMessageField("partner")]
        public string Partner { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ClientConfiguration;
    }
}
