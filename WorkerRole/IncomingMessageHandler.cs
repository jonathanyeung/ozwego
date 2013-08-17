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
                //ToDo: Error Handling.
                Trace.WriteLine("'message' field was not found in incoming message");
                //throw new ArgumentException("'message' field was not found in incoming message");
            }

            string sender;
            if (!messageFields.TryGetValue("sender", out sender))
            {
                //ToDo: Error Handling.
                Trace.WriteLine("'message' field was not found in incoming message");
                //throw new ArgumentException("'sender' field was not found in incoming message");
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
                //ToDo: Error Handling.
                //throw new ArgumentException(
                //        string.Format("[IncomingMessageHandler.HandleMessage] - " +
                //        "Invalid packet type from client PacketType = {0}", packetType));
            }

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
                    // Since the user is just logging in, send them a copy of the global buddy list.
                    //

                    string recipients =
                        WorkerRole.MessageSender.GetRecipientListFormattedString(WorkerRole.ClientManager.GetClientList());

                    WorkerRole.MessageSender.SendMessage(
                        client,
                        PacketType.ServerBuddyList,
                        recipients);

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

                default:
                    //ToDo: Error Handling.
                    //throw new ArgumentException(
                    //    string.Format("[IncomingMessageHandler.HandleMessage] - " +
                    //    "Invalid packet type from client PacketType = {0}", packetType));
                    break;

            }
        }
    }
}
