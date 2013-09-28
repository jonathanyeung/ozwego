using Ozwego.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        private MessageReceiver() { }


        public static MessageReceiver GetInstance()
        {
            return _instance ?? (_instance = new MessageReceiver());
        }


        /// <summary>
        /// Determines what to do with an incoming message
        /// </summary>
        /// <param name="client">
        /// the client connection that sent the message</param>
        /// <param name="msgBytes">
        /// the message buffer</param>
        public void HandleMessage(ref Client client, byte[] msgBytes)
        {
            string message;
            string sender;
            string messageRecipients;
            PacketType packetType;
            List<user> matchingUsers;
            string formattedString;

            if (0 == msgBytes.Length)
            {
                return;
            }

            //ToDo: Add return value on ExtractParameters.  if it fails, then return immediately.
            ExtractParameters(msgBytes, out message, out sender, out messageRecipients, out packetType);

            var messageSender = MessageSender.GetMessageSender();
            var roomManager = RoomManager.GetInstance();
            var db = Database.GetInstance();
            var clientManager = ClientManager.GetInstance();

            switch (packetType)
            {
                case PacketType.LogIn:
                    ProcessLogInPacket(client, message);
                    break;

                case PacketType.LogOut:
                    clientManager.RemoveClient(client);
                    break;

                case PacketType.JoinRoom:
                    client = ProcessJoinRoomPacket(client, message);
                    break;
                    
                case PacketType.LeaveRoom:
                    roomManager.RemoveMemberfromRoom(client.Room.Host, client);
                    break;
                    
                case PacketType.InitiateGame:

                    //
                    // Only allow the room host to initiate a game.
                    //

                    if (client.Room.Host.UserName == client.UserName)
                    {
                        messageSender.BroadcastMessage(
                            client.Room.Members,
                            PacketType.ServerInitiateGame,
                            "",
                            client);
                    }
                    break;
                    

                case PacketType.StartGame:

                    //
                    // Only allow the room host to start the game.
                    //

                    if (client.Room.Host.UserName == client.UserName)
                    {
                        messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerGameStart, "");
                    }
                    break;

                case PacketType.ClientDump:
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerDump, "", client);
                    break;

                case PacketType.ClientPeel:
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerPeel, "", client);
                    break;
                    
                case PacketType.ClientVictory:
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerGameOver, "");
                    break;

                case PacketType.ClientChat:
                    var arguments = new Dictionary<string, string> {{"sender", sender}, {"message", message}};
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerChat, arguments, client);
                    break;

                case PacketType.ClientAcceptFriendRequest:
                    db.AcceptFriendRequest(message, sender);


                    //
                    // If the person whose friend request was accepted is online, send them a
                    // notification so that the friends can begin playing immediately.
                    //

                    Client curClient = clientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(sender);
                        formattedString = CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(curClient, PacketType.ServerFriendRequestAccepted, formattedString);
                        messageSender.SendMessage(curClient, PacketType.UserLoggedIn, sender);
                    }


                    //
                    // Resend the client who accepted the friend request an updated list of who is 
                    // online and who is on their complete friends list now that it's been updated.
                    //

                    SendOnlineAndCompleteFriendList(client);

                    break;

                case PacketType.ClientRejectFriendRequest:
                    db.RejectFriendRequest(message, sender);
                    break;

                case PacketType.ClientSendFriendRequest:
                    db.SendFriendRequest(sender, message);

                    curClient = clientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(sender);
                        formattedString = CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(curClient, PacketType.ServerFriendRequests, formattedString);
                    }
                    break;

                case PacketType.ClientRemoveFriend:
                    db.RemoveFriendship(sender, message);

                    curClient = clientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(sender);
                        formattedString = CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(curClient, PacketType.ServerRemoveFriend, formattedString);
                    }
                    break;

                case PacketType.ClientFindBuddyFromGlobalList:
                    matchingUsers = db.GetMatchingUsersByEmail(message);

                    if (null != matchingUsers)
                    {
                        formattedString = CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(client, PacketType.ServerFriendSearchResults, formattedString);
                    }
                    break;

                case PacketType.ClientStartingMatchmaking:
                    //throw new NotImplementedException();
                    break;

                case PacketType.ClientStoppingMatchmaking:
                    //throw new NotImplementedException();
                    break;

                default:
                    Trace.WriteLine(string.Format("[IncomingMessageHandler.HandleMessage] - " +
                        "Invalid packet type from client PacketType = {0}", packetType));
                    break;
            }
        }


        private Client ProcessJoinRoomPacket(Client client, string message)
        {
            var clientManager = ClientManager.GetInstance();
            Client clientToJoin = clientManager.GetClientList().FirstOrDefault(
                myClient => myClient.UserName == message);

            if (clientToJoin != null)
            {
                var roomManager = RoomManager.GetInstance();
                roomManager.AddMemberToRoom(clientToJoin.Room.Host, ref client);
            }

            return client;
        }


        private void ProcessLogInPacket(Client client, string message)
        {
            //
            // Sign out anyone who is using the same user name.
            //

            var clientManager = ClientManager.GetInstance();
            var duplicateClient = clientManager.GetClientList()
                .FirstOrDefault((c) => c.UserName == message);
            
            if (duplicateClient != null)
            {
                duplicateClient.Disconnect();
            }

            client.UserName = message;

            clientManager.AddClient(client);

            //
            // Look for the user in the database.  If the user does not exist, add 
            // 'em to the DB
            //

            var db = Database.GetInstance();
            var user = db.GetUserByEmail(client.UserName);

            if (user == null)
            {
                // ToDo: In the line below, add an alias instead of replicating the email address.
                db.AddNewUser(client.UserName, client.UserName);
            }

            user = db.GetUserByEmail(client.UserName);


            //
            // Send the user a copy of his/her stats.
            //

            var userStatsString = CreateUrlStringFromUserList(new List<user> { user });

            var messageSender = MessageSender.GetMessageSender();
            messageSender.SendMessage(
                client,
                PacketType.ServerUserStats,
                userStatsString);


            //
            // Send both the online list and the complete list of friends to the user.
            //

            SendOnlineAndCompleteFriendList(client);


            //
            // Send pending friend requests to user.
            //

            var pendingRequests = db.GetPendingFriendRequests(client.UserName);

            if (null != pendingRequests)
            {
                var pendingRequestsString = CreateUrlStringFromUserList(pendingRequests);

                messageSender.SendMessage(
                    client,
                    PacketType.ServerFriendRequests,
                    pendingRequestsString);
            }
        }

        //ToDo: Make this method return bool.  If a required field is not present, then return false.
        /// <summary>
        /// Helper method to extract the required message fields from the passed in byte buffer of
        /// an incoming message from a client.
        /// </summary>
        /// <param name="msgBytes">the raw byte buffer containing the complete message</param>
        /// <param name="message">out parameter containing the core messasge contents</param>
        /// <param name="sender">out parameter containing the name of the sender</param>
        /// <param name="messageRecipients">out parameter containing the optional list of
        /// message recipients. </param>
        /// <param name="packetType">out parameter containing the packet type</param>
        private void ExtractParameters(
            byte[] msgBytes, 
            out string message, 
            out string sender, 
            out string messageRecipients, 
            out PacketType packetType)
        {
            int messageLength = msgBytes.Length;
            string rawMessage = "";

            if (messageLength > 1)
            {
                var tempArray = new byte[messageLength - 1];
                Array.Copy(msgBytes, 1, tempArray, 0, msgBytes.Length - 1);
                rawMessage = Encoding.UTF8.GetString(tempArray);
            }

            //
            // Extract the string fields from the www form url.
            //

            string[] messageArray = rawMessage.Split('&');
            var messageFields = new Dictionary<string, string>();

            foreach (string s in messageArray)
            {
                string[] kvp = s.Split('=');
                if (kvp.Length >= 2)
                {
                    messageFields.Add(kvp[0], kvp[1]);
                }
                else
                {
                    Trace.WriteLine("MessageReceiver.ExtractParameters - Invalid message does not contain an '='");
                }
            }


            if (!messageFields.TryGetValue("message", out message))
            {
                Trace.WriteLine("MessageReceiver.ExtractParameters - 'message' field was not found in incoming message");
            }


            if (!messageFields.TryGetValue("sender", out sender))
            {
                Trace.WriteLine("MessageReceiver.ExtractParameters - 'sender' field was not found in incoming message");
            }


            //
            // Recipients is not a required field. Thus, do not throw if it's not found.
            //

            messageFields.TryGetValue("recipients", out messageRecipients);


            //
            // Extract the packet type.
            //

            packetType = (PacketType)msgBytes[0];

            if (packetType >= PacketType.ClientMaxValue)
            {
                Trace.WriteLine(string.Format("MessageReceiver.ExtractParameters - " +
                        "Invalid packet type from client PacketType = {0}", packetType));
            }
        }


        /// <summary>
        /// Helper method that sends two messages to the user: A complete friend list,
        /// and then a list of friends that are online.
        /// </summary>
        /// <param name="client"></param>
        private void SendOnlineAndCompleteFriendList(Client client)
        {
            //
            // Send the user a list of all of their friends.
            //

            var db = Database.GetInstance();
            var friendList = db.GetFriends(client.UserName);

            if (null != friendList)
            {
                var friendListString = CreateUrlStringFromUserList(friendList);

                var messageSender = MessageSender.GetMessageSender();
                messageSender.SendMessage(
                    client,
                    PacketType.ServerFriendList,
                    friendListString);


                //
                // Send the user a list of all of their friends who are online.
                //

                var onlineUsers = new List<user>();

                foreach (user frd in friendList)
                {
                    var clientManager = ClientManager.GetInstance();
                    var onlineClient = clientManager.GetClientFromEmailAddress(frd.email);

                    if (null != onlineClient)
                    {
                        onlineUsers.Add(frd);
                    }
                }

                string onlineFriends = CreateUrlStringFromUserList(onlineUsers);

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
        public static string CreateUrlStringFromUserList(List<user> users)
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
