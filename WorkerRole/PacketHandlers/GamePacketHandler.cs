using System.Collections.Generic;
using System.Diagnostics;
using Shared;

namespace WorkerRole.PacketHandlers
{
    public class GamePacketHandler : PacketHandler
    {
        public GamePacketHandler(PacketType packetType, string sender, List<string> recipients, object data)
            : base(packetType, sender, recipients, data)
        {
        }

        public override void DoActions(ref Client client)
        {
            var messageSender = MessageSender.GetInstance();

            switch (PacketType)
            {
                case PacketType.ClientStartGame:

                    //
                    // Only allow the room host to start the game.
                    //

                    if (client.Room.Host.UserName == client.UserName)
                    {
                        messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerGameStart, "");
                    }
                    break;

                case PacketType.ClientDump:
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerDump, "", client);
                    break;

                case PacketType.ClientPeel:
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerPeel, "", client);
                    break;

                case PacketType.ClientVictory:
                    messageSender.BroadcastMessage(client.Room.Members, PacketType.ServerGameOver, "", client);
                    break;

                default:
                    Trace.WriteLine(string.Format("[GamePacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
                    break;
            }
        }
    }
}
