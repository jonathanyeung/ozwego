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

        private static RoomManager _instance;

        public ObservableCollection<Buddy> RoomMembers;

        /// <summary>
        /// Private Constructor
        /// </summary>
        private RoomManager()
        {
            RoomMembers = new ObservableCollection<Buddy>();
            AddMemberToRoom(Settings.EmailAddress);
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
        /// <param name="buddyAccountAddress">buddy that is joining the room</param>
        public async void AddMemberToRoom(string buddyAccountAddress)
        {
            //
            // First check to make sure that the buddy doesn't already exist in the room.  If
            // not, then add the buddy.
            //

            Buddy budref = RoomMembers.FirstOrDefault(
                    b => b.EmailAddress == buddyAccountAddress);

            if (budref == null)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    RoomMembers.Add(new Buddy(buddyAccountAddress));
                });
            }
        }


        /// <summary>
        /// This method is called when a server notification is received saying that someone has
        /// left your current room.
        /// </summary>
        /// <param name="buddyAccountAddress">buddy that is leaving the room</param>
        public async void RemoveMemberFromRoom(string buddyAccountAddress)
        {
            var roomManager = RoomManager.GetInstance();

            Buddy budref = roomManager.RoomMembers.FirstOrDefault(
                b => b.EmailAddress == buddyAccountAddress);

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //
                // Don't remove yourself from the room.
                //

                if ((budref != null) &&
                    (buddyAccountAddress != Settings.EmailAddress))
                {
                    roomManager.RoomMembers.Remove(budref);
                }
            });
        }


        public async void JoinRoom(Buddy buddyToJoin)
        {
            var serverProxy = ServerProxy.GetInstance();
            await serverProxy.messageSender.SendMessage(
                    PacketType.ClientJoinRoom,
                    buddyToJoin.EmailAddress);
        }


        public async void JoinRoom(string accountAddress)
        {
            var serverProxy = ServerProxy.GetInstance();
            await serverProxy.messageSender.SendMessage(
                    PacketType.ClientJoinRoom,
                    accountAddress);
        }


        public async void LeaveRoom()
        {
            //
            // Send a message to the server that you're leaving.
            //

            var serverProxy = ServerProxy.GetInstance();
            if (null != serverProxy.messageSender)
            {
                await serverProxy.messageSender.SendMessage(PacketType.ClientLeaveRoom);
            }


            //
            // Add the client (now with a new room GUID) back to the room list.
            //

            var roomManager = RoomManager.GetInstance();
            roomManager.RoomMembers.Clear();
            AddMemberToRoom(Settings.EmailAddress);
        }


        public async void ChangeRoomHost(string newHostName)
        {
            var mainPageViewModel = MainPageViewModel.GetInstance();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mainPageViewModel.RoomHost = newHostName;
            });
            
            if (newHostName != Settings.EmailAddress)
            {
                //ToDO: Disable Start Game with Room Button.
            }
        }


        public async void InitiateMessageSend(string message)
        {
            var serverProxy = ServerProxy.GetInstance();
            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.ClientChat, message);
            }
        }


        public async void ChatMessageReceived(string senderName, string message)
        {
            string formattedMessage = String.Format("{0}: {1}", senderName, message);

            var mainPageViewModel = MainPageViewModel.GetInstance();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mainPageViewModel.ChatMessages.Add(formattedMessage);
            });
        }
    }
}
