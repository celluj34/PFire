using PFire.Core.Enums;

namespace PFire.Core.Messages.Inbound
{
    internal sealed class GameInformation : IMessage
    {
        [XMessageField("gameid")]
        public int GameId { get; set; }

        [XMessageField("gip")]
        public int GameIP { get; set; }

        [XMessageField("gport")]
        public int GamePort { get; set; }

        public XFireMessageType MessageTypeId => XFireMessageType.GameInformation;
    }
}
