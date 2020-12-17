﻿using System;
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
        public Type GetMessageType(XFireMessageType messageType)
        {
            return messageType switch
            {
                XFireMessageType.ChatContent => typeof(ChatContent),
                XFireMessageType.LoginRequest => typeof(LoginRequest),

                // Hack: Client sends message type of 2 for chat messages but expects message type of 133 on receive...
                // This is because the client to client message (type 2) is send via UDP to the clients directly,
                // whereas 133 is a message routed via the server to the client.
                XFireMessageType.UDPChatMessage => typeof(ChatMessage),
                XFireMessageType.ClientVersion => typeof(ClientVersion),
                XFireMessageType.GameInformation => typeof(GameInformation),
                XFireMessageType.FriendRequest => typeof(FriendRequest),
                XFireMessageType.FriendRequestAccept => typeof(FriendRequestAccept),
                XFireMessageType.FriendRequestDecline => typeof(FriendRequestDecline),
                XFireMessageType.UserLookup => typeof(UserLookup),
                XFireMessageType.KeepAlive => typeof(KeepAlive),
                XFireMessageType.NicknameChange => typeof(NicknameChange),
                XFireMessageType.ClientConfiguration => typeof(ClientConfiguration),
                XFireMessageType.ConnectionInformation => typeof(ConnectionInformation),
                XFireMessageType.StatusChange => typeof(StatusChange),
                XFireMessageType.LoginChallenge => typeof(LoginChallenge),
                XFireMessageType.LoginFailure => typeof(LoginFailure),
                XFireMessageType.LoginSuccess => typeof(LoginSuccess),
                XFireMessageType.FriendsList => typeof(FriendsList),
                XFireMessageType.FriendsSessionAssign => typeof(FriendsSessionAssign),
                XFireMessageType.ServerChatMessage => typeof(ChatMessage),
                XFireMessageType.FriendInvite => typeof(FriendInvite),
                XFireMessageType.ClientPreferences => typeof(ClientPreferences),
                XFireMessageType.UserLookupResult => typeof(UserLookupResult),
                XFireMessageType.ServerList => typeof(ServerList),
                XFireMessageType.Groups => typeof(Groups),
                XFireMessageType.GroupsFriends => typeof(GroupsFriends),
                XFireMessageType.FriendStatusChange => typeof(FriendStatusChange),
                XFireMessageType.ChatRooms => typeof(ChatRooms),
                XFireMessageType.Did => typeof(Did),
                _ => throw new UnknownMessageTypeException(messageType)
            };
        }
    }
}