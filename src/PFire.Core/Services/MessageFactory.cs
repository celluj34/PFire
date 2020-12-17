using PFire.Core.Enums;
using PFire.Core.Exceptions;
using PFire.Core.Messages;
using PFire.Core.Messages.Bidirectional;
using PFire.Core.Messages.Inbound;
using PFire.Core.Messages.Outbound;

namespace PFire.Core.Services
{
    internal interface IMessageFactory
    {
        IMessage CreateMessage(XFireMessageType xMessageType);
    }

    internal class MessageFactory : IMessageFactory
    {
        public IMessage CreateMessage(XFireMessageType messageType)
        {
            return messageType switch
            {
                XFireMessageType.ChatContent => new ChatContent(),
                XFireMessageType.LoginRequest => new LoginRequest(),

                // Hack: Client sends message type of 2 for chat messages but expects message type of 133 on receive...
                // This is because the client to client message (type 2) is send via UDP to the clients directly,
                // whereas 133 is a message routed via the server to the client.
                XFireMessageType.UDPChatMessage => new ChatMessage(),
                XFireMessageType.ClientVersion => new ClientVersion(),
                XFireMessageType.GameInformation => new GameInformation(),
                XFireMessageType.FriendRequest => new FriendRequest(),
                XFireMessageType.FriendRequestAccept => new FriendRequestAccept(),
                XFireMessageType.FriendRequestDecline => new FriendRequestDecline(),
                XFireMessageType.UserLookup => new UserLookup(),
                XFireMessageType.KeepAlive => new KeepAlive(),
                XFireMessageType.NicknameChange => new NicknameChange(),
                XFireMessageType.ClientConfiguration => new ClientConfiguration(),
                XFireMessageType.ConnectionInformation => new ConnectionInformation(),
                XFireMessageType.StatusChange => new StatusChange(),
                XFireMessageType.LoginChallenge => new LoginChallenge(),
                XFireMessageType.LoginFailure => new LoginFailure(),
                XFireMessageType.LoginSuccess => new LoginSuccess(),
                XFireMessageType.FriendsList => new FriendsList(),
                XFireMessageType.FriendsSessionAssign => new FriendsSessionAssign(),
                XFireMessageType.ServerChatMessage => new ChatMessage(),
                XFireMessageType.FriendInvite => new FriendInvite(),
                XFireMessageType.ClientPreferences => new ClientPreferences(),
                XFireMessageType.UserLookupResult => new UserLookupResult(),
                XFireMessageType.ServerList => new ServerList(),
                XFireMessageType.Groups => new Groups(),
                XFireMessageType.GroupsFriends => new GroupsFriends(),
                XFireMessageType.FriendStatusChange => new FriendStatusChange(),
                XFireMessageType.ChatRooms => new ChatRooms(),
                XFireMessageType.Did => new Did(),
                _ => throw new UnknownMessageTypeException(messageType)
            };
        }
    }
}
