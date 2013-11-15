using Ozwego.BuddyManagement;
using Ozwego.Storage;
using Shared;
using System;
using WorkerRole.DataTypes;

namespace Ozwego.Server.MessageProcessors
{
    internal class DataBaseMessageProcessor : MessageProcessor
    {
        public delegate void DataBaseMessageEventHandler(object sender, string message);


        public static event DataBaseMessageEventHandler DataBaseMessageReceivedEvent;


        private void OnMessageReceived(string message)
        {
            DataBaseMessageEventHandler handler = DataBaseMessageReceivedEvent;

            if (handler != null)
            {
                handler(this, message);
            }
        }


        public override void DoActions(PacketType packetType, object data, string senderEmailAddress)
        {
            var separators = new string[] {","};
            var friendManager = FriendManager.GetInstance();

            Friend friend;
            FriendList friendList;

            switch (packetType)
            {
                case PacketType.UserLoggedIn:
                    friend = data as Friend;

                    if (null != friend)
                    {
                        friendManager.OnBuddySignIn(friend);
                    }

                    break;


                case PacketType.UserLoggedOut:
                    friend = data as Friend;

                    if (null != friend)
                    {
                        friendManager.OnBuddySignOut(friend);
                    }
                    break;

                case PacketType.ServerOnlineFriendList:
                    friendList = data as FriendList;

                    if (friendList != null)
                    {
                        friendManager.InitializeOnlineBuddyList(friendList.Friends);
                    }

                    break;

                    // This packet type contains all of the clients friends (both online and offline).
                case PacketType.ServerFriendList:
                    friendList = data as FriendList;

                    if (friendList != null)
                    {
                        friendManager.InitializeCompleteBuddyList(friendList.Friends);
                    }

                    break;

                case PacketType.ServerFriendRequests:
                    friendList = data as FriendList;

                    if (friendList != null)
                    {
                        foreach (Friend f in friendList.Friends)
                        {
                            var requestManager = RequestManager.GetInstance();
                            requestManager.NewFriendRequest(f);
                        }
                    }

                    break;

                case PacketType.ServerFriendRequestAccepted:
                    friend = data as Friend;

                    if (friend != null)
                    {
                        friendManager.AddNewBuddy(friend);
                    }

                    break;

                case PacketType.ServerFriendSearchResults:
                    friendManager.FriendSearchResultsList.Clear();

                    friendList = data as FriendList;

                    if (friendList != null)
                    {
                        foreach (Friend f in friendList.Friends)
                        {
                            friendManager.FriendSearchResultsList.Add(f);
                        }
                    }

                    break;

                case PacketType.ServerUserStats:
                    var tempFriend = data as Friend;

                    // ToDo: Don't just ovewrite this blindly.  We need to make sure that if the user has
                    // gained experience in offline mode, that that isn't overwritten here.  It needs to 
                    // be uploaded first.
                    if (tempFriend != null)
                    {
                        Settings.Alias = tempFriend.Alias;
                        Settings.CreationTime = tempFriend.CreationTime;
                        Settings.Level = tempFriend.Ranking;
                        Settings.Experience = tempFriend.Experience;
                        Settings.Level = tempFriend.Level;
                        Settings.Ranking = tempFriend.Ranking;
                    }

                    break;

                case PacketType.ServerIsAliasAvailable:
                    OnMessageReceived(data as String);
                    break;

                default:
                    throw new ArgumentException(
                        "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }
}
