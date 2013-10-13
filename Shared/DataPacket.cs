using System;
using System.Collections.Generic;
using Ozwego.Storage;

namespace Shared
{
    public enum PacketType : byte
    {
        //
        // Client to Server
        //

        ClientLogIn                     = 1,
        ClientLogOut                    = 2,
        ClientJoinRoom                  = 3,
        ClientLeaveRoom                 = 4,

        // Sendable only by room host:
        ClientInitiateGame              = 5,          // Move everyone in the room to the GameUI page
        ClientStartGame                 = 6,          // Start the actual round

        // In game packet types:
        ClientDump                      = 7,
        ClientPeel                      = 8,
        ClientVictory                   = 9,
        ClientChat                      = 10,

        ClientGetFriendList             = 11,           // ToDo: Is this type necessary, it might be sufficient to just send this from the server on log in.
        ClientSendFriendRequest         = 12,
        ClientAcceptFriendRequest       = 13,
        ClientRejectFriendRequest       = 14,
        ClientRemoveFriend              = 15,
        ClientFindBuddyFromGlobalList   = 16,

        ClientStartingMatchmaking       = 17,
        ClientStoppingMatchmaking       = 18,

        ClientQueryIfAliasAvailable     = 19,

        ClientUploadGameData            = 20,

        ClientMaxValue                  = 21,


        //
        // Server to Client
        //

        UserLoggedOut                   = 100,
        UserLoggedIn                    = 101,

        UserJoinedRoom                  = 102,
        UserLeftRoom                    = 103,
        HostTransfer                    = 104,       // You became the new room host.

        ServerPeel                      = 105,
        ServerDump                      = 106,
        ServerGameStart                 = 107,
        ServerGameOver                  = 108,
        ServerChat                      = 109,

        ServerOnlineFriendList          = 110,       // User just signed in, send the list of friends currently online.
        ServerRoomList                  = 111,       // User just joined a room, send the room members list.
        ServerInitiateGame              = 112,       // On receiving this, the client is supposed to move to the Game UI, but the actual game has NOT started yet.

        ServerFriendList                = 113,       // This contains the complete list of friends, both online and offline.
        ServerFriendRequests            = 114,
        ServerFriendRequestAccepted     = 115,
        ServerFriendSearchResults       = 116,
        ServerUserStats                 = 117,
        ServerRemoveFriend              = 118,

        ServerMatchmakingGameFound      = 119,
        ServerMatchmakingGameNotFound   = 120,

        ServerIsAliasAvailable          = 121,

        ServerMaxValue                  = 122
    }

    public static class DataPacket
    {
        public static readonly Dictionary<PacketType, Type> PacketTypeMap = new Dictionary<PacketType, Type>
            {
                //
                // Client Message Types
                //
                //ToDo: Scrub through all of these typeof's to make sure that they are correct.
                {PacketType.ClientLogIn,                        typeof (string)},
                {PacketType.ClientLogOut,                       typeof (string)},
                {PacketType.ClientJoinRoom,                     typeof (string)},
                {PacketType.ClientLeaveRoom,                    typeof (string)},
                {PacketType.ClientInitiateGame,                 typeof (string)},
                {PacketType.ClientStartGame,                    typeof (string)},
                {PacketType.ClientDump,                         typeof (string)},
                {PacketType.ClientPeel,                         typeof (string)},
                {PacketType.ClientVictory,                      typeof (string)},
                {PacketType.ClientChat,                         typeof (string)},
                {PacketType.ClientGetFriendList,                typeof (string)},
                {PacketType.ClientSendFriendRequest,            typeof (string)},
                {PacketType.ClientAcceptFriendRequest,          typeof (string)},
                {PacketType.ClientRejectFriendRequest,          typeof (string)},
                {PacketType.ClientRemoveFriend,                 typeof (string)},
                {PacketType.ClientFindBuddyFromGlobalList,      typeof (string)},
                {PacketType.ClientStartingMatchmaking,          null},
                {PacketType.ClientStoppingMatchmaking,          null},
                {PacketType.ClientQueryIfAliasAvailable,        typeof (string)},
                {PacketType.ClientUploadGameData,               typeof (GameData)},


                //
                // Server Message Types
                //

                {PacketType.UserLoggedOut,                      typeof (string)},
                {PacketType.UserLoggedIn,                       typeof (string)},
                {PacketType.UserJoinedRoom,                     typeof (string)},
                {PacketType.UserLeftRoom,                       typeof (string)},
                {PacketType.HostTransfer,                       typeof (string)},
                {PacketType.ServerPeel,                         typeof (string)},
                {PacketType.ServerDump,                         typeof (string)},
                {PacketType.ServerGameStart,                    typeof (string)},
                {PacketType.ServerGameOver,                     typeof (string)},
                {PacketType.ServerChat,                         typeof (string)},
                {PacketType.ServerOnlineFriendList,             typeof (string)},
                {PacketType.ServerRoomList,                     typeof (string)},
                {PacketType.ServerInitiateGame,                 typeof (string)},
                {PacketType.ServerFriendList,                   typeof (string)},
                {PacketType.ServerFriendRequests,               typeof (string)},
                {PacketType.ServerFriendRequestAccepted,        typeof (string)},
                {PacketType.ServerFriendSearchResults,          typeof (string)},
                {PacketType.ServerUserStats,                    typeof (string)},
                {PacketType.ServerRemoveFriend,                 typeof (string)},
                {PacketType.ServerMatchmakingGameFound,         typeof (string)},
                {PacketType.ServerMatchmakingGameNotFound,      typeof (string)},
                {PacketType.ServerIsAliasAvailable,             typeof (string)}
            };
    }
}
