using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class LoginFailure : IMessage
    {
        [XMessageField("reason")]
        public int Reason { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.LoginFailure;
    }
}
