using Ozwego.Shared;
using System.Collections.Generic;
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
        private List<Client> _globalClientList;

        // Singleton
        private static ClientManager _instance;

        private ClientManager()
        {
            _globalClientList = new List<Client>();
        }

        public static ClientManager GetInstance()
        {
            return _instance ?? (_instance = new ClientManager());
        }

        public List<Client> GetClientList()
        {
            return _globalClientList;
        }


        public Client GetClientFromEmailAddress(string email)
        {
            Client curClient = null;
            foreach (Client c in _globalClientList)
            {
                if (c.UserName == email)
                {
                    curClient = c;
                    break;
                }
            }

            return curClient;
        }


        public void AddClient(Client client)
        {
            _globalClientList.Add(client);

            var db = Database.GetInstance();
            var user = db.GetUserByEmail(client.UserName);

            if (user == null)
            {
                return;
            }

            var userString = MessageReceiver.CreateUrlStringFromUserList(new List<user> { user });


            //
            // Send a message saying that the user logged on to all of his friends who are online.
            //

            var friends = db.GetFriends(client.UserName);
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

            var messageSender = MessageSender.GetMessageSender();
            string onlineClients =
                messageSender.GetRecipientListFormattedString(onlineFriends);

            messageSender.BroadcastMessage(
                onlineFriends,
                PacketType.UserLoggedIn,
                userString,
                client);
        }


        public void RemoveClient(Client client)
        {
            _globalClientList.Remove(client);

            var db = Database.GetInstance();
            var user = db.GetUserByEmail(client.UserName);
            var userString = MessageReceiver.CreateUrlStringFromUserList(new List<user> { user });


            //
            // Remove the user from his room in case he is still part of one.
            //

            var roomManager = RoomManager.GetInstance();
            roomManager.RemoveMemberfromRoom(client.Room.Host, client);


            //
            // Send a message saying that the user logged off to all of his friends who are online.
            //

            var friends = db.GetFriends(client.UserName);

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


            var messageSender = MessageSender.GetMessageSender();
            string onlineClients =
                messageSender.GetRecipientListFormattedString(onlineFriends);

            messageSender.BroadcastMessage(
                onlineFriends,
                PacketType.UserLoggedOut,
                userString,
                client);
        }
    }
}
