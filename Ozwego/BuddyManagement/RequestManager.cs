using Ozwego.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;


namespace Ozwego.BuddyManagement
{
    public class RequestManager
    {
        private static RequestManager _instance;

        public readonly ObservableCollection<Friend> PendingFriendRequests;
        
        //ToDo: Complete this collection.
        //public readonly ObservableCollection<?> PendingGameRequests();

        /// <summary>
        /// Private Constructor
        /// </summary>
        private RequestManager()
        {
            PendingFriendRequests = new ObservableCollection<Friend>();
        }


        /// <summary>
        /// Public method to instantiate ServerMessageSender singleton.
        /// </summary>
        /// <returns></returns>
        public static RequestManager GetInstance()
        {
            return _instance ?? (_instance = new RequestManager());
        }


        public void NewFriendRequest(Friend friend)
        {
            PendingFriendRequests.Add(friend);
        }


        public async void AcceptFriendRequest(Friend friend)
        {
            PendingFriendRequests.Remove(friend);

            //ToDo: This null check is no bueno.  This needs to get sent at some point, 
            // this call will make the packet never get sent to the server.
            var serverProxy = ServerProxy.GetInstance();

            if (null != serverProxy.messageSender)
            {
                await serverProxy.messageSender.SendMessage(
                        PacketType.c_AcceptFriendRequest,
                        friend);
            }
        }

        public async void RejectFriendRequest(Friend friend)
        {
            PendingFriendRequests.Remove(friend);

            //ToDo: This null check is no bueno.  This needs to get sent at some point, 
            // this call will make the packet never get sent to the server.
            var serverProxy = ServerProxy.GetInstance();
            if (null != serverProxy.messageSender)
            {
                await serverProxy.messageSender.SendMessage(
                        PacketType.c_RejectFriendRequest,
                        friend);
            }
        }
    }
}
