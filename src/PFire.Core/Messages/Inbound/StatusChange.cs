using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class StatusChange : IMessage
    {
        [XMessageField(0x2e)]
        public string Message { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.StatusChange;
    }
}
