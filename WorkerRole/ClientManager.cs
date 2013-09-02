using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static ClientManager GetClientManager()
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

            var user = WorkerRole.Database.GetUserByEmail(client.UserName);
            var userString = IncomingMessageHandler.CreateUrlStringFromUserList(new List<user> { user });


            //
            // Send the user a list of all of their friends who are online.
            //

            var friends = WorkerRole.Database.GetFriends(client.UserName);
            var onlineFriends = new List<Client>();

            foreach (user frd in friends)
            {
                var c = GetClientFromEmailAddress(frd.email);
                if (null != c)
                {
                    onlineFriends.Add(c);
                }
            }

            string onlineClients =
                WorkerRole.MessageSender.GetRecipientListFormattedString(onlineFriends);

            WorkerRole.MessageSender.BroadcastMessage(
                onlineFriends,
                PacketType.UserLoggedIn,
                userString,
                client);
        }


        public void RemoveClient(Client client)
        {
            _globalClientList.Remove(client);

            var user = WorkerRole.Database.GetUserByEmail(client.UserName);
            var userString = IncomingMessageHandler.CreateUrlStringFromUserList(new List<user> { user });


            //
            // Send the user a list of all of their friends who are online.
            //

            var friends = WorkerRole.Database.GetFriends(client.UserName);
            var onlineFriends = new List<Client>();

            foreach (user frd in friends)
            {
                var c = GetClientFromEmailAddress(frd.email);
                if (null != c)
                {
                    onlineFriends.Add(c);
                }
            }

            string onlineClients =
                WorkerRole.MessageSender.GetRecipientListFormattedString(onlineFriends);

            WorkerRole.MessageSender.BroadcastMessage(
                onlineFriends,
                PacketType.UserLoggedOut,
                userString,
                client);
        }
    }
}
