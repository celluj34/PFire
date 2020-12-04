using System;
using PFire.Core.Enums;

namespace PFire.Core.Exceptions
{
    public class UnknownXFireAttributeTypeException : Exception
    {
        public UnknownXFireAttributeTypeException(XFireAttributeType attributeTypeId) : base($"Unknown xfire attribute type {attributeTypeId}") {}
    }
}
