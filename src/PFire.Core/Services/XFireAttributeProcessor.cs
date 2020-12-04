using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PFire.Core.Enums;
using PFire.Core.Exceptions;
using PFire.Core.Messages;
using PFire.Core.Messages.Bidirectional;

namespace PFire.Core.Services
{
    internal interface IXFireAttributeProcessor
    {
        void WriteValue(BinaryWriter writer, XMessageField messageField, object value);
        dynamic ReadValue(BinaryReader reader, XFireAttributeType attributeType);
    }

    internal class XFireAttributeProcessor : IXFireAttributeProcessor
    {
        public void WriteValue(BinaryWriter writer, XMessageField messageField, object value)
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

        public dynamic ReadValue(BinaryReader reader, XFireAttributeType attributeType)
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
                        values.Add(ReadValue(reader, listItemType));
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
                        var messageType = ReadValue(reader, xFireAttributeType);
                        var o = CreateMessage(messageType);
                        var message = (IMessage)null; //TODO: deserialize sub-message
                        //StaticMessageSerializer.Deserialize(reader, o); 
                        values.Add(messageTypeName, message);
                    }

                    return values;

                    IMessage CreateMessage(short type)
                    {
                        var MESSAGE_TYPES = new Dictionary<int, Type>
                        {
                            {
                                0, typeof(ChatMessage)
                            },
                            {
                                1, typeof(ChatAcknowledgement)
                            }
                        };

                        if (MESSAGE_TYPES.ContainsKey(type))
                        {
                            return (IMessage)Activator.CreateInstance(MESSAGE_TYPES[type]);
                        }

                        throw new UnknownMessageTypeException((XFireMessageType)type);
                    }
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
                var key = typeof(T) == typeof(string) ? ReadInt8AsString(reader) : ReadValue(reader, XFireAttributeType.Int8);

                var type = (XFireAttributeType)reader.ReadByte();
                var readValue = ReadValue(reader, type);
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
