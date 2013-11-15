﻿using Ozwego.BuddyManagement;
using Ozwego.Gameplay;
using Ozwego.UI;
using Shared;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WorkerRole.DataTypes;

namespace Ozwego.Server.MessageProcessors
{
    class RoomMessageProcessor : MessageProcessor
    {
        public override void DoActions(PacketType packetType, object data, string senderEmailAddress)
        {
            var roomManager = RoomManager.GetInstance();
            Friend friend;

            switch (packetType)
            {
                case PacketType.ServerRoomList:
                    var friendList = data as FriendList;

                    if (friendList != null)
                    {
                        foreach (var f in friendList.Friends)
                        {
                            roomManager.AddMemberToRoom(f);
                        }
                    }

                    break;

                case PacketType.UserJoinedRoom:
                    friend = data as Friend;

                    if (friend != null)
                    {
                        roomManager.AddMemberToRoom(friend);
                    }

                    break;

                case PacketType.UserLeftRoom:
                    friend = data as Friend;

                    if (friend != null)
                    {
                        roomManager.RemoveMemberFromRoom(friend);
                    }

                    break;

                case PacketType.ServerBeginGameInitialization:
                    var currentFrame = Window.Current.Content as Frame;
                    if (currentFrame != null)
                    {
                        var args = new GameBoardNavigationArgs()
                        {
                            GameConnectionType = GameConnectionType.Online,
                            BotCount = 0
                        };

                        currentFrame.Navigate(typeof(GameBoardPrototype), args);
                    }
                    break;

                case PacketType.ServerChat:
                    var chatmessage = data as ChatMessage;

                    if (chatmessage != null)
                    {
                        roomManager.ChatMessageReceived(chatmessage);
                    }

                    break;

                case PacketType.HostTransfer:
                    friend = data as Friend;

                    if (friend != null)
                    {
                        roomManager.ChangeRoomHost(friend);
                    }

                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }
}
