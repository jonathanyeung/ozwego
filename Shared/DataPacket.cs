namespace Ozwego.Shared
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

        ClientGetFriendList = 11,           // ToDo: Is this type necessary, it might be sufficient to just send this from the server on log in.
        ClientSendFriendRequest = 12,
        ClientAcceptFriendRequest = 13,
        ClientRejectFriendRequest = 14,
        ClientRemoveFriend = 15,
        ClientFindBuddyFromGlobalList = 16,

        ClientStartingMatchmaking = 17,
        ClientStoppingMatchmaking = 18,

        ClientMaxValue  = 19,

        
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

        ServerOnlineFriendList = 110,       // User just signed in, send the list of friends currently online.
        ServerRoomList  = 111,              // User just joined a room, send the room members list.
        ServerInitiateGame = 112,           // On receiving this, the client is supposed to move to the Game UI, but the actual game has NOT started yet.

        ServerFriendList = 113,             // This contains the complete list of friends, both online and offline.
        ServerFriendRequests = 114,
        ServerFriendRequestAccepted = 115,
        ServerFriendSearchResults = 116,
        ServerUserStats = 117,
        ServerRemoveFriend = 118,

        ServerMaxValue  = 119
    }
}
