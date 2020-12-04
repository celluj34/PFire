using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using PFire.Core.Enums;
using PFire.Core.Messages;
using PFire.Core.Messages.Inbound;
using PFire.Core.Util;

namespace PFire.Core.Services
{
    internal interface IMessageSerializer
    {
        byte[] Serialize(IMessage message);
        IMessage Deserialize(byte[] bytes);
    }

    internal class MessageSerializer : IMessageSerializer
    {
        private const int MessageSizeLengthInBytes = 2;
        private readonly IXFireAttributeProcessor _attributeProcessor;
        private readonly ILogger<MessageSerializer> _logger;
        private readonly IMessageTypeFactory _messageTypeFactory;

        public MessageSerializer(ILogger<MessageSerializer> logger, IMessageTypeFactory messageTypeFactory, IXFireAttributeProcessor attributeProcessor)
        {
            _logger = logger;
            _messageTypeFactory = messageTypeFactory;
            _attributeProcessor = attributeProcessor;
        }

        public byte[] Serialize(IMessage message)
        {
            var payload = GetPayload(message);
            var payloadShort = (short)(payload.Length + MessageSizeLengthInBytes);
            var payloadLength = BitConverter.GetBytes(payloadShort);

            var finalPayload = ByteHelper.CombineByteArray(payloadLength, payload);

            _logger.LogDebug($"Serialized [{message}]: {BitConverter.ToString(finalPayload)}");

            return finalPayload;
        }

        public IMessage Deserialize(byte[] bytes)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes));
            var messageTypeId = reader.ReadInt16();
            var xMessageType = (XFireMessageType)messageTypeId;

            var messageType = _messageTypeFactory.GetMessageType(xMessageType);
            var message = Activator.CreateInstance(messageType) as IMessage;
            return Deserialize(reader, message);
        }

        private byte[] GetPayload(IMessage message)
        {
            var attributesToBeWritten = message.GetType()
                                               .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                                               .Where(a => Attribute.IsDefined(a, typeof(XMessageField)))
                                               .Select(property => new
                                               {
                                                   messageField = property.GetCustomAttribute<XMessageField>(),
                                                   value = property.GetValue(message)
                                               })
                                               .Where(x => x.messageField != null)
                                               .ToList();

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((short)message.MessageTypeId);
            writer.Write((byte)attributesToBeWritten.Count);
            foreach (var a in attributesToBeWritten)
            {
                _attributeProcessor.WriteValue(writer, a.messageField, a.value);
            }

            return ms.ToArray();
        }

        private IMessage Deserialize(BinaryReader reader, IMessage messageBase)
        {
            var messageType = messageBase.GetType();
            var fieldInfo = messageType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            var attributeCount = reader.ReadByte();

            for (var i = 0; i < attributeCount; i++)
            {
                var attributeName = GetAttributeName(reader, messageType);

                var attributeType = (XFireAttributeType)reader.ReadByte();

                var value = _attributeProcessor.ReadValue(reader, attributeType);

                var field = fieldInfo.Where(a => a.GetCustomAttribute<XMessageField>() != null)
                                     .FirstOrDefault(a => a.GetCustomAttribute<XMessageField>()?.Name == attributeName);

                if (field != null)
                {
                    field.SetValue(messageBase, value);
                }
                else
                {
                    _logger.LogWarning($"WARN: No attribute defined for {attributeName} on class {messageType.Name}");
                }
            }

            _logger.LogDebug($"Deserialized [{messageType}]: {messageBase}");

            return messageBase;
        }

        private string GetAttributeName(BinaryReader reader, Type messageType)
        {
            // TODO: Be brave enough to find an elegant fix for this
            // XFire decides not to follow its own rules. Message type 32 does not have a prefix byte for the length of the attribute name
            // and breaks this code. Assume first byte after the attribute count as the attribute name
            var count = messageType == typeof(StatusChange) ? 1 : reader.ReadByte();

            var readBytes = reader.ReadBytes(count);
            return Encoding.UTF8.GetString(readBytes);
        }
    }
}
