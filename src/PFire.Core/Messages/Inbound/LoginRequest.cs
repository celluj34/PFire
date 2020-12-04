using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class LoginRequest : IMessage
    {
        [XMessageField("name")]
        public string Username { get; set; }

        [XMessageField("password")]
        public string Password { get; set; }

        [XMessageField("flags")]
        public int Flags { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.LoginRequest;
    }
}
