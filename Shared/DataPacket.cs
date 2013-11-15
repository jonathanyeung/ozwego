using System;
using System.Collections.Generic;
using Ozwego.BuddyManagement;
using Ozwego.Storage;
using WorkerRole.DataTypes;

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
        ClientReadyForGameStart         = 6,          // Start the actual round

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
        ClientGetGameHistory            = 21,

        ClientHeartBeat                 = 22,

        ClientMaxValue                  = 23,


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
        ServerBeginGameInitialization   = 112,       // On receiving this, the client is supposed to move to the Game UI and initialize, but the game should NOT start.

        ServerFriendList                = 113,       // This contains the complete list of friends, both online and offline.
        ServerFriendRequests            = 114,
        ServerFriendRequestAccepted     = 115,
        ServerFriendSearchResults       = 116,
        ServerUserStats                 = 117,
        ServerRemoveFriend              = 118,

        ServerMatchmakingGameFound      = 119,
        ServerMatchmakingGameNotFound   = 120,

        ServerIsAliasAvailable          = 121,
        ServerUserGameHistory           = 122,

        ServerMaxValue                  = 123
    }

    public static class DataPacket
    {
        public static readonly Dictionary<PacketType, Type> PacketTypeMap = new Dictionary<PacketType, Type>
            {
                //
                // Client Message Types
                //
                //ToDo: Scrub through all of these typeof's to make sure that they are correct.
                {PacketType.ClientLogIn,                        typeof (Friend)},
                {PacketType.ClientLogOut,                       typeof (Friend)},
                {PacketType.ClientJoinRoom,                     typeof (Friend)}, //ToDo: It makes sense to change this to null.
                {PacketType.ClientLeaveRoom,                    null},
                {PacketType.ClientInitiateGame,                 null},
                {PacketType.ClientReadyForGameStart,            null},
                {PacketType.ClientDump,                         null},
                {PacketType.ClientPeel,                         null},
                {PacketType.ClientVictory,                      null},
                {PacketType.ClientChat,                         typeof (ChatMessage)},
                {PacketType.ClientGetFriendList,                null},
                {PacketType.ClientSendFriendRequest,            typeof (Friend)},
                {PacketType.ClientAcceptFriendRequest,          typeof (Friend)},
                {PacketType.ClientRejectFriendRequest,          typeof (Friend)},
                {PacketType.ClientRemoveFriend,                 typeof (Friend)},
                {PacketType.ClientFindBuddyFromGlobalList,      typeof (string)},
                {PacketType.ClientStartingMatchmaking,          null},
                {PacketType.ClientStoppingMatchmaking,          null},
                {PacketType.ClientQueryIfAliasAvailable,        typeof (string)},
                {PacketType.ClientUploadGameData,               typeof (GameData)},
                {PacketType.ClientGetGameHistory,               null},
                {PacketType.ClientHeartBeat,                    null},


                //
                // Server Message Types
                //

                {PacketType.UserLoggedOut,                      typeof (Friend)},
                {PacketType.UserLoggedIn,                       typeof (Friend)},
                {PacketType.UserJoinedRoom,                     typeof (Friend)},
                {PacketType.UserLeftRoom,                       typeof (Friend)},
                {PacketType.HostTransfer,                       typeof (Friend)},
                {PacketType.ServerPeel,                         typeof (Friend)}, //ToDo: If the server holds tile information, then change this to something else
                {PacketType.ServerDump,                         typeof (Friend)}, //ToDo: If the server holds tile information, then change this to something else
                {PacketType.ServerGameStart,                    null},
                {PacketType.ServerGameOver,                     typeof (Friend)},
                {PacketType.ServerChat,                         typeof (ChatMessage)},
                {PacketType.ServerOnlineFriendList,             typeof (FriendList)},
                {PacketType.ServerRoomList,                     typeof (FriendList)},
                {PacketType.ServerBeginGameInitialization,      null},
                {PacketType.ServerFriendList,                   typeof (FriendList)},
                {PacketType.ServerFriendRequests,               typeof (FriendList)},
                {PacketType.ServerFriendRequestAccepted,        typeof (Friend)},
                {PacketType.ServerFriendSearchResults,          typeof (FriendList)},
                {PacketType.ServerUserStats,                    typeof (Friend)},
                {PacketType.ServerRemoveFriend,                 typeof (Friend)},
                {PacketType.ServerMatchmakingGameFound,         null},
                {PacketType.ServerMatchmakingGameNotFound,      null},
                {PacketType.ServerIsAliasAvailable,             typeof (string)}
            };
    }
}
