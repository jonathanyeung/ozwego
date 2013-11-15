using Ozwego.BuddyManagement;
using Ozwego.Gameplay;
using Shared;
using System;

namespace Ozwego.Server.MessageProcessors
{
    class GameMessageProcessor : MessageProcessor
    {
        public override void DoActions(PacketType packetType, object data, string senderEmailAddress)
        {
            var gameController = GameController.GetInstance();
            Friend friend;

            switch (packetType)
            {
                case PacketType.ServerGameStart:
                    gameController.StartGame();
                    break;

                case PacketType.ServerDump:
                    friend = data as Friend;

                    if (null != friend)
                    {
                        gameController.DumpActionReceivedFromServer(friend.EmailAddress);
                    }
                    else
                    {
                        //ToDo: throw debug exception.
                    }
                    
                    break;

                case PacketType.ServerPeel:
                    friend = data as Friend;

                    if (null != friend)
                    {
                        gameController.PeelActionReceived(friend.EmailAddress, false);
                    }
                    else
                    {
                        //ToDo: throw debug exception.
                    }

                    break;

                case PacketType.ServerGameOver:
                    friend = data as Friend;

                    if (null != friend)
                    {
                        gameController.EndGame(friend.EmailAddress);
                    }
                    else
                    {
                        //ToDo: throw debug exception.
                    }

                    break;

                default:
                    throw new ArgumentException(
                            "Passed PacketType is not meant to be handled by this MessageProcessor");
            }
        }
    }
}
