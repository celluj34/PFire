using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class NicknameChange : IMessage
    {
        public const int MAX_LENGTH = 35;

        [XMessageField("nick")]
        public string Nickname { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.NicknameChange;
    }
}
