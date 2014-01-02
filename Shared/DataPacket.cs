using System;
using System.Collections.Generic;
using Ozwego.BuddyManagement;
using Ozwego.Storage;
using WorkerRole.DataTypes;

namespace Shared
{
    /// <summary>
    /// Represents where in the app an online client currently is.  A client is only joinable when
    /// they are in the InLobby or Idle state.
    /// </summary>
    public enum ClientUserState : byte
    {
        InLobby,
        InGame,
        WaitingForMatchmaking,
        Idle
    }


    /// <summary>
    /// Notation: 
    ///     c_ indicates packet is sent by the client
    ///     s_ indicates packet is sent by the server
    /// </summary>
    public enum PacketType : byte
    {
        //
        // Client to Server
        //

        c_LogIn                     = 1,
        c_LogOut                    = 2,
        c_JoinRoom                  = 3,
        c_LeaveRoom                 = 4,

        // Sendable only by room host:
        c_InitiateGame              = 5,          // Move everyone in the room to the GameUI page
        c_ReadyForGameStart         = 6,          // Start the actual round

        // In game packet types:
        c_Dump                      = 7,
        c_Peel                      = 8,
        c_Victory                   = 9,
        c_Chat                      = 10,

        c_GetFriendList             = 11,           // ToDo: Is this type necessary, it might be sufficient to just send this from the server on log in.
        c_SendFriendRequest         = 12,
        c_AcceptFriendRequest       = 13,
        c_RejectFriendRequest       = 14,
        c_RemoveFriend              = 15,
        c_FindBuddyFromGlobalList   = 16,

        c_StartingMatchmaking       = 17,
        c_StoppingMatchmaking       = 18,

        c_QueryIfAliasAvailable     = 19,

        c_UploadGameData            = 20,
        c_GetGameHistory            = 21,

        c_HeartBeat                 = 22,

        c_UserStateChange           = 23,

        c_MaxValue                  = 24,


        //
        // Server to Client
        //

        s_UserLoggedOut             = 100,
        s_UserLoggedIn              = 101,

        s_UserJoinedRoom            = 102,
        s_UserLeftRoom              = 103,
        s_HostTransfer              = 104,       // You became the new room host.

        s_Peel                      = 105,
        s_Dump                      = 106,
        s_GameStart                 = 107,
        s_GameOver                  = 108,
        s_Chat                      = 109,

        s_OnlineFriendList          = 110,       // User just signed in, send the list of friends currently online.
        s_RoomList                  = 111,       // User just joined a room, send the room members list.
        s_BeginGameInitialization   = 112,       // On receiving this, the client is supposed to move to the Game UI and initialize, but the game should NOT start.

        s_FriendList                = 113,       // This contains the complete list of friends, both online and offline.
        s_FriendRequests            = 114,
        s_FriendRequestAccepted     = 115,
        s_FriendSearchResults       = 116,
        s_UserStats                 = 117,
        s_RemoveFriend              = 118,

        s_MatchmakingGameFound      = 119,
        s_MatchmakingGameNotFound   = 120,

        s_IsAliasAvailable          = 121,
        s_UserGameHistory           = 122,

        s_UserStateChange           = 123,

        s_MaxValue                  = 124
    }

    public static class DataPacket
    {
        public static readonly Dictionary<PacketType, Type> PacketTypeMap = new Dictionary<PacketType, Type>
            {
                //
                // Client Message Types
                //
                //ToDo: Scrub through all of these typeof's to make sure that they are correct.
                {PacketType.c_LogIn,                        typeof (Friend)},
                {PacketType.c_LogOut,                       typeof (Friend)},
                {PacketType.c_JoinRoom,                     typeof (Friend)}, //ToDo: It makes sense to change this to null.
                {PacketType.c_LeaveRoom,                    null},
                {PacketType.c_InitiateGame,                 null},
                {PacketType.c_ReadyForGameStart,            null},
                {PacketType.c_Dump,                         null},
                {PacketType.c_Peel,                         null},
                {PacketType.c_Victory,                      null},
                {PacketType.c_Chat,                         typeof (ChatMessage)},
                {PacketType.c_GetFriendList,                null},
                {PacketType.c_SendFriendRequest,            typeof (Friend)},
                {PacketType.c_AcceptFriendRequest,          typeof (Friend)},
                {PacketType.c_RejectFriendRequest,          typeof (Friend)},
                {PacketType.c_RemoveFriend,                 typeof (Friend)},
                {PacketType.c_FindBuddyFromGlobalList,      typeof (string)},
                {PacketType.c_StartingMatchmaking,          null},
                {PacketType.c_StoppingMatchmaking,          null},
                {PacketType.c_QueryIfAliasAvailable,        typeof (string)},
                {PacketType.c_UploadGameData,               typeof (GameData)},
                {PacketType.c_GetGameHistory,               null},
                {PacketType.c_HeartBeat,                    null},
                {PacketType.c_UserStateChange,              typeof(byte)},


                //
                // Server Message Types
                //

                {PacketType.s_UserLoggedOut,                typeof (Friend)},
                {PacketType.s_UserLoggedIn,                 typeof (Friend)},
                {PacketType.s_UserJoinedRoom,               typeof (Friend)},
                {PacketType.s_UserLeftRoom,                 typeof (Friend)},
                {PacketType.s_HostTransfer,                 typeof (Friend)},
                {PacketType.s_Peel,                         typeof (Friend)}, //ToDo: If the server holds tile information, then change this to something else
                {PacketType.s_Dump,                         typeof (Friend)}, //ToDo: If the server holds tile information, then change this to something else
                {PacketType.s_GameStart,                    null},
                {PacketType.s_GameOver,                     typeof (Friend)},
                {PacketType.s_Chat,                         typeof (ChatMessage)},
                {PacketType.s_OnlineFriendList,             typeof (FriendList)},
                {PacketType.s_RoomList,                     typeof (FriendList)},
                {PacketType.s_BeginGameInitialization,      null},
                {PacketType.s_FriendList,                   typeof (FriendList)},
                {PacketType.s_FriendRequests,               typeof (FriendList)},
                {PacketType.s_FriendRequestAccepted,        typeof (Friend)},
                {PacketType.s_FriendSearchResults,          typeof (FriendList)},
                {PacketType.s_UserStats,                    typeof (Friend)},
                {PacketType.s_RemoveFriend,                 typeof (Friend)},
                {PacketType.s_MatchmakingGameFound,         null},
                {PacketType.s_MatchmakingGameNotFound,      null},
                {PacketType.s_IsAliasAvailable,             typeof (string)},
                {PacketType.s_UserGameHistory,              typeof (GameDataList)}, //ToDo: Wrong DataType
                {PacketType.s_UserStateChange,              typeof (UserStateChange)}
            };
    }
}
