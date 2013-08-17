using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void AddClient(Client client)
        {
            _globalClientList.Add(client);

            WorkerRole.MessageSender.BroadcastMessage(
                _globalClientList,
                PacketType.UserLoggedIn,
                client.UserName,
                client);
        }

        public void RemoveClient(Client client)
        {
            _globalClientList.Remove(client);

            WorkerRole.MessageSender.BroadcastMessage(
                _globalClientList,
                PacketType.UserLoggedOut,
                client.UserName,
                client);
        }
    }
}
