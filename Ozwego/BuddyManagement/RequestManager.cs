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

        public readonly ObservableCollection<Buddy> PendingFriendRequests;
        
        //ToDo: Complete this collection.
        //public readonly ObservableCollection<?> PendingGameRequests();

        /// <summary>
        /// Private Constructor
        /// </summary>
        private RequestManager()
        {
            PendingFriendRequests = new ObservableCollection<Buddy>();

            //ToDo: Remove temporary code.
            //for (int i = 0; i < 10; i++)
            //{
            //    var tempBuddy = new Buddy()
            //    {
            //        Alias = "IncomingFriendlyUser" + i.ToString(),
            //        EmailAddress = "IncomingFriendlyUser" + i.ToString() + "@address.com"
            //    };

            //    PendingFriendRequests.Add(tempBuddy);
            //}
        }


        /// <summary>
        /// Public method to instantiate ServerMessageSender singleton.
        /// </summary>
        /// <returns></returns>
        public static RequestManager GetInstance()
        {
            return _instance ?? (_instance = new RequestManager());
        }


        public void NewFriendRequest(Buddy buddy)
        {
            PendingFriendRequests.Add(buddy);
        }


        public async void AcceptFriendRequest(Buddy buddy)
        {
            PendingFriendRequests.Remove(buddy);

            //ToDo: This null check is no bueno.  This needs to get sent at some point, 
            // this call will make the packet never get sent to the server.
            var serverProxy = ServerProxy.GetInstance();

            if (null != serverProxy.messageSender)
            {
                await serverProxy.messageSender.SendMessage(
                        PacketType.ClientAcceptFriendRequest,
                        buddy.EmailAddress);
            }
        }

        public async void RejectFriendRequest(Buddy buddy)
        {
            PendingFriendRequests.Remove(buddy);

            //ToDo: This null check is no bueno.  This needs to get sent at some point, 
            // this call will make the packet never get sent to the server.
            var serverProxy = ServerProxy.GetInstance();
            if (null != serverProxy.messageSender)
            {
                await serverProxy.messageSender.SendMessage(
                        PacketType.ClientRejectFriendRequest,
                        buddy.EmailAddress);
            }
        }
    }
}
