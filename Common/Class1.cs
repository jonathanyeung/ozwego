using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum PacketType : byte
    {
        //
        // Client to Server
        //

        LogIn = 1,
        LogOut = 2,
        JoinRoom = 3,
        LeaveRoom = 4,

        // Sendable only by room host:
        EnterGame = 5,          // Move everyone in the room to the GameUI page
        StartGame = 6,          // Start the actual round

        // In game packet types:
        ClientDump = 7,
        ClientPeel = 8,
        ClientVictory = 9,
        ClientChat = 10,
        ClientMaxValue = 11,


        //
        // Server to Client
        //

        UserLoggedOut = 100,
        UserLoggedIn = 101,
        UserJoinedRoom = 102,
        UserLeftRoom = 103,
        HostTransfer = 104,       // You became the new room host.
        ServerPeel = 105,
        ServerDump = 106,
        ServerGameOver = 107,
        ServerChat = 108,
        ServerMaxValue = 109
    }

    public class Class1
    {
    }
}
