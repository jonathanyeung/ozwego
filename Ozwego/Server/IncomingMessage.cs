using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Windows.Foundation;

namespace Ozwego.Server
{
    public class IncomingMessage : Message
    {
        /// <summary>
        /// Delegates message processing to ProcessMessage strategy.
        /// </summary>
        private MessageProcessor ProcessMessage { get; set; }


        /// <summary>
        /// Setter for the MessageProcessor delegate.
        /// </summary>
        /// <param name="messageProcessor"></param>
        public void SetMessageProcessor(MessageProcessor messageProcessor)
        {
            ProcessMessage = messageProcessor;
        }


        /// <summary>
        /// Public method that is called to perform all of the necessary actions required on a 
        /// message received.
        /// </summary>
        public void HandleMessage()
        {
            ProcessMessage.DoActions(PacketType, MessageString, SenderEmailAddress);
        }
    }


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
                // ToDo: Add cases for the type of message received.
                case PacketType.UserLoggedIn:
                case PacketType.UserLoggedOut:
                case PacketType.ServerBuddyList:
                    newIncomingMessage.SetMessageProcessor(new BuddyListMessageProcessor());
                    break;

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

                default:
                    throw new ArgumentException("Message sent from server not recognized.");
            }
        }

    }
}
