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
    public class IncomingMessageHandler
    {
        // Singleton
        private static IncomingMessageHandler _instance;

        private IncomingMessageHandler() { }


        public static IncomingMessageHandler GetIncomingMessageHandler()
        {
            return _instance ?? (_instance = new IncomingMessageHandler());
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
            int messageLength = msgBytes.Length;
            string rawMessage = "";

            if (messageLength == 0)
            {
                return;
            }
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
                messageFields.Add(kvp[0], kvp[1]);
            }

            string message;
            if (!messageFields.TryGetValue("message", out message))
            {
                Trace.WriteLine("'message' field was not found in incoming message");
            }

            string sender;
            if (!messageFields.TryGetValue("sender", out sender))
            {
                Trace.WriteLine("'sender' field was not found in incoming message");
            }


            //
            // Recipients is not a required field. Thus, do not throw if it's not found.
            //

            string msgRecipients;
            messageFields.TryGetValue("recipients", out msgRecipients);


            //
            // Extract the packet type.
            //

            var packetType = (PacketType)msgBytes[0];

            if (packetType >= PacketType.ClientMaxValue)
            {
                Trace.WriteLine(string.Format("[IncomingMessageHandler.HandleMessage] - " + 
                        "Invalid packet type from client PacketType = {0}", packetType));
            }

            List<user> matchingUsers;
            string formattedString;

            switch (packetType)
            {
                case PacketType.LogIn:
                    //
                    // Sign out anyone who is using the same user name.
                    //

                    var duplicateClient = WorkerRole.ClientManager.GetClientList()
                        .FirstOrDefault((c) => c.UserName == message);

                    if (duplicateClient != null)
                    {
                        duplicateClient.Disconnect();
                    }

                    client.UserName = message;

                    WorkerRole.ClientManager.AddClient(client);

                    //
                    // Look for the user in the database.  If the user does not exist, add 
                    // 'em to the DB
                    //

                    var user = WorkerRole.Database.GetUserByEmail(client.UserName);

                    if (user == null)
                    {
                        // ToDo: In the line below, add an alias instead of replicating the email address.
                        WorkerRole.Database.AddNewUser(client.UserName, client.UserName);
                    }

                    user = WorkerRole.Database.GetUserByEmail(client.UserName);


                    //
                    // Send the user a copy of his/her stats.
                    //

                    var userStatsString = CreateUrlStringFromUserList(new List<user> { user });

                    WorkerRole.MessageSender.SendMessage(
                        client,
                        PacketType.ServerUserStats,
                        userStatsString);


                    //
                    // Send the user a list of all of their friends.
                    //

                    var friendList = WorkerRole.Database.GetFriends(client.UserName);

                    if (null != friendList)
                    {
                        var friendListString = CreateUrlStringFromUserList(friendList);

                        WorkerRole.MessageSender.SendMessage(
                            client,
                            PacketType.ServerFriendList,
                            friendListString);


                        //
                        // Send the user a list of all of their friends who are online.
                        //

                        var OnlineFriends = new List<Client>();

                        foreach (user frd in friendList)
                        {
                            OnlineFriends.Add(WorkerRole.ClientManager.GetClientFromEmailAddress(frd.email));
                        }

                        string onlineClients =
                            WorkerRole.MessageSender.GetRecipientListFormattedString(OnlineFriends);

                        WorkerRole.MessageSender.SendMessage(
                            client,
                            PacketType.ServerOnlineFriendList,
                            onlineClients);
                    }


                    //
                    // Send pending friend requests to user.
                    //

                    var pendingRequests = WorkerRole.Database.GetPendingFriendRequests(client.UserName);

                    if (null != pendingRequests)
                    {
                        var pendingRequestsString = CreateUrlStringFromUserList(pendingRequests);

                        WorkerRole.MessageSender.SendMessage(
                            client,
                            PacketType.ServerFriendRequests,
                            pendingRequestsString);
                    }

                    break;

                case PacketType.LogOut:
                    WorkerRole.ClientManager.RemoveClient(client);
                    break;

                case PacketType.JoinRoom:
                    Client clientToJoin = WorkerRole.ClientManager.GetClientList().FirstOrDefault(
                        myClient => myClient.UserName == message);

                    if (clientToJoin != null)
                    {
                        WorkerRole.RoomManager.AddMemberToRoom(clientToJoin.Room.Host, ref client);
                    }

                    break;
                    
                case PacketType.LeaveRoom:
                    WorkerRole.RoomManager.RemoveMemberfromRoom(client.Room.Host, client);
                    break;
                    
                case PacketType.InitiateGame:

                    //
                    // Only allow the room host to initiate a game.
                    //

                    if (client.Room.Host.UserName == client.UserName)
                    {
                        WorkerRole.MessageSender.BroadcastMessage(
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
                        WorkerRole.MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerGameStart, "");
                    }
                    break;

                case PacketType.ClientDump:
                    WorkerRole.MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerDump, "", client);
                    break;

                case PacketType.ClientPeel:
                    WorkerRole.MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerPeel, "", client);
                    break;
                    
                case PacketType.ClientVictory:
                    WorkerRole.MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerGameOver, "");
                    break;

                case PacketType.ClientChat:
                    var arguments = new Dictionary<string, string> {{"sender", sender}, {"message", message}};
                    WorkerRole.MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerChat, arguments, client);
                    break;

                case PacketType.ClientAcceptFriendRequest:
                    WorkerRole.Database.AcceptFriendRequest(message, sender);


                    //
                    // If the person whose friend request was accepted is online, send them a
                    // notification so that the friends can begin playing immediately.
                    //

                    Client curClient = WorkerRole.ClientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = WorkerRole.Database.GetMatchingUsersByEmail(sender);
                        formattedString = CreateUrlStringFromUserList(matchingUsers);

                        WorkerRole.MessageSender.SendMessage(curClient, PacketType.ServerFriendRequestAccepted, formattedString);
                        WorkerRole.MessageSender.SendMessage(curClient, PacketType.UserLoggedIn, sender);
                    }
                    break;

                case PacketType.ClientRejectFriendRequest:
                    WorkerRole.Database.RejectFriendRequest(message, sender);
                    break;

                case PacketType.ClientSendFriendRequest:
                    WorkerRole.Database.SendFriendRequest(sender, message);

                    curClient = WorkerRole.ClientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = WorkerRole.Database.GetMatchingUsersByEmail(sender);
                        formattedString = CreateUrlStringFromUserList(matchingUsers);

                        WorkerRole.MessageSender.SendMessage(curClient, PacketType.ServerFriendRequests, formattedString);
                    }
                    break;

                case PacketType.ClientRemoveFriend:
                    WorkerRole.Database.RemoveFriendship(sender, message);

                    curClient = WorkerRole.ClientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = WorkerRole.Database.GetMatchingUsersByEmail(sender);
                        formattedString = CreateUrlStringFromUserList(matchingUsers);

                        WorkerRole.MessageSender.SendMessage(curClient, PacketType.ServerRemoveFriend, formattedString);
                    }
                    break;

                case PacketType.ClientFindBuddyFromGlobalList:
                    matchingUsers = WorkerRole.Database.GetMatchingUsersByEmail(message);

                    formattedString = CreateUrlStringFromUserList(matchingUsers);

                    WorkerRole.MessageSender.SendMessage(client, PacketType.ServerFriendSearchResults, formattedString);
                    break;

                case PacketType.ClientStartingMatchmaking:
                    throw new NotImplementedException();
                    break;

                case PacketType.ClientStoppingMatchmaking:
                    throw new NotImplementedException();
                    break;

                default:
                    //ToDo: Error Handling.
                    //throw new ArgumentException(
                    //    string.Format("[IncomingMessageHandler.HandleMessage] - " +
                    //    "Invalid packet type from client PacketType = {0}", packetType));
                    break;

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

            foreach (var user in users)
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

            return newFormattedString.TrimEnd(',');
        }
    }
}
