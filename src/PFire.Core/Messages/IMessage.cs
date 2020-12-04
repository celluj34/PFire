using PFire.Core.Enums;

namespace PFire.Core.Messages
{
    internal interface IMessage
    {
        XFireMessageType MessageTypeId { get; }
    }
}
