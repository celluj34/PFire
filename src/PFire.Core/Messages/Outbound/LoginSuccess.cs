using System;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class LoginSuccess : IMessage
    {
        [XMessageField("userid")]
        public int UserId { get; set; }

        [XMessageField("sid")]
        public Guid SessionId { get; set; }

        [XMessageField("nick")]
        public string Nickname { get; set; }

        [XMessageField("status")]
        public int Status { get; set; }

        [XMessageField("dlset")]
        public string DlSet { get; set; }

        [XMessageField("p2pset")]
        public string P2PSet { get; set; }

        [XMessageField("clntset")]
        public string ClientSet { get; set; }

        [XMessageField("minrect")]
        public int MinRect { get; set; }

        [XMessageField("maxrect")]
        public int MaxRect { get; set; }

        [XMessageField("ctry")]
        public int Country { get; set; }

        [XMessageField("n1")]
        public int N1 { get; set; }

        [XMessageField("n2")]
        public int N2 { get; set; }

        [XMessageField("n3")]
        public int N3 { get; set; }

        [XMessageField("pip")]
        public int PublicIp { get; set; }

        [XMessageField("salt")]
        public string Salt { get; set; }

        [XMessageField("reason")]
        public string Reason { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.LoginSuccess;
    }
}
