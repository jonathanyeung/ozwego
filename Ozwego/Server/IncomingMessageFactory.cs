using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozwego.Server.MessageProcessors;
using Shared;

namespace Ozwego.Server
{
    public class IncomingMessageFactory
    {
        public static IncomingMessage GetMessage(PacketV1 packet)
        {
            var newIncomingMessage = new IncomingMessage()
            {
                PacketType = packet.PacketType,
                Sender = packet.Sender,
                Data = packet.Data,
            };

            CreateMessageProcessor(ref newIncomingMessage);

            return newIncomingMessage;
        }

        /// <summary>
        /// Helper method to create the appropriate message processor 
        /// </summary>
        /// <param name="newIncomingMessage"></param>
        private static void CreateMessageProcessor(ref IncomingMessage newIncomingMessage)
        {
            switch (newIncomingMessage.PacketType)
            {
                case PacketType.s_UserJoinedRoom:
                case PacketType.s_UserLeftRoom:
                case PacketType.s_Chat:
                case PacketType.s_HostTransfer:
                case PacketType.s_RoomList:
                case PacketType.s_BeginGameInitialization:
                    newIncomingMessage.SetMessageProcessor(new RoomMessageProcessor());
                    break;

                case PacketType.s_GameStart:
                case PacketType.s_Dump:
                case PacketType.s_Peel:
                case PacketType.s_GameOver:
                    newIncomingMessage.SetMessageProcessor(new GameMessageProcessor());
                    break;

                case PacketType.s_UserLoggedIn:
                case PacketType.s_UserLoggedOut:
                case PacketType.s_OnlineFriendList:
                case PacketType.s_FriendList:
                case PacketType.s_FriendRequests:
                case PacketType.s_FriendRequestAccepted:
                case PacketType.s_FriendSearchResults:
                case PacketType.s_UserStats:
                case PacketType.s_IsAliasAvailable:
                    newIncomingMessage.SetMessageProcessor(new DataBaseMessageProcessor());
                    break;

                case PacketType.s_MatchmakingGameFound:
                case PacketType.s_MatchmakingGameNotFound:
                    newIncomingMessage.SetMessageProcessor(new MatchmakingMessageProcessor());
                    break;

                default:
                    throw new ArgumentException("Message sent from server not recognized.");
            }
        }

    }
}
