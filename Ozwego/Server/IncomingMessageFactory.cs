using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozwego.Shared;

namespace Ozwego.Server
{
    public class IncomingMessageFactory
    {
        public static IncomingMessage GetMessage(PacketType packetType, string message, string senderEmailAddress)
        {
            var newIncomingMessage = new IncomingMessage()
            {
                PacketType = packetType,
                MessageString = message,
                SenderEmailAddress = senderEmailAddress
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
                case PacketType.ServerInitiateGame:
                case PacketType.ServerRoomList:
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
                    newIncomingMessage.SetMessageProcessor(new DataBaseMessageProcessor());
                    break;

                default:
                    throw new ArgumentException("Message sent from server not recognized.");
            }
        }

    }
}
