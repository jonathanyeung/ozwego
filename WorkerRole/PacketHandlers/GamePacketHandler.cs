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
                case PacketType.c_InitiateGame:

                    //
                    // Only allow the room host to start the game.
                    //

                    if (client.Room.GetHostAddress() == client.UserInfo.EmailAddress)
                    {
                        MessageSender.BroadcastMessage(client.Room.Members, PacketType.s_BeginGameInitialization, null);
                    }
                    break;

                case PacketType.c_Dump:
                    MessageSender.BroadcastMessage(client.Room.Members, PacketType.s_Dump, client.UserInfo, client);
                    break;

                case PacketType.c_Peel:
                    MessageSender.BroadcastMessage(client.Room.Members, PacketType.s_Peel, client.UserInfo, client);
                    break;

                case PacketType.c_Victory:
                    MessageSender.BroadcastMessage(client.Room.Members, PacketType.s_GameOver, client.UserInfo, client);
                    break;

                default:
                    Trace.WriteLine(string.Format("[GamePacketHandler.DoActions] - " +
                        "Invalid packet type for this PacketHandler = {0}", PacketType.ToString()));
                    break;
            }
        }
    }
}
