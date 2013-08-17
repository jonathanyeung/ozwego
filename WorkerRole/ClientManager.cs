using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationServer.Caching;

namespace WorkerRole
{
    /// <summary>
    /// Contains the global list of connected external clients, and handles basic operations 
    /// regarding the global list.
    /// 
    /// Cache objects:
    /// key: clientList; object: A list of all of the keys (which are client user names).  This
    ///     contains a list of all people signed into Ozwego. There is only one such entry in the
    ///     cache.
    /// 
    /// key: userName; object: clientInformation class containing user name, room host, and 
    ///     server GUID identifier.  There are many entries of this type in the cache.
    /// </summary>
    public class ClientManager
    {
        // A list of all of the clients connected to this server instance
        private readonly List<ExternalClient> _localClientList;


        private const string ClientListKey = "clientList";

        // Singleton
        private static ClientManager _instance;


        private ClientManager()
        {
            _localClientList = new List<ExternalClient>();
        }


        public static ClientManager GetClientManager()
        {
            return _instance ?? (_instance = new ClientManager());
        }


        /// <summary>
        /// Returns the list of TCP client user names across all server instances.
        /// </summary>
        /// <returns>Client list of everyone logged into Ozwego</returns>
        public List<string> GetGlobalClientList()
        {
            CacheOperationHandler noOperation = (list, client) => { };

            return ClientListOperation(noOperation, "");
        }


        public ExternalClient GetLocalClient(string username)
        {
            return _localClientList.FirstOrDefault((client) => client.Information.UserName == username);
        }


        /// <summary>
        /// Returns the list of actual clients on this role instance.
        /// </summary>
        /// <returns>Client list of everyone logged into Ozwego</returns>
        public List<ExternalClient> GetLocalClientList()
        {
            return _localClientList;
        }


        /// <summary>
        /// Retrieves the client information of the specified user name
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public ClientInformation GetClientInformation(string username)
        {
            //ToDo: What does this return if the user is not in the cache?
            return CacheManager.DataCache.Get(username) as ClientInformation;
        }


        /// <summary>
        /// Adds a new client to the cache
        /// </summary>
        /// <param name="client">Client to add</param>
        public void AddClient(ExternalClient client)
        {
            Trace.WriteLine(string.Format("++ ClientManger.AddClient: '{0}'", client.Information.UserName));

            _localClientList.Add(client);

            try
            {
                CacheManager.DataCache.Add(client.Information.UserName, client.Information);

                CacheOperationHandler addClient =
                    (list, client1) => list.Add(client1);

                var clientList = ClientListOperation(addClient, client.Information.UserName);

                WorkerRole.MessageSender.BroadcastMessage(
                    clientList,
                    PacketType.UserLoggedIn,
                    client.Information.UserName,
                    client.Information.UserName);
            }
            catch (DataCacheException e)
            {
                if (e.ErrorCode != DataCacheErrorCode.KeyAlreadyExists)
                {
                    Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                              e.StackTrace));
                }
            }
        }


        /// <summary>
        /// Removes a client from the local client list and the global cache.
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(ExternalClient client)
        {
            Trace.WriteLine(string.Format("++ ClientManger.RemoveClient: '{0}'", client.Information.UserName));

            _localClientList.Remove(client);

            //ToDo: Perform a null check on client.Information.UserName
            if (!CacheManager.DataCache.Remove(client.Information.UserName))
            {
                return;
            }

            CacheOperationHandler removeClient =
                (list, client1) => list.Remove(client1);

            var clientList = ClientListOperation(removeClient, client.Information.UserName);

            WorkerRole.MessageSender.BroadcastMessage(
                clientList,
                PacketType.UserLoggedOut,
                client.Information.UserName,
                client.Information.UserName);
        }


        /// <summary>
        /// Delegate for a cache operation.  This functionality is encapsulated to prevent
        /// repeating the lock/unlock code of the global key list that is required for 
        /// accessing the cache data.
        /// </summary>
        /// <param name="keyList">The list of keys in the cache dictionary</param>
        /// <param name="client">The client instance to perform an operation on</param>
        delegate void CacheOperationHandler(List<string> keyList, string clientName);


        /// <summary>
        /// Performs the specified operation on the cache key list object while observing thread
        /// safety.
        /// </summary>
        /// <param name="handler">Delegate method to perform</param>
        /// <param name="clientName">client name on which to perform an operation</param>
        /// <returns>The most updated list of online clients</returns>
        private List<string> ClientListOperation(CacheOperationHandler handler, string clientName)
        {
            var clientList = new List<string>();

            while (true)
            {
                try
                {
                    DataCacheLockHandle lockHandle;

                    //ToDo: Check this 1 second timeout and pick one that makes sense.
                    var keyList = CacheManager.DataCache.GetAndLock(
                            ClientListKey, TimeSpan.FromSeconds(1), out lockHandle) as List<string>;

                    if (keyList != null)
                    {
                        handler(keyList, clientName);

                        foreach (string key in keyList)
                        {
                            var clientele = CacheManager.DataCache.Get(key) as string;
                            if (clientele != null)
                            {
                                clientList.Add(clientele);
                            }
                        }
                    }

                    CacheManager.DataCache.PutAndUnlock(ClientListKey, keyList, lockHandle);

                    break;
                }
                catch (DataCacheException e)
                {
                    //
                    // Only continue the loop if the exception is that the object is locked.
                    //

                    if (e.ErrorCode != DataCacheErrorCode.ObjectLocked)
                    {
                        Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                                                      e.StackTrace));
                        break;
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("Error during data cache retrieval! '{0}'\n{1}", e.Message,
                              e.StackTrace));
                    break;
                }
            }

            return clientList;
        }
    }
}
