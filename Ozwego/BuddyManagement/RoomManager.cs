using Ozwego.Server;

using Ozwego.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Shared;
using Windows.UI.Core;
using Ozwego.Storage;

namespace Ozwego.BuddyManagement
{
    public class RoomManager
    {
        public Friend Host;
        private object RoomMemberListLock = new object();
        private static RoomManager _instance;

        public ObservableCollection<Friend> RoomMembers;

        /// <summary>
        /// Private Constructor
        /// </summary>
        private RoomManager()
        {
            RoomMembers = new ObservableCollection<Friend>();
            AddMemberToRoom(Settings.userInstance);
        }


        /// <summary>
        /// Public method to instantiate ServerMessageSender singleton.
        /// </summary>
        /// <returns></returns>
        public static RoomManager GetInstance()
        {
            return _instance ?? (_instance = new RoomManager());
        }


        /// <summary>
        /// This method is called when a server notification is received saying that someone has
        /// joined your current room.
        /// </summary>
        public async void AddMemberToRoom(Friend newMember)
        {
            //
            // First check to make sure that the buddy doesn't already exist in the room.  If
            // not, then add the buddy.
            //
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var budref = RoomMembers.FirstOrDefault(
                        b => b.EmailAddress == newMember.EmailAddress);

                    if (budref == null)
                    {
                        RoomMembers.Add(newMember);
                    }
                });
        }


        /// <summary>
        /// This method is called when a server notification is received saying that someone has
        /// left your current room.
        /// </summary>
        /// <param name="buddyAccountAddress">buddy that is leaving the room</param>
        public async void RemoveMemberFromRoom(Friend leavingMember)
        {
            var roomManager = RoomManager.GetInstance();

            Friend budref = roomManager.RoomMembers.FirstOrDefault(
                b => b.EmailAddress == leavingMember.EmailAddress);

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //
                // Don't remove yourself from the room.
                //

                if ((budref != null) &&
                    (leavingMember.EmailAddress != Settings.EmailAddress))
                {
                    roomManager.RoomMembers.Remove(budref);
                }
            });
        }


        public async void JoinRoom(Friend buddyToJoin)
        {
            var serverProxy = ServerProxy.GetInstance();
            await serverProxy.messageSender.SendMessage(
                    PacketType.c_JoinRoom,
                    buddyToJoin);
        }


        //public async void JoinRoom(string accountAddress)
        //{
        //    var serverProxy = ServerProxy.GetInstance();
        //    await serverProxy.messageSender.SendMessage(
        //            PacketType.c_JoinRoom,
        //            accountAddress);
        //}


        public async void LeaveRoom()
        {
            //
            // Send a message to the server that you're leaving.
            //

            var serverProxy = ServerProxy.GetInstance();
            if (null != serverProxy.messageSender)
            {
                await serverProxy.messageSender.SendMessage(PacketType.c_LeaveRoom);
            }


            //
            // Add the client (now with a new room GUID) back to the room list.
            //

            var roomManager = RoomManager.GetInstance();
            roomManager.RoomMembers.Clear();
            AddMemberToRoom(Settings.userInstance);
        }


        public async void ChangeRoomHost(Friend newHost)
        {
            Host = newHost;

            var mainPageViewModel = MainPageViewModel.GetInstance();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mainPageViewModel.RoomHost = newHost.Alias;
            });
            
            if (newHost.EmailAddress != Settings.EmailAddress)
            {
                //ToDO: Disable Start Game with Room Button.
            }
        }


        public async void InitiateMessageSend(string message)
        {
            var serverProxy = ServerProxy.GetInstance();

            var chat = new ChatMessage {Message = message, Sender = Settings.Alias};

            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.c_Chat, chat);
            }
        }


        public async void ChatMessageReceived(ChatMessage message)
        {
            string formattedMessage = String.Format("{0}: {1}", message.Sender, message.Message);

            var mainPageViewModel = MainPageViewModel.GetInstance();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mainPageViewModel.ChatMessages.Add(formattedMessage);
            });
        }
    }
}
