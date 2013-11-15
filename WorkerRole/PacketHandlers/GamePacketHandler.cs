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
            switch (PacketType)
            {
                case PacketType.ClientInitiateGame:

                    //
                    // Only allow the room host to start the game.
                    //

                    if (client.Room.GetHostAddress() == client.UserInfo.EmailAddress)
                    {
                        MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerBeginGameInitialization, null);
                    }
                    break;

                case PacketType.ClientDump:
                    MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerDump, client.UserInfo, client);
                    break;

                case PacketType.ClientPeel:
                    MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerPeel, client.UserInfo, client);
                    break;

                case PacketType.ClientVictory:
                    MessageSender.BroadcastMessage(client.Room.Members, PacketType.ServerGameOver, client.UserInfo, client);
                    break;

                default:
                    Trace.WriteLine(string.Format("[GamePacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
                    break;
            }
        }
    }
}
