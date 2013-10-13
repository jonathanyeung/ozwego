using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

using Shared;
using WorkerRole.PacketHandlers;

namespace WorkerRole
{
    public static class PacketHandlerFactory
    {
        public static PacketHandler GetPacketHandler(object data)
        {
            PacketHandler packetHandler = null;
            PacketV1 packet;

            var bytes = (byte[]) data;

            using (var stream = new MemoryStream(bytes))
            {
                var ser = new XmlSerializer(typeof(PacketV1));

                packet = (PacketV1)ser.Deserialize(stream);
            }

            switch (packet.PacketType)
            {
                case PacketType.ClientLogIn:
                case PacketType.ClientLogOut:
                case PacketType.ClientJoinRoom:
                case PacketType.ClientLeaveRoom:
                case PacketType.ClientInitiateGame:
                case PacketType.ClientChat:
                    packetHandler = new RoomPacketHandler(
                            packet.PacketType, 
                            packet.Sender, 
                            packet.Recipients,
                            packet.Data);
                    break;

                case PacketType.ClientStartGame:
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
