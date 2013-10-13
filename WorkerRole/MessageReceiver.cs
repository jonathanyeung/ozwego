
using Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using WorkerRole.Datacore;


namespace WorkerRole
{
    /// <summary>
    /// This processes incoming messages sent from the client and performs the appropriate action.
    /// </summary>
    public class MessageReceiver
    {
        // Singleton
        private static MessageReceiver _instance;

        private MessageReceiver()
        {
        }


        public static MessageReceiver GetInstance()
        {
            return _instance ?? (_instance = new MessageReceiver());
        }

        public void HandleMessage(ref Client client, byte[] msgBytes)
        {
            PacketBase packetBase;

            using (var stream = new MemoryStream(msgBytes))
            {
                var ser = new XmlSerializer(typeof (PacketBase));

                packetBase = (PacketBase) ser.Deserialize(stream);
            }

            switch (packetBase.PacketVersion)
            {
                case PacketVersion.Version1:
                    var handler = PacketHandlerFactory.GetPacketHandler(packetBase.Data);
                    handler.DoActions(ref client);
                    break;

                default:
                    Trace.WriteLine(string.Format("[IncomingMessageHandler.HandleMessage] - " +
                                                  "Invalid packet version from client! PacketVersion = {0}",
                                                  packetBase.PacketVersion.ToString()));
                    break;
            }
        }


        /// <summary>
        /// Helper method that sends two messages to the user: A complete friend list,
        /// and then a list of friends that are online.
        /// </summary>
        /// <param name="client"></param>
        public static void SendOnlineAndCompleteFriendList(Client client)
        {
            //
            // Send the user a list of all of their friends.
            //

            var db = Database.GetInstance();
            var friendList = db.GetFriends(client.UserName);

            if (null != friendList)
            {
                var friendListString = CreateUrlStringFromUserList(friendList);

                var messageSender = MessageSender.GetInstance();
                messageSender.SendMessage(
                    client,
                    PacketType.ServerFriendList,
                    friendListString);


                //
                // Send the user a list of all of their friends who are online.
                //

                var onlineUsers = new List<user>();

                foreach (var frd in friendList)
                {
                    var clientManager = ClientManager.GetInstance();
                    var onlineClient = clientManager.GetClientFromEmailAddress(frd.email);

                    if (null != onlineClient)
                    {
                        onlineUsers.Add(frd);
                    }
                }

                var onlineFriends = CreateUrlStringFromUserList(onlineUsers);

                messageSender.SendMessage(
                    client,
                    PacketType.ServerOnlineFriendList,
                    onlineFriends);
            }
        }


        /// <summary>
        /// Helper method that creates a sendable URL string from a given list of user data types.
        /// This method takes the fields of each user class and stringifies them together, and then
        /// combines all of the user strings together.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public static string CreateUrlStringFromUserList(IEnumerable<user> users)
        {
            var newFormattedString = "";

            if (null == users)
            {
                return newFormattedString;
            }

            foreach (var user in users)
            {
                if (user != null)
                {
                    var dic = new Dictionary<string, string> 
                        {
                            {"email", user.email },
                            {"creationTime", user.creation_time.ToString() },
                            {"alias", user.alias }
                        };

                    newFormattedString += MessageSender.CreateUrlQueryString(dic, '|');
                    newFormattedString += ',';
                }
            }

            return newFormattedString.TrimEnd(',');
        }
    }
}
