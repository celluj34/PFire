using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PFire.Core.Enums;
using PFire.Core.Messages;
using PFire.Core.Messages.Bidirectional;
using PFire.Core.Messages.Inbound;
using PFire.Core.Messages.Outbound;

namespace PFire.Core.Services
{
    internal interface IXFireMessageProcessor
    {
        Task Process(IMessage message, IXFireClient xFireClient, IXFireClientManager xFireClientManager);
    }

    internal class XFireMessageProcessor : IXFireMessageProcessor
    {
        private readonly IPFireDatabase _database;
        private readonly ILogger<XFireMessageProcessor> _logger;

        public XFireMessageProcessor(ILogger<XFireMessageProcessor> logger, IPFireDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task Process(IMessage message, IXFireClient xFireClient, IXFireClientManager xFireClientManager)
        {
            switch (message)
            {
                case ChatMessage chatMessage:
                {
                    // TODO: Create test for this message so we can refactor and build this message the same way as the others to avoid the switch statement
                    // TODO: How to tell the client we didn't receive the ACK?
                    // TODO: P2P stuff???
                    var otherSession = xFireClientManager.GetSession(chatMessage.SessionId);
                    if (otherSession == null)
                    {
                        return;
                    }

                    var messageType = (ChatMessageType)(byte)chatMessage.MessagePayload["msgtype"];

                    switch (messageType)
                    {
                        case ChatMessageType.Content:
                            var responseAck = BuildAckResponse(otherSession.SessionId);
                            await xFireClient.SendMessage(responseAck);

                            var chatMsg = BuildChatMessageResponse(xFireClient.SessionId);
                            await otherSession.SendMessage(chatMsg);
                            break;

                        case ChatMessageType.TypingNotification:
                            var typingMsg = BuildChatMessageResponse(xFireClient.SessionId);
                            await otherSession.SendMessage(typingMsg);
                            break;

                        default:
                            _logger.LogDebug($"NOT BUILT: Got {messageType} for session: {xFireClient.SessionId}");
                            break;
                    }

                    ChatMessage BuildChatMessageResponse(Guid sessionId)
                    {
                        return new()
                        {
                            SessionId = sessionId,
                            MessagePayload = new Dictionary<string, dynamic>(chatMessage.MessagePayload)
                        };
                    }

                    ChatMessage BuildAckResponse(Guid sessionId)
                    {
                        var ack = new ChatMessage
                        {
                            SessionId = sessionId,
                            MessagePayload = new Dictionary<string, dynamic>()
                        };

                        ack.MessagePayload.Add("imindex", (int)chatMessage.MessagePayload["imindex"]);
                        return ack;
                    }

                    break;
                }
                case ClientConfiguration clientConfiguration:
                {
                    await xFireClient.SendAndProcessMessage(new Did());

                    break;
                }
                case ClientPreferences clientPreferences:
                {
                    clientPreferences.Preferences = new Dictionary<byte, string>
                    {
                        {
                            1, "0"
                        },
                        {
                            4, "0"
                        },
                        {
                            5, "0"
                        },
                        {
                            6, "1"
                        },
                        {
                            7, "0"
                        },
                        {
                            8, "0"
                        },
                        {
                            11, "0"
                        },
                        {
                            17, "0"
                        },
                        {
                            18, "0"
                        },
                        {
                            19, "0"
                        },
                        {
                            20, "0"
                        },
                        {
                            21, "0"
                        }
                    };

                    break;
                }
                case ClientVersion clientVersion:
                {
                    var loginChallenge = new LoginChallenge();
                    await Process(loginChallenge, xFireClient, xFireClientManager);
                    await xFireClient.SendMessage(loginChallenge);

                    break;
                }
                case ConnectionInformation connectionInformation:
                {
                    var clientPrefs = new ClientPreferences();
                    await xFireClient.SendAndProcessMessage(clientPrefs);

                    var groups = new Groups();
                    await xFireClient.SendAndProcessMessage(groups);

                    var groupsFriends = new GroupsFriends();
                    await xFireClient.SendAndProcessMessage(groupsFriends);

                    var serverList = new ServerList();
                    await xFireClient.SendAndProcessMessage(serverList);

                    var chatRooms = new ChatRooms();
                    await xFireClient.SendAndProcessMessage(chatRooms);

                    var friendsList = new FriendsList
                    {
                        Owner = xFireClient.User
                    };

                    await xFireClient.SendAndProcessMessage(friendsList);

                    var friendsStatus = new FriendsSessionAssign
                    {
                        Owner = xFireClient.User
                    };

                    await xFireClient.SendAndProcessMessage(friendsStatus);

                    // Tell friends this user came online
                    //if (xFireClient.User.Username == "test") Debugger.Break();
                    var friends = await _database.QueryFriends(xFireClient.User);
                    foreach (var friend in friends)
                    {
                        var otherSession = xFireClientManager.GetSession(friend);
                        if (otherSession != null)
                        {
                            await otherSession.SendAndProcessMessage(new FriendsSessionAssign
                            {
                                Owner = friend
                            });
                        }
                    }

                    var pendingFriendRequests = await _database.QueryPendingFriendRequests(xFireClient.User);
                    foreach (var request in pendingFriendRequests.Select(request => new FriendInvite
                    {
                        Usernames =
                        {
                            request.Username
                        },
                        Nicknames =
                        {
                            request.Nickname
                        },
                        Messages =
                        {
                            request.Message
                        }
                    }))
                    {
                        await xFireClient.SendAndProcessMessage(request);
                    }

                    break;
                }
                case FriendsSessionAssign friendsSessionAssign:
                {
                    var friends = await _database.QueryFriends(friendsSessionAssign.Owner);
                    foreach (var friend in friends)
                    {
                        var friendSession = xFireClientManager.GetSession(friend);

                        friendsSessionAssign.UserIds.Add(friend.Id);
                        friendsSessionAssign.SessionIds.Add(friendSession?.SessionId ?? FriendsSessionAssign.FriendIsOffLineSessionId);
                    }

                    break;
                }
                case LoginChallenge loginChallenge:
                {
                    loginChallenge.Salt = xFireClient.Salt;

                    break;
                }
                case LoginRequest loginRequest:
                {
                    var user = await _database.QueryUser(loginRequest.Username);
                    if (user != null)
                    {
                        if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                        {
                            await xFireClient.SendAndProcessMessage(new LoginFailure());

                            return;
                        }
                    }
                    else
                    {
                        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(loginRequest.Password);
                        user = await _database.InsertUser(loginRequest.Username, hashedPassword, xFireClient.Salt);
                    }

                    // Remove any older sessions from this user (duplicate logins)
                    xFireClient.RemoveOtherSessions(user);

                    xFireClient.User = user;

                    var success = new LoginSuccess();
                    await xFireClient.SendAndProcessMessage(success);

                    break;
                }
                case LoginSuccess loginSuccess:
                {
                    loginSuccess.UserId = xFireClient.User.Id;
                    loginSuccess.SessionId = xFireClient.SessionId;
                    loginSuccess.Status = 0;
                    loginSuccess.Nickname = string.IsNullOrEmpty(xFireClient.User.Nickname) ? xFireClient.User.Username : xFireClient.User.Nickname;
                    loginSuccess.MinRect = 1;
                    loginSuccess.MaxRect = 164867;
                    loginSuccess.PublicIp = xFireClient.PublicIp;
                    loginSuccess.Salt = xFireClient.Salt;
                    loginSuccess.Reason = "Mq_P8Ad3aMEUvFinw0ceu6FITnZTWXxg46XU8xHW";

                    _logger.LogDebug($"User {xFireClient.User.Username}[{xFireClient.User.Id}] logged in successfully with session id {xFireClient.SessionId}");

                    _logger.LogInformation($"User {xFireClient.User.Username} logged in");

                    break;
                }
                case Groups groups:
                {
                    groups.GroupIds = new List<int>();
                    groups.GroupNames = new List<string>();

                    break;
                }
                case FriendRequest friendRequest:
                {
                    var recipient = await _database.QueryUser(friendRequest.Username);
                    var invite = new FriendInvite
                    {
                        Usernames =
                        {
                            xFireClient.User.Username
                        },
                        Nicknames =
                        {
                            xFireClient.User.Nickname
                        },
                        Messages =
                        {
                            friendRequest.Message
                        }
                    };

                    await Process(invite, xFireClient, xFireClientManager);

                    await _database.InsertFriendRequest(xFireClient.User, recipient, friendRequest.Message);

                    var recipientSession = xFireClientManager.GetSession(recipient);
                    if (recipientSession != null)
                    {
                        await recipientSession.SendMessage(invite);
                    }

                    break;
                }
                case FriendRequestAccept friendRequestAccept:
                {
                    var friend = await _database.QueryUser(friendRequestAccept.FriendUsername);

                    await _database.InsertMutualFriend(xFireClient.User, friend);

                    await xFireClient.SendAndProcessMessage(new FriendsList
                    {
                        Owner = xFireClient.User
                    });

                    await xFireClient.SendAndProcessMessage(new FriendsSessionAssign
                    {
                        Owner = xFireClient.User
                    });

                    // It's possible to accept a friend request where the inviter is not online
                    var friendSession = xFireClientManager.GetSession(friend);
                    if (friendSession != null)
                    {
                        await friendSession.SendAndProcessMessage(new FriendsList
                        {
                            Owner = friend
                        });

                        await friendSession.SendAndProcessMessage(new FriendsSessionAssign
                        {
                            Owner = friend
                        });
                    }

                    var pendingRequests = await _database.QueryPendingFriendRequests(xFireClient.User);
                    var pq = pendingRequests.Where(a => a.Id == friend.Id).ToArray();
                    await _database.DeletePendingFriendRequest(xFireClient.User, pq);

                    break;
                }
                case FriendRequestDecline friendRequestDecline:
                {
                    var requesterUser = await _database.QueryUser(friendRequestDecline.RequesterUsername);
                    var pendingRequests = await _database.QueryPendingFriendRequestsSelf(requesterUser);

                    var requestsIds = pendingRequests.Where(a => a.Id == requesterUser.Id).ToArray();

                    await _database.DeletePendingFriendRequest(xFireClient.User, requestsIds);

                    break;
                }
                case FriendsList friendsList:
                {
                    var friends = await _database.QueryFriends(friendsList.Owner);
                    foreach (var f in friends)
                    {
                        friendsList.UserIds.Add(f.Id);
                        friendsList.Usernames.Add(f.Username);
                        friendsList.Nicks.Add(f.Nickname);
                    }

                    break;
                }
                case NicknameChange nicknameChange:
                {
                    if (nicknameChange.Nickname.Length > NicknameChange.MAX_LENGTH)
                    {
                        nicknameChange.Nickname = nicknameChange.Nickname.Substring(0, NicknameChange.MAX_LENGTH);
                    }

                    await _database.UpdateNickname(xFireClient.User, nicknameChange.Nickname);

                    var updatedFriendsList = new FriendsList
                    {
                        Owner = xFireClient.User
                    };

                    var queryFriends = await _database.QueryFriends(xFireClient.User);
                    foreach (var friend in queryFriends)
                    {
                        var friendSession = xFireClientManager.GetSession(friend);
                        if (friendSession != null)
                        {
                            await friendSession.SendAndProcessMessage(updatedFriendsList);
                        }
                    }

                    break;
                }
                case StatusChange statusChange:
                {
                    var friendStatusChange = new FriendStatusChange
                    {
                        SessionIds =
                        {
                            xFireClient.SessionId
                        },
                        Messages =
                        {
                            statusChange.Message
                        }
                    };

                    var friends = await _database.QueryFriends(xFireClient.User);
                    foreach (var friend in friends)
                    {
                        var friendSession = xFireClientManager.GetSession(friend);
                        if (friendSession != null)
                        {
                            await friendSession.SendAndProcessMessage(friendStatusChange);
                        }
                    }

                    break;
                }
                case UserLookup userLookup:
                {
                    var result = new UserLookupResult
                    {
                        QueryByUsername = userLookup.Username
                    };

                    await xFireClient.SendAndProcessMessage(result);

                    break;
                }
                case UserLookupResult userLookupResult:
                {
                    var queryUsers = await _database.QueryUsers(userLookupResult.QueryByUsername);
                    var usernames = queryUsers.Select(a => a.Username).ToList();

                    userLookupResult.Usernames.AddRange(usernames);

                    // Don't really care about these but they're necessary to work properly
                    var unknowns = usernames.Select(a => "Unknown").ToList();

                    userLookupResult.FirstNames.AddRange(unknowns);
                    userLookupResult.LastNames.AddRange(unknowns);
                    userLookupResult.Emails.AddRange(unknowns);

                    break;
                }
                case null:
                {
                    // base implementation is to do nothing
                    _logger.LogError(" *** Could not process null message");

                    break;
                }
                default:
                    // base implementation is to do nothing
                    _logger.LogWarning($" *** Unimplemented processing for message type {message.MessageTypeId}");

                    break;
            }
        }
    }
}
