using System;
using PFire.Core.Enums;

namespace PFire.Core.Exceptions
{
    public sealed class UnknownMessageTypeException : Exception
    {
        public UnknownMessageTypeException(XFireMessageType messageType) : base($"Unknown message type: {messageType}") {}
    }
}
