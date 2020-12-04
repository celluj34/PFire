using System;
using System.Collections.Generic;
using PFire.Core.Enums;
using PFire.Core.Exceptions;
using PFire.Core.Messages.Bidirectional;
using PFire.Core.Messages.Inbound;
using PFire.Core.Messages.Outbound;

namespace PFire.Core.Services
{
    internal interface IMessageTypeFactory
    {
        Type GetMessageType(XFireMessageType xMessageType);
    }

    internal class MessageTypeFactory : IMessageTypeFactory
    {
        private static readonly Dictionary<XFireMessageType, Type> Messages = new Dictionary<XFireMessageType, Type>
        {
            {
                XFireMessageType.ClientVersion, typeof(ClientVersion)
            },
            {
                XFireMessageType.LoginRequest, typeof(LoginRequest)
            },
            {
                XFireMessageType.LoginFailure, typeof(LoginFailure)
            },
            {
                XFireMessageType.LoginSuccess, typeof(LoginSuccess)
            },
            {
                XFireMessageType.ClientConfiguration, typeof(ClientConfiguration)
            },
            {
                XFireMessageType.ConnectionInformation, typeof(ConnectionInformation)
            },
            {
                XFireMessageType.Groups, typeof(Groups)
            },
            {
                XFireMessageType.GroupsFriends, typeof(GroupsFriends)
            },
            {
                XFireMessageType.ServerList, typeof(ServerList)
            },
            {
                XFireMessageType.ChatRooms, typeof(ChatRooms)
            },
            {
                XFireMessageType.GameInformation, typeof(GameInformation)
            },
            {
                XFireMessageType.KeepAlive, typeof(KeepAlive)
            },
            {
                XFireMessageType.Did, typeof(Did)
            },
            {
                XFireMessageType.ServerChatMessage, typeof(ChatMessage)
            },
            {
                XFireMessageType.UserLookup, typeof(UserLookup)
            },
            {
                XFireMessageType.FriendRequest, typeof(FriendRequest)
            },
            {
                XFireMessageType.FriendRequestAccept, typeof(FriendRequestAccept)
            },
            {
                XFireMessageType.FriendRequestDecline, typeof(FriendRequestDecline)
            },
            {
                XFireMessageType.NicknameChange, typeof(NicknameChange)
            },
            {
                XFireMessageType.StatusChange, typeof(StatusChange)
            }
        };

        public Type GetMessageType(XFireMessageType messageType)
        {
            // Hack: Client sends message type of 2 for chat messages but expects message type of 133 on receive...
            // this is because the client to client message (type 2) is send via UDP to the clients directly,
            // whereas 133 is a message routed via the server to the client
            if (messageType == XFireMessageType.UDPChatMessage)
            {
                messageType = XFireMessageType.ServerChatMessage;
            }

            if (Messages.TryGetValue(messageType, out var message))
            {
                return message;
            }

            throw new UnknownMessageTypeException(messageType);
        }
    }
}
