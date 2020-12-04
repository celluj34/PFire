using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class LoginChallenge : IMessage
    {
        [XMessageField("salt")]
        public string Salt { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.LoginChallenge;
    }
}
