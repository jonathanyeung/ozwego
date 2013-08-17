using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace WorkerRole
{
    /// <summary>
    /// This processes incoming messages sent from the client and performs the appropriate action.
    /// </summary>
    public class IncomingMessageHandler
    {
        // Singleton
        private static IncomingMessageHandler _instance;

        private IncomingMessageHandler()
        {
        }


        public static IncomingMessageHandler GetIncomingMessageHandler()
        {
            return _instance ?? (_instance = new IncomingMessageHandler());
        }


        /// <summary>
        /// Determines what to do with an incoming message
        /// </summary>
        /// <param name="client">
        /// the client connection that sent the message</param>
        /// <param name="endpointType">The TCP endpoint type of the source of this message</param>
        /// <param name="msgBytes">
        /// the message buffer</param>
        public void HandleMessage(ref Client client, EndpointType endpointType, byte[] msgBytes)
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
                Trace.WriteLine(
                    " IncomingMessageHandler.HandleMessage Error: 'message' field was not found in incoming message");
            }

            string sender;
            if (!messageFields.TryGetValue("sender", out sender))
            {
                Trace.WriteLine(
                    " IncomingMessageHandler.HandleMessage Error: 'message' field was not found in incoming message");
            }


            //
            // Recipients is not a required field. Thus, do not throw if it's not found.
            //

            string msgRecipients;
            messageFields.TryGetValue("recipients", out msgRecipients);


            //
            // Extract the packet type.
            //

            var packetType = (PacketType) msgBytes[0];

            if (packetType >= PacketType.ClientMaxValue)
            {
                Trace.WriteLine(string.Format(" IncomingMessageHandler.HandleMessage " +
                                              "Error: Invalid packet type from client PacketType = {0}", packetType));
            }

            switch (endpointType)
            {
                case EndpointType.External:
                    PerformActionsForExternalClientMessage(
                            client as ExternalClient,
                            packetType,
                            sender,
                            message,
                            msgRecipients);
                    break;

                case EndpointType.Internal:
                    PerformActionsForInternalClientMessage(
                            client as InternalClient,
                            packetType,
                            sender,
                            message,
                            msgRecipients);
                    break;
            }
        }


        private void PerformActionsForInternalClientMessage(
            InternalClient client,
            PacketType packetType,
            string sender,
            string message,
            string msgRecipients)
        {
            switch (packetType)
            {
                case PacketType.UserLoggedOut:
                case PacketType.UserLoggedIn:
                case PacketType.UserJoinedRoom:
                case PacketType.UserLeftRoom:
                case PacketType.HostTransfer:
                case PacketType.ServerPeel:
                case PacketType.ServerDump:
                case PacketType.ServerGameStart:
                case PacketType.ServerGameOver:
                case PacketType.ServerChat:
                case PacketType.ServerInitiateGame:
                    //ToDo: Check to make sure this "message" is what should be sent here...
                    WorkerRole.MessageSender.SendMessage(msgRecipients, packetType, message);
                    break;

                case PacketType.ForceDisconnect:
                    ExternalClient clientToDisconnect = WorkerRole.ClientManager.GetLocalClient(message);

                    if (client != null)
                    {
                        clientToDisconnect.Disconnect();
                    }
                    break;

                default:
                    Trace.WriteLine(string.Format(" IncomingMessageHandler.HandleMessage " +
                              "Error: Invalid packet type from external client. PacketType = {0}", packetType));
                    break;
            }
        }


        private void PerformActionsForExternalClientMessage(
            ExternalClient client,
            PacketType packetType,
            string sender,
            string message,
            string msgRecipients)
        {
            var roomMembers = WorkerRole.RoomManager.GetRoomMembers(client.Information.RoomHost);

            switch (packetType)
            {
                case PacketType.LogIn:
                    //
                    // Sign out anyone who is using the same user name.
                    //
                    //ToDo: Verify this is correct: sender was message.
                    var duplicateClient = WorkerRole.ClientManager.GetGlobalClientList()
                                                    .FirstOrDefault((c) => c == sender);

                    if (duplicateClient != null)
                    {
                        var clientInfo = WorkerRole.ClientManager.GetClientInformation(duplicateClient);

                        if (clientInfo.ServerInstanceId == WorkerRole.instanceID)
                        {
                            WorkerRole.ClientManager.GetLocalClient(clientInfo.UserName).Disconnect();
                        }
                        else
                        {
                            WorkerRole.MessageSender.SendMessage(clientInfo.UserName, PacketType.ForceDisconnect, clientInfo.UserName);
                        }
                    }

                    //ToDo: Verify this is correct: sender was message.
                    client.Information.UserName = sender;
                    WorkerRole.ClientManager.AddClient(client);


                    //
                    // Create a new room for the new connecting client.
                    //
                    client.Information.RoomHost = client.Information.UserName;
                    WorkerRole.RoomManager.CreateNewRoom(client);


                    //
                    // Since the user is just logging in, send them a copy of the global buddy list.
                    //

                    string recipients =
                        WorkerRole.MessageSender.GetRecipientListFormattedString(
                            WorkerRole.ClientManager.GetGlobalClientList());

                    WorkerRole.MessageSender.SendMessage(
                        client.Information.UserName,
                        PacketType.ServerBuddyList,
                        recipients);

                    break;

                case PacketType.LogOut:
                    WorkerRole.ClientManager.RemoveClient(client);
                    break;

                case PacketType.JoinRoom:
                    var clientToJoin = WorkerRole.ClientManager.GetGlobalClientList().FirstOrDefault(
                        myClient => myClient == message);

                    var roomHost = WorkerRole.RoomManager.GetRoomHost(clientToJoin);

                    if (clientToJoin != null)
                    {
                        WorkerRole.RoomManager.AddMemberToRoom(roomHost, ref client);
                    }

                    break;

                case PacketType.LeaveRoom:
                    WorkerRole.RoomManager.RemoveMemberfromRoom(client.Information.RoomHost, client);
                    break;

                case PacketType.InitiateGame:
                    //
                    // Only allow the room host to initiate a game.
                    //

                    if (client.Information.RoomHost == client.Information.UserName)
                    {
                        WorkerRole.MessageSender.BroadcastMessage(
                            roomMembers,
                            PacketType.ServerInitiateGame,
                            new Dictionary<string, string> {{"sender", client.Information.UserName}},
                            client.Information.UserName);
                    }
                    break;


                case PacketType.StartGame:
                    //
                    // Only allow the room host to start the game.
                    //

                    if (client.Information.RoomHost == client.Information.UserName)
                    {
                        WorkerRole.MessageSender.BroadcastMessage(
                            roomMembers,
                            PacketType.ServerGameStart,
                            new Dictionary<string, string> {{"sender", client.Information.UserName}},
                            client.Information.UserName);
                    }
                    break;

                case PacketType.ClientDump:
                    WorkerRole.MessageSender.BroadcastMessage(
                        roomMembers,
                        PacketType.ServerDump,
                        new Dictionary<string, string> {{"sender", client.Information.UserName}},
                        client.Information.UserName);
                    break;

                case PacketType.ClientPeel:
                    WorkerRole.MessageSender.BroadcastMessage(
                        roomMembers,
                        PacketType.ServerPeel,
                        new Dictionary<string, string> {{"sender", client.Information.UserName}},
                        client.Information.UserName);
                    break;

                case PacketType.ClientVictory:
                    WorkerRole.MessageSender.BroadcastMessage(
                        roomMembers,
                        PacketType.ServerGameOver,
                        new Dictionary<string, string> {{"sender", client.Information.UserName}},
                        client.Information.UserName);
                    break;

                case PacketType.ClientChat:
                    var arguments = new Dictionary<string, string> {{"sender", sender}, {"message", message}};

                    WorkerRole.MessageSender.BroadcastMessage(
                        roomMembers,
                        PacketType.ServerChat,
                        arguments,
                        client.Information.UserName);
                    break;

                default:
                    Trace.WriteLine(string.Format(" IncomingMessageHandler.HandleMessage " +
                                                  "Error: Invalid packet type from external client PacketType = {0}", packetType));
                    break;
            }
        }
    }
}
