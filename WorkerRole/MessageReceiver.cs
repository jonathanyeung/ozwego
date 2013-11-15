
using System;
using Ozwego.BuddyManagement;
using Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using WorkerRole.DataTypes;
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
            var packetBase = new PacketBase();

            using (var stream = new MemoryStream(msgBytes))
            {
                try
                {
                    var reader = new BinaryReader(stream);

                    packetBase.Read(reader);
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Trace.WriteLine(string.Format("[IncomingMessageHandler.HandleMessage] - " +
                            "Invalid packet from client! Deserialization failed: {0}, Trace: {1}",
                            e.Message,
                            e.StackTrace));
#endif
                }
            }

            var handler = PacketHandlerFactory.GetPacketHandler(packetBase);

            if (handler != null)
            {
                handler.DoActions(ref client);
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
            var friendList = db.GetFriends(client.UserInfo.EmailAddress);

            if (null != friendList)
            {
                var friendListString = CreateFriendListFromUserList(friendList);

                MessageSender.SendMessage(
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

                var onlineFriends = CreateFriendListFromUserList(onlineUsers);

                MessageSender.SendMessage(
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
        public static FriendList CreateFriendListFromUserList(IEnumerable<user> users)
        {
            var friendList = new FriendList();

            if (null == users)
            {
                return null;
            }

            foreach (var user in users)
            {
                var friend = GetFriendFromUser(user);

                friendList.Friends.Add(friend);
            }

            return friendList;
        }

        //ToDo: Move this class elsewhere.
        public static Friend GetFriendFromUser(user user)
        {
            var friend = new Friend { Alias = user.alias, EmailAddress = user.email };

            if (user.creation_time != null)
            {
                friend.CreationTime = (DateTime)user.creation_time;
            }

            if (user.skill_level != null)
            {
                friend.Level = (int)user.skill_level;
            }

            if (user.ranking != null)
            {
                friend.Ranking = (int)user.ranking;
            }

            if (user.experience != null)
            {
                friend.Experience = (long)user.experience;
            }

            return friend;
        }
    }
}
