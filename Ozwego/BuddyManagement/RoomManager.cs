using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ozwego.Server;
using Windows.UI.Core;

namespace Ozwego.BuddyManagement
{
    public class RoomManager
    {

        private static RoomManager _instance;


        /// <summary>
        /// Private Constructor
        /// </summary>
        private RoomManager()
        {
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
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //
                // First check to make sure that the buddy doesn't already exist in the room.  If
                // not, then add the buddy.
                //

                Buddy budref = App.MainPageViewModel.RoomMembers.FirstOrDefault(
                    b => b.MicrosoftAccountAddress == buddyAccountAddress);

                if (budref == null)
                {
                    App.MainPageViewModel.RoomMembers.Add(new Buddy(buddyAccountAddress));
                }
            });
        }


        /// <summary>
        /// This method is called when a server notification is received saying that someone has
        /// left your current room.
        /// </summary>
        /// <param name="buddyAccountAddress">buddy that is leaving the room</param>
        public async void RemoveMemberFromRoom(string buddyAccountAddress)
        {
            Buddy budref = App.MainPageViewModel.RoomMembers.FirstOrDefault(
                b => b.MicrosoftAccountAddress == buddyAccountAddress);

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //
                // Don't remove yourself from the room.
                //

                if ((budref != null) &&
                    (buddyAccountAddress != App.ClientBuddyInstance.MicrosoftAccountAddress))
                {
                    App.MainPageViewModel.RoomMembers.Remove(budref);
                }
            });
        }


        public async void JoinRoom(Buddy buddyToJoin)
        {
            await App.ServerProxy.messageSender.SendMessage(
                    PacketType.JoinRoom,
                    buddyToJoin.MicrosoftAccountAddress);
        }


        public async void JoinRoom(string accountAddress)
        {
            await App.ServerProxy.messageSender.SendMessage(
                    PacketType.JoinRoom,
                    accountAddress);
        }


        public async void LeaveRoom()
        {
            //
            // Send a message to the server that you're leaving.
            //

            await App.ServerProxy.messageSender.SendMessage(PacketType.LeaveRoom);

            //
            // Add the client (now with a new room GUID) back to the room list.
            //

            App.MainPageViewModel.RoomMembers.Clear();
            AddMemberToRoom(App.ClientBuddyInstance.MicrosoftAccountAddress);
        }


        public async void ChangeRoomHost(string newHostName)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.MainPageViewModel.RoomHost = newHostName;
            });
            
            if (newHostName != App.ClientBuddyInstance.MicrosoftAccountAddress)
            {
                //ToDO: Disable Start Game with Room Button.
            }
        }


        public async void InitiateMessageSend(string message)
        {
            if (App.ServerProxy.messageSender != null)
            {
                await App.ServerProxy.messageSender.SendMessage(PacketType.ClientChat, message);
            }
        }


        public async void ChatMessageReceived(string senderName, string message)
        {
            string formattedMessage = String.Format("{0}: {1}", senderName, message);

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.MainPageViewModel.ChatMessages.Add(formattedMessage);
            });
        }
    }
}
