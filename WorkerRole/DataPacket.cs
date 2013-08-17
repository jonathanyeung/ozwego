namespace WorkerRole
{
    public enum PacketType : byte
    {
        //
        // Client to Server
        //

        LogIn           = 1,
        LogOut          = 2,
        JoinRoom        = 3,
        LeaveRoom       = 4,

        // Sendable only by room host:
        InitiateGame    = 5,          // Move everyone in the room to the GameUI page
        StartGame       = 6,          // Start the actual round

        // In game packet types:
        ClientDump      = 7,
        ClientPeel      = 8,
        ClientVictory   = 9,
        ClientChat      = 10,
        ClientMaxValue  = 11,

        
        //
        // Server to Client
        //

        UserLoggedOut   = 100,
        UserLoggedIn    = 101,
        UserJoinedRoom  = 102,
        UserLeftRoom    = 103,
        HostTransfer    = 104,       // You became the new room host.
        ServerPeel      = 105,
        ServerDump      = 106,
        ServerGameStart = 107,
        ServerGameOver  = 108,
        ServerChat      = 109,
        ServerBuddyList = 110,      // User just signed in, send the global buddy list.
        ServerRoomList  = 111,      // User just joined a room, send the room members list.
        ServerInitiateGame = 112,   // On receiving this, the client is supposed to move to the Game UI, but the actual game has NOT started yet.
        ServerMaxValue  = 113,


        //
        // Server to Server
        //

        ForceDisconnect = 200

    }

    class DataPacket
    {
    }
}
