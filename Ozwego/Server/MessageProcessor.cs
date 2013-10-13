using Ozwego.BuddyManagement;
using Ozwego.Gameplay;
using Ozwego.UI;
using System;
using System.Collections.Generic;
using Shared;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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


    class RoomMessageProcessor : MessageProcessor
    {
        public override void DoActions(PacketType packetType, string messageString, string senderEmailAddress)
        {
            var roomManager = RoomManager.GetInstance();
            switch (packetType)
            {
                case PacketType.ServerRoomList:
                    var separators = new string[] { "," };
                    string[] members = messageString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string member in members)
                    {
                        roomManager.AddMemberToRoom(member);
                    }
                    break;

                case PacketType.UserJoinedRoom:
                    roomManager.AddMemberToRoom(messageString);
                    break;

                case PacketType.UserLeftRoom:
                    roomManager.RemoveMemberFromRoom(messageString);
                    break;

                case PacketType.ServerInitiateGame:
                    var currentFrame = Window.Current.Content as Frame;
                    if (currentFrame != null)
                    {
                        GameBoardNavigationArgs args = new GameBoardNavigationArgs()
                        {
                            GameConnectionType = GameConnectionType.Online,
                            BotCount = 0
                        };

                        currentFrame.Navigate(typeof(GameBoardPrototype), args);
                    }
                    break;

                case PacketType.ServerChat:
                    roomManager.ChatMessageReceived(senderEmailAddress, messageString);
                    break;

                case PacketType.HostTransfer:
                    roomManager.ChangeRoomHost(messageString);
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
            var gameController = GameController.GetInstance();
            switch (packetType)
            {
                case PacketType.ServerGameStart:
                    gameController.StartGame();
                    break;

                case PacketType.ServerDump:
                    gameController.DumpActionReceivedFromServer(senderEmailAddress);
                    break;

                case PacketType.ServerPeel:
                    gameController.PeelActionReceived(senderEmailAddress);
                    break;

                case PacketType.ServerGameOver:
                    gameController.EndGame(senderEmailAddress);
                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }


    class DataBaseMessageProcessor : MessageProcessor
    {
        public delegate void DataBaseMessageEventHandler(object sender, string message);


        public static event DataBaseMessageEventHandler DataBaseMessageReceivedEvent;


        private void OnMessageReceived(string message)
        {
            DataBaseMessageEventHandler handler = DataBaseMessageReceivedEvent;

            if (handler != null)
            {
                handler(this, message);
            }
        }


        public override void DoActions(PacketType packetType, string messageString, string senderEmailAddress)
        {
            var separators = new string[] { "," };
            var friendManager = FriendManager.GetInstance();

            switch (packetType)
            {
                case PacketType.UserLoggedIn:
                    var buddy = CreateBuddyFromUrlString(messageString);
                    friendManager.OnBuddySignIn(buddy);
                    break;


                case PacketType.UserLoggedOut:
                    buddy = CreateBuddyFromUrlString(messageString);
                    friendManager.OnBuddySignOut(buddy);
                    break;

                case PacketType.ServerOnlineFriendList:
                    var buddies = messageString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    var buddyList = new List<Buddy>();
                    foreach (string s in buddies)
                    {
                        var b = CreateBuddyFromUrlString(s);

                        if (b != null)
                        {
                            buddyList.Add(b);
                        }
                    }

                    friendManager.InitializeOnlineBuddyList(buddyList);
                    break;

                // This packet type contains all of the clients friends (both online and offline).
                case PacketType.ServerFriendList:
                    buddies = messageString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    buddyList = new List<Buddy>();
                    foreach (string s in buddies)
                    {
                        var b = CreateBuddyFromUrlString(s);

                        if (b != null)
                        {
                            buddyList.Add(b);
                        }
                    }

                    friendManager.InitializeCompleteBuddyList(buddyList);

                    break;

                case PacketType.ServerFriendRequests:
                    buddies = messageString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string s in buddies)
                    {
                        var b = CreateBuddyFromUrlString(s);

                        if (b != null)
                        {
                            var requestManager = RequestManager.GetInstance();
                            requestManager.NewFriendRequest(b);
                        }
                    }
                    break;

                case PacketType.ServerFriendRequestAccepted:
                    friendManager.AddNewBuddy(CreateBuddyFromUrlString(messageString));
                    break;

                case PacketType.ServerFriendSearchResults:
                    buddies = messageString.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    friendManager.FriendSearchResultsList.Clear();

                    foreach (string s in buddies)
                    {
                        var b = CreateBuddyFromUrlString(s);

                        if (b != null)
                        {
                            friendManager.FriendSearchResultsList.Add(b);
                        }
                    }
                    break;

                case PacketType.ServerUserStats:
                    //
                    // Don't override App.ClientBuddyInstance here, or else INotifyProperty changed
                    // is not going to fire for Buddy and the UI will not update.
                    //

                    var tempBuddy = CreateBuddyFromUrlString(messageString);
                    App.ClientBuddyInstance.Alias = tempBuddy.Alias;
                    App.ClientBuddyInstance.CreationTime = tempBuddy.CreationTime;
                    App.ClientBuddyInstance.Ranking = tempBuddy.Ranking;
                    break;

                case PacketType.ServerIsAliasAvailable:
                    OnMessageReceived(messageString);
                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }


        /// <summary>
        /// Creates a buddy instance based on the information string sent from the server. The 
        /// values in the string sent from the server are delimited by the '|' character, so that
        /// needs to be convered to an '&' char for the wwwFormUrlDecoder to work.
        /// </summary>
        /// <param name="urlString"></param>
        /// <returns></returns>
        private Buddy CreateBuddyFromUrlString(string urlString)
        {
            urlString = urlString.Replace('|', '&');
            var decoder = new WwwFormUrlDecoder(urlString);

            try
            {
                var buddy = new Buddy()
                    {
                        //ToDo: Re-enable here...
                        //Ranking = decoder.GetFirstValueByName("ranking"),
                        CreationTime = decoder.GetFirstValueByName("creationTime"),
                        Alias = decoder.GetFirstValueByName("alias"),
                        EmailAddress = decoder.GetFirstValueByName("email")
                    };

                return buddy;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }


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


        public override void DoActions(PacketType packetType, string messageString, string senderEmailAddress)
        {
            switch (packetType)
            {
                case PacketType.ServerMatchmakingGameFound:
                    OnMessageReceived(MessageResult.GameFound);
                    break;

                case PacketType.ServerMatchmakingGameNotFound:
                    OnMessageReceived(MessageResult.GameNotFound);
                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }
}
