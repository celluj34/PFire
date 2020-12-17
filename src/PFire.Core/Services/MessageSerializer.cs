using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using PFire.Core.Enums;
using PFire.Core.Exceptions;
using PFire.Core.Messages;
using PFire.Core.Messages.Bidirectional;
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
        private readonly ILogger<MessageSerializer> _logger;
        private readonly IMessageFactory _messageFactory;

        public MessageSerializer(ILogger<MessageSerializer> logger, IMessageFactory messageFactory)
        {
            _logger = logger;
            _messageFactory = messageFactory;
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
            using var memoryStream = new MemoryStream(bytes);
            using var reader = new BinaryReader(memoryStream);

            var messageType = (XFireMessageType)reader.ReadInt16();

            var message = _messageFactory.CreateMessage(messageType);
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

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            writer.Write((short)message.MessageTypeId);
            writer.Write((byte)attributesToBeWritten.Count);
            foreach (var a in attributesToBeWritten)
            {
                WriteAttribute(writer, a.messageField, a.value);
            }

            return memoryStream.ToArray();
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

                var value = ReadAttribute(reader, attributeType);

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

        private void WriteAttribute(BinaryWriter writer, XMessageField messageField, object value)
        {
            if (messageField.NonTextualName)
            {
                writer.Write(messageField.NameAsBytes);
            }
            else
            {
                if (messageField.Name != null)
                {
                    writer.Write((byte)messageField.Name.Length);
                    writer.Write(Encoding.UTF8.GetBytes(messageField.Name));
                }
            }

            WriteValue(writer, value);
        }

        private void WriteValue(BinaryWriter writer, object value)
        {
            switch (value)
            {
                case string stringValue:
                {
                    writer.Write((byte)XFireAttributeType.String);
                    writer.Write((short)stringValue.Length);
                    writer.Write(Encoding.UTF8.GetBytes(stringValue));

                    break;
                }
                case int intValue:
                {
                    writer.Write((byte)XFireAttributeType.Int32);
                    writer.Write(intValue);

                    break;
                }
                case Guid guidValue:
                {
                    writer.Write((byte)XFireAttributeType.SessionId);
                    writer.Write(guidValue.ToByteArray());

                    break;
                }
                case List<object> listValue:
                {
                    writer.Write((byte)XFireAttributeType.List);
                    writer.Write((short)listValue.Count);

                    foreach (var o in listValue)
                    {
                        WriteValue(writer, o);
                    }

                    break;
                }

                case Dictionary<string, object> dictValue:
                {
                    writer.Write((byte)XFireAttributeType.StringKeyMap);
                    WriteTypedDictionary(writer, dictValue);

                    break;
                }
                case XFireAttributeType.DID:
                {
                    writer.Write((byte)XFireAttributeType.DID);
                    writer.Write((byte[])value);

                    break;
                }
                case byte byteValue:
                {
                    writer.Write((byte)XFireAttributeType.Int8);
                    writer.Write(byteValue);

                    break;
                }
                case Dictionary<byte, object> dictValue:
                {
                    writer.Write((byte)XFireAttributeType.Int8KeyMap);
                    WriteTypedDictionary(writer, dictValue);

                    break;
                }
                case XFireAttributeType.Message:
                {
                    writer.Write((byte)XFireAttributeType.Message);

                    //TODO

                    break;
                }
            }
        }

        private dynamic ReadAttribute(BinaryReader reader, XFireAttributeType attributeType)
        {
            switch (attributeType)
            {
                case XFireAttributeType.List:
                {
                    var listItemType = (XFireAttributeType)reader.ReadByte();
                    var listLength = reader.ReadInt16();

                    var values = new List<dynamic>();
                    for (var i = 0; i < listLength; i++)
                    {
                        values.Add(ReadAttribute(reader, listItemType));
                    }

                    return values;
                }
                case XFireAttributeType.Message:
                {
                    var values = new Dictionary<string, IMessage>();
                    var mapLength = reader.ReadByte();

                    for (var i = 0; i < mapLength; i++)
                    {
                        var readValue = ReadInt8AsString(reader);
                        var messageTypeName = readValue;
                        var xFireAttributeType = (XFireAttributeType)reader.ReadByte();
                        var messageType = ReadAttribute(reader, xFireAttributeType);
                        var rawMessage = (IMessage)((short)messageType switch
                        {
                            0 => new ChatMessage(),
                            1 => new ChatAcknowledgement(),
                            _ => throw new UnknownMessageTypeException((XFireMessageType)(short)messageType)
                        });

                        var message = Deserialize(reader, rawMessage);
                        values.Add(messageTypeName, message);
                    }

                    return values;
                }

                case XFireAttributeType.String:
                {
                    var valueLength = reader.ReadInt16();
                    var bytes = reader.ReadBytes(valueLength);

                    return Encoding.UTF8.GetString(bytes);
                }
                case XFireAttributeType.Int32:
                {
                    return reader.ReadInt32();
                }
                case XFireAttributeType.SessionId:
                {
                    return new Guid(reader.ReadBytes(16));
                }
                case XFireAttributeType.StringKeyMap:
                {
                    return ReadTypedDictionary<string>(reader);
                }
                case XFireAttributeType.DID:
                {
                    return reader.ReadBytes(21);
                }
                case XFireAttributeType.Int8:
                {
                    return reader.ReadByte();
                }
                case XFireAttributeType.Int8KeyMap:
                {
                    return ReadTypedDictionary<byte>(reader);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(attributeType), attributeType, null);
                }
            }
        }

        private void WriteTypedDictionary<T>(BinaryWriter writer, Dictionary<T, dynamic> data)
        {
            var mapLength = (byte)data.Count;

            writer.Write(mapLength);

            foreach (var (key, value) in data)
            {
                // TODO: Fix hack
                // Stupid protocol decides to not be nice and expect an 8bit string length prefix instead of 16 for string key mapped types
                if (typeof(T) == typeof(string))
                {
                    WriteStringLengthAsInt8(writer, key as string);
                }
                else
                {
                    WriteValue(writer, value);
                }

                WriteValue(writer, value);
            }
        }

        private Dictionary<T, dynamic> ReadTypedDictionary<T>(BinaryReader reader)
        {
            var values = new Dictionary<T, dynamic>();
            var mapLength = reader.ReadByte();

            for (var i = 0; i < mapLength; i++)
            {
                // TODO: Fix hack
                // Stupid protocol decides to not be nice and expect an 8bit string length prefix instead of the normal 16 for string key mapped types
                var key = typeof(T) == typeof(string) ? ReadInt8AsString(reader) : ReadAttribute(reader, XFireAttributeType.Int8);

                var type = (XFireAttributeType)reader.ReadByte();
                var readValue = ReadAttribute(reader, type);
                values.Add(key, readValue);
            }

            return values;
        }

        private void WriteStringLengthAsInt8(BinaryWriter writer, string key)
        {
            writer.Write((byte)key.Length);
            writer.Write(Encoding.UTF8.GetBytes(key));
        }

        private string ReadInt8AsString(BinaryReader reader)
        {
            var length = reader.ReadByte();
            return Encoding.UTF8.GetString(reader.ReadBytes(length));
        }
    }
}
