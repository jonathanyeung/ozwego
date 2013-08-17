using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ozwego.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Ozwego.BuddyManagement;

namespace Ozwego.Server
{
    /// <summary>
    /// Strategy class for processing an incoming message.  The incoming message class delegates
    /// responsibility of the message processing to these classes.
    /// </summary>
    public abstract class MessageProcessor
    {
        public abstract void DoActions(PacketType packetType, string messageString, string senderEmailAddress);
    }


    class BuddyListMessageProcessor : MessageProcessor
    {
        public override void DoActions(PacketType packetType, string messageString, string senderEmailAddress)
        {
            string[] separators = new string[] { "," };
            string[] buddies = messageString.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var buddyList = new List<Buddy>();

            foreach (string s in buddies)
            {
                var buddy = new Buddy();
                buddy.MicrosoftAccountAddress = s;
                buddyList.Add(buddy);
            }

            switch (packetType)
            {
                case PacketType.UserLoggedIn:
                case PacketType.ServerBuddyList:
                    App.BuddyList.InitializeBuddyList(buddyList);
                    break;

                case PacketType.UserLoggedOut:
                    foreach (Buddy b in buddyList)
                    {
                        App.BuddyList.OnBuddySignOut(b);
                    }
                    break;

                default:
                    throw new ArgumentException(
                        "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }


    class RoomMessageProcessor : MessageProcessor
    {
        public override void DoActions(PacketType packetType, string messageString, string senderEmailAddress)
        {

            switch (packetType)
            {
                case PacketType.ServerRoomList:
                    var separators = new string[] { "," };
                    string[] members = messageString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string member in members)
                    {
                        App.RoomManager.AddMemberToRoom(member);
                    }
                    break;

                case PacketType.UserJoinedRoom:
                    App.RoomManager.AddMemberToRoom(messageString);
                    break;

                case PacketType.UserLeftRoom:
                    App.RoomManager.RemoveMemberFromRoom(messageString);
                    break;

                case PacketType.ServerInitiateGame:
                    var currentFrame = Window.Current.Content as Frame;
                    if (currentFrame != null)
                    {
                        currentFrame.Navigate(typeof(GameBoardPrototype));
                    }
                    break;

                case PacketType.ServerChat:
                    App.RoomManager.ChatMessageReceived(senderEmailAddress, messageString);
                    break;

                case PacketType.HostTransfer:
                    App.RoomManager.ChangeRoomHost(messageString);
                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }

    class GameMessageProcessor : MessageProcessor
    {
        public override void DoActions(PacketType packetType, string messageString, string senderEmailAddress)
        {
            //ToDO: You can wrap this switch statement with if(App.GameController != null).
            switch (packetType)
            {
                case PacketType.ServerGameStart:
                    if (App.GameController != null)
                    {
                        App.GameController.StartGame();
                    }
                    break;

                case PacketType.ServerDump:
                    if (App.GameController != null)
                    {
                        App.GameController.DumpActionReceived(senderEmailAddress);
                    }
                    break;

                case PacketType.ServerPeel:
                    if (App.GameController != null)
                    {
                        App.GameController.PeelActionReceived(senderEmailAddress);
                    }
                    break;

                case PacketType.ServerGameOver:
                    if (App.GameController != null)
                    {
                        App.GameController.EndGame(senderEmailAddress);
                    }
                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }
}
