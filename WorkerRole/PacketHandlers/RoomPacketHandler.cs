using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ozwego.BuddyManagement;
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
            var clientManager = ClientManager.GetInstance();
            var roomManager = RoomManager.GetInstance();
            Friend friend;

            switch (PacketType)
            {
                case PacketType.ClientLogIn:
                    friend = Data as Friend;

                    if (null != friend)
                    {
                        ProcessLogInPacket(client, friend);
                    }
                    break;

                case PacketType.ClientLogOut:
                    clientManager.RemoveClient(client);
                    break;

                case PacketType.ClientJoinRoom:
                    friend = Data as Friend;

                    if (null != friend)
                    {
                        client = ProcessJoinRoomPacket(client, friend);
                    }
                    break;

                case PacketType.ClientLeaveRoom:
                    roomManager.RemoveMemberfromRoom(client.Room.Host, client);


                    //
                    //  Add the user to a newly createad room.
                    //

                    var newRoom = roomManager.CreateNewRoom(client);

                    client.Room = newRoom;

                    break;

                case PacketType.ClientInitiateGame:

                    //
                    // Only allow the room host to initiate a game.
                    //

                    if (client.Room.GetHostAddress() == client.UserInfo.EmailAddress)
                    {
                        MessageSender.BroadcastMessage(
                            client.Room.Members,
                            PacketType.ServerBeginGameInitialization,
                            null,
                            client);
                    }

                    break;

                case PacketType.ClientChat:
                    var arguments = Data as ChatMessage;

                    if (arguments != null)
                    {
                        MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerChat, arguments, client);
                    }
                    break;

                case PacketType.ClientReadyForGameStart:
                    client.Room.SignalClientIsReadyForGame(client);
                    break;

                default:
                    Trace.WriteLine(string.Format("[RoomPacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
                    break;
            }
        }

        private Client ProcessJoinRoomPacket(Client client, Friend joiner)
        {
            var clientManager = ClientManager.GetInstance();
            var clientToJoin = clientManager.GetClientList().FirstOrDefault(
                myClient => myClient.UserInfo.EmailAddress == joiner.EmailAddress);

            if (clientToJoin != null)
            {
                var roomManager = RoomManager.GetInstance();
                roomManager.AddMemberToRoom(clientToJoin.Room.Host, ref client);
            }

            return client;
        }


        private void ProcessLogInPacket(Client client, Friend friend)
        {
            //
            // Sign out anyone who is using the same user name.
            //

            var clientManager = ClientManager.GetInstance();
            var duplicateClient = clientManager.GetClientList()
                .FirstOrDefault(c => c.UserInfo.EmailAddress == friend.EmailAddress);

            if (duplicateClient != null)
            {
                duplicateClient.Disconnect();
            }

            client.UserInfo = friend;

            clientManager.AddClient(client);

            //
            // Look for the user in the database.  If the user does not exist, add 
            // 'em to the DB
            //

            var db = Database.GetInstance();
            var user = db.GetUserByEmail(client.UserInfo.EmailAddress);

            if (user == null)
            {
                db.AddNewUser(client.UserInfo.EmailAddress, client.UserInfo.Alias);
            }

            user = db.GetUserByEmail(client.UserInfo.EmailAddress);

            var userAsFriend = MessageReceiver.GetFriendFromUser(user);


            //
            // Send the user a copy of his/her stats.
            //

            MessageSender.SendMessage(
                client,
                PacketType.ServerUserStats,
                userAsFriend);


            //
            // Send both the online list and the complete list of friends to the user.
            //

            MessageReceiver.SendOnlineAndCompleteFriendList(client);


            //
            // Send pending friend requests to user.
            //

            var pendingRequests = db.GetPendingFriendRequests(client.UserInfo.EmailAddress);

            if (null != pendingRequests)
            {
                var pendingFriendRequests = MessageReceiver.CreateFriendListFromUserList(pendingRequests);

                MessageSender.SendMessage(
                        client,
                        PacketType.ServerFriendRequests,
                        pendingFriendRequests);
            }
        }
    }
}
