using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Shared;
using WorkerRole.Datacore;

namespace WorkerRole.PacketHandlers
{
    public class RoomPacketHandler : PacketHandler
    {
        public RoomPacketHandler(PacketType packetType, string sender, List<string> recipients, object data)
            : base(packetType, sender, recipients, data)
        {
        }

        public override void DoActions(ref Client client)
        {
            var messageSender = MessageSender.GetInstance();
            var clientManager = ClientManager.GetInstance();
            var roomManager = RoomManager.GetInstance();
            
            //ToDo: Do a dictionary look up for casting type instead of doing a hardcoded cast here:
            var message = (string) Data;

            switch (PacketType)
            {
                case PacketType.ClientLogIn:
                    ProcessLogInPacket(client, message);
                    break;

                case PacketType.ClientLogOut:
                    clientManager.RemoveClient(client);
                    break;

                case PacketType.ClientJoinRoom:
                    client = ProcessJoinRoomPacket(client, message);
                    break;

                case PacketType.ClientLeaveRoom:
                    roomManager.RemoveMemberfromRoom(client.Room.Host, client);
                    break;

                case PacketType.ClientInitiateGame:

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

                case PacketType.ClientChat:
                    var arguments = new Dictionary<string, string> { { "sender", Sender }, { "message", message } };
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerChat, arguments, client);
                    break;

                default:
                    Trace.WriteLine(string.Format("[RoomPacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
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
                .FirstOrDefault(c => c.UserName == message);

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

            var userStatsString = MessageReceiver.CreateUrlStringFromUserList(new List<user> { user });

            var messageSender = MessageSender.GetInstance();
            messageSender.SendMessage(
                client,
                PacketType.ServerUserStats,
                userStatsString);


            //
            // Send both the online list and the complete list of friends to the user.
            //

            MessageReceiver.SendOnlineAndCompleteFriendList(client);


            //
            // Send pending friend requests to user.
            //

            var pendingRequests = db.GetPendingFriendRequests(client.UserName);

            if (null != pendingRequests)
            {
                var pendingRequestsString = MessageReceiver.CreateUrlStringFromUserList(pendingRequests);

                messageSender.SendMessage(
                    client,
                    PacketType.ServerFriendRequests,
                    pendingRequestsString);
            }
        }
    }
}
