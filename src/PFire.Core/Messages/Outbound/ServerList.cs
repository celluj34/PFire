using System.Collections.Generic;
using PFire.Core.Enums;

namespace PFire.Core.Messages.Outbound
{
    internal sealed class ServerList : IMessage
    {
        public ServerList()
        {
            GameIds = new List<int>();
            GameIPs = new List<int>();
            GamePorts = new List<int>();
        }

        [XMessageField("max")]
        public int MaximumFavorites { get; set; }

        [XMessageField("gameid")]
        public List<int> GameIds { get; set; }

        [XMessageField("gip")]
        public List<int> GameIPs { get; set; }

        [XMessageField("gport")]
        public List<int> GamePorts { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.ServerList;
    }
}
