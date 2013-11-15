using System;
using System.Diagnostics;
using System.IO;
using Shared;
using Shared.Serialization;
using WorkerRole.PacketHandlers;

namespace WorkerRole
{
    public static class PacketHandlerFactory
    {
        public static PacketHandler GetPacketHandler(PacketBase packetBase)
        {
            PacketHandler packetHandler = null;

            var packet = packetBase.Data as PacketV1;

            if (null == packet)
            {
                return null;
            }


            switch (packet.PacketType)
            {
                case PacketType.ClientLogIn:
                case PacketType.ClientLogOut:
                case PacketType.ClientJoinRoom:
                case PacketType.ClientLeaveRoom:
                case PacketType.ClientInitiateGame:
                case PacketType.ClientChat:
                case PacketType.ClientReadyForGameStart:
                    packetHandler = new RoomPacketHandler(
                            packet.PacketType, 
                            packet.Sender, 
                            packet.Recipients,
                            packet.Data);
                    break;

                case PacketType.ClientDump:
                case PacketType.ClientPeel:
                case PacketType.ClientVictory:
                    packetHandler = new GamePacketHandler(
                            packet.PacketType, 
                            packet.Sender, 
                            packet.Recipients,
                            packet.Data);
                    break;

                case PacketType.ClientAcceptFriendRequest:
                case PacketType.ClientRejectFriendRequest:
                case PacketType.ClientSendFriendRequest:
                case PacketType.ClientRemoveFriend:
                case PacketType.ClientFindBuddyFromGlobalList:
                case PacketType.ClientQueryIfAliasAvailable:
                case PacketType.ClientUploadGameData:
                    packetHandler = new DataBasePacketHandler(
                            packet.PacketType,
                            packet.Sender,
                            packet.Recipients,
                            packet.Data);
                    break;

                case PacketType.ClientStartingMatchmaking:
                case PacketType.ClientStoppingMatchmaking:
                    packetHandler = new MatchmakingPacketHandler(
                            packet.PacketType,
                            packet.Sender,
                            packet.Recipients,
                            packet.Data);
                    break;

                default:
                    Trace.WriteLine(string.Format("[PacketHandlerFactory] - " +
                            "Invalid packet type from client PacketType = {0}",
                            packet.PacketType.ToString()));
                    break;
            }

            return packetHandler;
        }
    }
}
