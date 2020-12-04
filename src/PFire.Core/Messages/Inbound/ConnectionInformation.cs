using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class ConnectionInformation : IMessage
    {
        [XMessageField("conn")]
        public int Connection { get; set; }

        [XMessageField("nat")]
        public int Nat { get; set; }

        [XMessageField("naterr")]
        public int NatError { get; set; }

        [XMessageField("sec")]
        public int Sec { get; set; }

        [XMessageField("clientip")]
        public int ClientIp { get; set; }

        [XMessageField("upnpinfo")]
        public string UpnpInfo { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ConnectionInformation;
    }
}
