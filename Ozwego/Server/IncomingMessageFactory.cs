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
                case PacketType.UserJoinedRoom:
                case PacketType.UserLeftRoom:
                case PacketType.ServerChat:
                case PacketType.HostTransfer:
                case PacketType.ServerRoomList:
                case PacketType.ServerBeginGameInitialization:
                    newIncomingMessage.SetMessageProcessor(new RoomMessageProcessor());
                    break;

                case PacketType.ServerGameStart:
                case PacketType.ServerDump:
                case PacketType.ServerPeel:
                case PacketType.ServerGameOver:
                    newIncomingMessage.SetMessageProcessor(new GameMessageProcessor());
                    break;

                case PacketType.UserLoggedIn:
                case PacketType.UserLoggedOut:
                case PacketType.ServerOnlineFriendList:
                case PacketType.ServerFriendList:
                case PacketType.ServerFriendRequests:
                case PacketType.ServerFriendRequestAccepted:
                case PacketType.ServerFriendSearchResults:
                case PacketType.ServerUserStats:
                case PacketType.ServerIsAliasAvailable:
                    newIncomingMessage.SetMessageProcessor(new DataBaseMessageProcessor());
                    break;

                case PacketType.ServerMatchmakingGameFound:
                case PacketType.ServerMatchmakingGameNotFound:
                    newIncomingMessage.SetMessageProcessor(new MatchmakingMessageProcessor());
                    break;

                default:
                    throw new ArgumentException("Message sent from server not recognized.");
            }
        }

    }
}
