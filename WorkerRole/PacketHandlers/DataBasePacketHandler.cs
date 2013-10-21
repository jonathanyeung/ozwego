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
            var messageSender = MessageSender.GetInstance();
            var db = Database.GetInstance();
            var clientManager = ClientManager.GetInstance();
            string formattedString;

            dynamic message = null;

            Type castType = DataPacket.PacketTypeMap[PacketType];

            if (castType == typeof (string))
            {
                message = Data as string;
            }


            switch (PacketType)
            {
                case PacketType.ClientAcceptFriendRequest:
                    db.AcceptFriendRequest(message, Sender);


                    //
                    // If the person whose friend request was accepted is online, send them a
                    // notification so that the friends can begin playing immediately.
                    //

                    var curClient = clientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(Sender);
                        formattedString = MessageReceiver.CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(curClient, PacketType.ServerFriendRequestAccepted, formattedString);
                        messageSender.SendMessage(curClient, PacketType.UserLoggedIn, Sender);
                    }


                    //
                    // Resend the client who accepted the friend request an updated list of who is 
                    // online and who is on their complete friends list now that it's been updated.
                    //

                    MessageReceiver.SendOnlineAndCompleteFriendList(client);

                    break;

                case PacketType.ClientRejectFriendRequest:
                    db.RejectFriendRequest(message, Sender);
                    break;

                case PacketType.ClientSendFriendRequest:
                    db.SendFriendRequest(Sender, message);

                    curClient = clientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(Sender);
                        formattedString = MessageReceiver.CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(curClient, PacketType.ServerFriendRequests, formattedString);
                    }

                    break;

                case PacketType.ClientRemoveFriend:
                                        db.RemoveFriendship(Sender, message);

                    curClient = clientManager.GetClientFromEmailAddress(message);

                    if (curClient != null)
                    {
                        matchingUsers = db.GetMatchingUsersByEmail(Sender);
                        formattedString = MessageReceiver.CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(curClient, PacketType.ServerRemoveFriend, formattedString);
                    }

                    break;

                case PacketType.ClientFindBuddyFromGlobalList:
                    matchingUsers = db.GetMatchingUsersByEmail(message);

                    if (null != matchingUsers)
                    {
                        formattedString = MessageReceiver.CreateUrlStringFromUserList(matchingUsers);

                        messageSender.SendMessage(client, PacketType.ServerFriendSearchResults, formattedString);
                    }

                    break;

                case PacketType.ClientQueryIfAliasAvailable:
                    var users = db.GetMatchingUsersByAlias(message);

                    if (users == null || users.Count == 0)
                    {
                        messageSender.SendMessage(client, PacketType.ServerIsAliasAvailable, "true");
                    }
                    else
                    {
                        messageSender.SendMessage(client, PacketType.ServerIsAliasAvailable, "false");
                    }

                    break;

                case PacketType.ClientUploadGameData:

                    GameData data;
                    //ToDo: Move this stream conversion logic to a higher level class, this should not exist in ClientUploadGameData.
                    using (var stream = new MemoryStream(Data as byte[]))
                    {
                        var ser = new XmlSerializer(castType);

                        //data = Convert.ChangeType(ser.Deserialize(stream), castType);
                        data = (GameData) (ser.Deserialize(stream));
                    }
                    
                    //ToDo: Catch XML Exception or some type of exception here.

                    db.AddNewGameData(data);

                    break;

                case PacketType.ClientGetGameHistory:

                    var user = db.GetUserByEmail(Sender);

                    var games = db.GetUserGameHistory(user);

                    // ToDo: Check whether this should be (games != null) or (games.Count != 0)
                    if (games != null)
                    {
                        //messageSender.SendMessage(client, PacketType.ServerUserGameHistory, ??);
                    }

                    //ToDo: Finish Implementing.
                    throw new NotImplementedException();

                default:
                    Trace.WriteLine(string.Format("[DataBasePacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
                    break;
            }
        }
    }
}
