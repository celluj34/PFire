using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class UserLookupResult : IMessage
    {
        public readonly string QueryByUsername;

        public UserLookupResult(string username)
        {
            QueryByUsername = username;

            Usernames = new List<string>();
            FirstNames = new List<string>();
            LastNames = new List<string>();
            Emails = new List<string>();
        }

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
