using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class KeepAlive : IMessage
    {
        public XFireMessageType MessageTypeId => XFireMessageType.KeepAlive;
    }
}
