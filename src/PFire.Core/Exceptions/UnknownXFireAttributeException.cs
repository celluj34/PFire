using System;
using PFire.Core.Enums;

namespace PFire.Core.Exceptions
{
    internal class UnknownXFireAttributeTypeException : Exception
    {
        public UnknownXFireAttributeTypeException(XFireAttributeType attributeTypeId) : base($"Unknown xfire attribute type {attributeTypeId}") {}
    }
}
