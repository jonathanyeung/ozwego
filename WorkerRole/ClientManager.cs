
using System.Collections.Generic;
using Shared;
using WorkerRole.Datacore;

namespace WorkerRole
{
    /// <summary>
    /// Contains the global list of connected clients, and handles basic operations regarding the
    /// global list.
    /// ToDo: Make this class scalable across multiple WorkerRole instances.
    /// </summary>
    public class ClientManager
    {
        // The list of all clients with a TCP connection to this WorkerRole instance.
        //ToDo: Change this from a list to a hash table.
        private List<Client> _globalClientList;
        private object clientListLock;

        // Singleton
        private static ClientManager _instance;

        private ClientManager()
        {
            _globalClientList = new List<Client>();
            clientListLock = new object();
        }

        public static ClientManager GetInstance()
        {
            return _instance ?? (_instance = new ClientManager());
        }

        public List<Client> GetClientList()
        {
            lock (clientListLock)
            {
                return _globalClientList;
            }
        }


        public Client GetClientFromEmailAddress(string email)
        {
            Client curClient = null;

            lock (clientListLock)
            {
                foreach (Client c in _globalClientList)
                {
                    if (c.UserInfo.EmailAddress == email)
                    {
                        curClient = c;
                        break;
                    }
                }
            }

            return curClient;
        }


        public void AddClient(Client client)
        {
            lock (clientListLock)
            {
                _globalClientList.Add(client);
            }

            var db = Database.GetInstance();

            var user = db.GetUserByEmail(client.UserInfo.EmailAddress);

            if (user == null)
            {
                return;
            }

            var friend = MessageReceiver.GetFriendFromUser(user);


            //
            // Send a message saying that the user logged on to all of his friends who are online.
            //

            var friends = db.GetFriends(client.UserInfo.EmailAddress);
            var onlineFriends = new List<Client>();

            if (friends == null)
            {
                return;
            }

            foreach (user frd in friends)
            {
                var c = GetClientFromEmailAddress(frd.email);
                if (null != c)
                {
                    onlineFriends.Add(c);
                }
            }

            MessageSender.BroadcastMessage(
                onlineFriends,
                PacketType.s_UserLoggedIn,
                friend,
                client);
        }


        public void RemoveClient(Client client)
        {
            lock (clientListLock)
            {
                _globalClientList.Remove(client);
            }

            var db = Database.GetInstance();

            if (client.UserInfo == null)
            {
                return;
            }

            var user = db.GetUserByEmail(client.UserInfo.EmailAddress);

            var friend = MessageReceiver.GetFriendFromUser(user);


            //
            // Remove the user from his room in case he is still part of one.
            //

            var roomManager = RoomManager.GetInstance();

            roomManager.RemoveMemberfromRoom(client.Room.Host, client);


            //
            // Send a message saying that the user logged off to all of his friends who are online.
            //

            var friends = db.GetFriends(client.UserInfo.EmailAddress);

            if (null == friends)
            {
                return;
            }

            var onlineFriends = new List<Client>();

            foreach (user frd in friends)
            {
                var c = GetClientFromEmailAddress(frd.email);
                if (null != c)
                {
                    onlineFriends.Add(c);
                }
            }

            MessageSender.BroadcastMessage(
                    onlineFriends,
                    PacketType.s_UserLoggedOut,
                    friend,
                    client);
        }
    }
}
