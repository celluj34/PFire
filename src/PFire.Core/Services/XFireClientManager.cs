using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using PFire.Core.Models;

namespace PFire.Core.Services
{
    internal interface IXFireClientManager
    {
        IXFireClient GetSession(Guid sessionId);
        IXFireClient GetSession(UserModel user);
        void AddSession(IXFireClient session);
        void RemoveSession(IXFireClient session);
    }

    internal sealed class XFireClientManager : IXFireClientManager
    {
        private readonly ILogger<XFireClientManager> _logger;
        private readonly ConcurrentDictionary<Guid, IXFireClient> _sessions;

        public XFireClientManager(ILogger<XFireClientManager> logger)
        {
            _logger = logger;
            _sessions = new ConcurrentDictionary<Guid, IXFireClient>();
        }

        public void AddSession(IXFireClient session)
        {
            if (!_sessions.TryAdd(session.SessionId, session))
            {
                _logger.LogWarning($"Tried to add a user with session id {session.SessionId} that already existed.");
            }
        }

        public IXFireClient GetSession(Guid sessionId)
        {
            return _sessions.TryGetValue(sessionId, out var result) ? result : null;
        }

        public IXFireClient GetSession(UserModel user)
        {
            var session = _sessions.ToList().Select(x => x.Value).FirstOrDefault(a => a.User?.Id == user.Id);

            return session == null ? null : GetSession(session.SessionId);
        }

        public void RemoveSession(IXFireClient session)
        {
            if (!_sessions.TryRemove(session.SessionId, out var currentSession))
            {
                return;
            }

            currentSession.Disconnect();
            currentSession.Dispose();
        }
    }
}
