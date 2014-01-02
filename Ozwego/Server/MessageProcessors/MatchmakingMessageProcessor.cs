using Shared;
using System;

namespace Ozwego.Server.MessageProcessors
{
    class MatchmakingMessageProcessor : MessageProcessor
    {
        public delegate void MatchmakingMessageEventHandler(object sender);

        public static event MatchmakingMessageEventHandler GameFoundEvent;
        public static event MatchmakingMessageEventHandler GameNotFoundEvent;


        private enum MessageResult
        {
            GameFound,
            GameNotFound
        }


        private void OnMessageReceived(MessageResult msgResult)
        {
            MatchmakingMessageEventHandler handler = null;

            switch (msgResult)
            {
                case MessageResult.GameFound:
                    handler = GameFoundEvent;
                    break;

                case MessageResult.GameNotFound:
                    handler = GameNotFoundEvent;
                    break;
            }

            if (handler != null)
            {
                handler(this);
            }
        }


        public override void DoActions(PacketType packetType, object data, string senderEmailAddress)
        {
            switch (packetType)
            {
                case PacketType.s_MatchmakingGameFound:
                    OnMessageReceived(MessageResult.GameFound);
                    break;

                case PacketType.s_MatchmakingGameNotFound:
                    OnMessageReceived(MessageResult.GameNotFound);
                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }
}
