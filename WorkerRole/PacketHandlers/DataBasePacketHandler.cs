using Ozwego.BuddyManagement;
using Ozwego.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Shared;
using WorkerRole.Datacore;

namespace WorkerRole.PacketHandlers
{
    public class DataBasePacketHandler : PacketHandler
    {
        public DataBasePacketHandler(PacketType packetType, string sender, List<string> recipients, object data)
            : base(packetType, sender, recipients, data)
        {
        }


        public override void DoActions(ref Client client)
        {
            List<user> matchingUsers;
            var db = Database.GetInstance();
            var clientManager = ClientManager.GetInstance();

            switch (PacketType)
            {
                case PacketType.ClientAcceptFriendRequest:
                    var friend = Data as Friend;

                    if (!DataIsNull(friend))
                    {
                        db.AcceptFriendRequest(friend.EmailAddress, Sender);
                    }


                    //
                    // If the person whose friend request was accepted is online, send them a
                    // notification so that the friends can begin playing immediately.
                    //

                    var curClient = clientManager.GetClientFromEmailAddress(friend.EmailAddress);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(Sender);
                        var userList = MessageReceiver.CreateFriendListFromUserList(matchingUsers);

                        MessageSender.SendMessage(curClient, PacketType.ServerFriendRequestAccepted, userList);
                        MessageSender.SendMessage(curClient, PacketType.UserLoggedIn, Sender);
                    }


                    //
                    // Resend the client who accepted the friend request an updated list of who is 
                    // online and who is on their complete friends list now that it's been updated.
                    //

                    MessageReceiver.SendOnlineAndCompleteFriendList(client);

                    break;

                case PacketType.ClientRejectFriendRequest:
                    friend = Data as Friend;

                    if (!DataIsNull(friend))
                    {
                        db.RejectFriendRequest(friend.EmailAddress, Sender);
                    }

                    break;

                case PacketType.ClientSendFriendRequest:
                    friend = Data as Friend;

                    if (DataIsNull(friend))
                    {
                        return;
                    }

                    db.SendFriendRequest(Sender, friend.EmailAddress);

                    curClient = clientManager.GetClientFromEmailAddress(friend.EmailAddress);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(Sender);
                        var userList = MessageReceiver.CreateFriendListFromUserList(matchingUsers);

                        MessageSender.SendMessage(curClient, PacketType.ServerFriendRequests, userList);
                    }

                    break;

                case PacketType.ClientRemoveFriend:
                    friend = Data as Friend;

                    if (DataIsNull(friend))
                    {
                        return;
                    }

                    db.RemoveFriendship(Sender, friend.EmailAddress);

                    curClient = clientManager.GetClientFromEmailAddress(friend.EmailAddress);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(Sender);
                        var userList = MessageReceiver.CreateFriendListFromUserList(matchingUsers);

                        MessageSender.SendMessage(curClient, PacketType.ServerRemoveFriend, userList);
                    }

                    break;

                case PacketType.ClientFindBuddyFromGlobalList:
                    var friendQueryString = Data as string;

                    if (DataIsNull(friendQueryString))
                    {
                        return;
                    }

                    matchingUsers = db.GetMatchingUsersByEmail(friendQueryString);

                    if (null != matchingUsers)
                    {
                        var userList = MessageReceiver.CreateFriendListFromUserList(matchingUsers);

                        MessageSender.SendMessage(client, PacketType.ServerFriendSearchResults, userList);
                    }

                    break;

                case PacketType.ClientQueryIfAliasAvailable:
                    friend = Data as Friend;

                    if (DataIsNull(friend))
                    {
                        return;
                    }

                    var users = db.GetMatchingUsersByAlias(friend.EmailAddress);

                    if (users == null || users.Count == 0)
                    {
                        MessageSender.SendMessage(client, PacketType.ServerIsAliasAvailable, "true");
                    }
                    else
                    {
                        MessageSender.SendMessage(client, PacketType.ServerIsAliasAvailable, "false");
                    }

                    break;

                case PacketType.ClientUploadGameData:
                    var gameData = Data as GameData;

                    if (DataIsNull(gameData))
                    {
                        return;
                    }

                    db.AddNewGameData(gameData);

                    break;

                case PacketType.ClientGetGameHistory:

                    var user = db.GetUserByEmail(Sender);

                    var games = db.GetUserGameHistory(user);

                    // ToDo: Check whether this should be (games != null) or (games.Count != 0)
                    if (games != null)
                    {
                        MessageSender.SendMessage(client, PacketType.ServerUserGameHistory, games);
                    }

                    break;

                default:
#if DEBUG
                    throw new ArgumentException();
#endif
                    Trace.WriteLine(string.Format("[DataBasePacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
                    break;
            }
        }


        private bool DataIsNull(object castedData)
        {
            bool isNull = false;

            if (castedData == null)
            {
#if DEBUG
                throw new ArgumentNullException();
#endif
                Trace.WriteLine(string.Format("Data received in DataBasePacketHandler.DoActions is invalid. PacketType = {0}", PacketType));

                isNull = true;
            }

            return isNull;
        }
    }
}
