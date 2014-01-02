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
                case PacketType.c_LogIn:
                case PacketType.c_LogOut:
                case PacketType.c_JoinRoom:
                case PacketType.c_LeaveRoom:
                case PacketType.c_InitiateGame:
                case PacketType.c_Chat:
                case PacketType.c_ReadyForGameStart:
                    packetHandler = new RoomPacketHandler(
                            packet.PacketType, 
                            packet.Sender, 
                            packet.Recipients,
                            packet.Data);
                    break;

                case PacketType.c_Dump:
                case PacketType.c_Peel:
                case PacketType.c_Victory:
                    packetHandler = new GamePacketHandler(
                            packet.PacketType, 
                            packet.Sender, 
                            packet.Recipients,
                            packet.Data);
                    break;

                case PacketType.c_AcceptFriendRequest:
                case PacketType.c_RejectFriendRequest:
                case PacketType.c_SendFriendRequest:
                case PacketType.c_RemoveFriend:
                case PacketType.c_FindBuddyFromGlobalList:
                case PacketType.c_QueryIfAliasAvailable:
                case PacketType.c_UploadGameData:
                    packetHandler = new DataBasePacketHandler(
                            packet.PacketType,
                            packet.Sender,
                            packet.Recipients,
                            packet.Data);
                    break;

                case PacketType.c_StartingMatchmaking:
                case PacketType.c_StoppingMatchmaking:
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
