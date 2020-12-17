using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class UserLookupResult : IMessage
    {
        public UserLookupResult()
        {
            Usernames = new List<string>();
            FirstNames = new List<string>();
            LastNames = new List<string>();
            Emails = new List<string>();
        }

        public string QueryByUsername { get; set; }

        [XMessageField("name")]
        public List<string> Usernames { get; set; }

        [XMessageField("fname")]
        public List<string> FirstNames { get; set; }

        [XMessageField("lname")]
        public List<string> LastNames { get; set; }

        [XMessageField("email")]
        public List<string> Emails { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.UserLookupResult;
    }
}
