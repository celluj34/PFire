using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class UserLookup : IMessage
    {
        [XMessageField("name")]
        public string Username { get; set; }

        [XMessageField("fname")]
        public string FirstName { get; set; }

        [XMessageField("lname")]
        public string LastName { get; set; }

        [XMessageField("email")]
        public string Email { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.UserLookup;
    }
}
