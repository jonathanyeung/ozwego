using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Ozwego.Storage;
using Shared;

namespace WorkerRole.Datacore
{
    public class Database
    {
        private const string ConnectionString = "Server=tcp:blkp55bbj6.database.windows.net,1433;Database=ozwego-db;user ID=jonathanyeung@blkp55bbj6;Password=Jy121242121!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

        private static Database _instance;

        public static Database GetInstance()
        {
            return _instance ?? (_instance = new Database());
        }

        private Database()
        {
        }


        #region user

        /// <summary>
        /// Returns the list of users whose email addresses match the given sequence of characters
        /// passed in. I.E. passing in "jo" will return jon, john, jonathan, jo*.  This is
        /// intended to be used when the end user is searching for new friends in the database.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public List<user> GetMatchingUsersByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var userQuery =
                        from u in db.users
                        where u.email.StartsWith(email)
                        select u;

                    return userQuery.ToList();
                }
            }
        }


        public user GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var userQuery =
                        from u in db.users
                        where u.email == email
                        select u;

                    return userQuery.FirstOrDefault();
                }
            }
        }


        public List<user> GetMatchingUsersByAlias(string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return null;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var userQuery =
                        from u in db.users
                        where u.alias.StartsWith(alias)
                        select u;

                    return userQuery.ToList();
                }
            }
        }


        public user GetUserByAlias(string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return null;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var userQuery =
                        from u in db.users
                        where u.alias == alias
                        select u;

                    return userQuery.FirstOrDefault();
                }
            }
        }


        public void AddNewUser(string newEmail, string newAlias)
        {
            if (string.IsNullOrEmpty(newEmail))
            {
                return;
            }

            if (string.IsNullOrEmpty(newAlias))
            {
                return;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var newUser = new user
                        {
                            email = newEmail,
                            alias = newAlias,
                            creation_time = DateTime.UtcNow,
                            last_seen_time = DateTime.UtcNow
                        };

                    db.users.InsertOnSubmit(newUser);

                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(string.Format(
                                "Exception in Database.AddNewUser!\n Exception: {0} \n Callstack: {1}",
                                e.Message,
                                e.StackTrace));
                    }
                }
            }
        }

        /// <summary>
        /// Test Hook Method for Unit Tests.  Not to be used IRL.
        /// </summary>
        public void RemoveUser(string emailAddr)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var curUser = GetUserFromEmailAddress(emailAddr);

                    if (curUser != null)
                    {
                        IQueryable<user> userQuery =
                            from u in db.users
                            where (u.email == emailAddr)
                            select u;

                        foreach (var user in userQuery)
                        {
                            RemovePendingFriendRequests(user);
                            RemoveAllFriends(user);
                            db.users.DeleteOnSubmit(user);
                        }

                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(string.Format(
                                    "Exception in Database.RemoveUser!\n Exception: {0} \n Callstack: {1}",
                                    e.Message,
                                    e.StackTrace));
                        }
                    }
                }
            }
        }

        #endregion


        #region friendRequest

        public List<user> GetPendingFriendRequests(string username)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var curUser = GetUserFromEmailAddress(username);

                    if (null == curUser)
                    {
                        return null;
                    }

                    IQueryable<friendRequest> requestQuery =
                        from request in db.friendRequests
                        where request.to_user == curUser.ID
                        select request;

                    if (null == requestQuery.FirstOrDefault())
                    {
                        return null;
                    }

                    return requestQuery.Select(request => 
                        (
                            from u in db.users
                            where u.ID == request.from_user
                            select u)
                        )
                        .Select(userQuery => userQuery.First()).ToList();
                }
            }
        }


        public void AcceptFriendRequest(string fromUser, string toUser)
        {
            CreateFriendship(fromUser, toUser);
            RemoveFriendRequest(fromUser, toUser);
        }


        public void RejectFriendRequest(string fromUser, string toUser)
        {
            RemoveFriendRequest(fromUser, toUser);
        }


        public void SendFriendRequest(string fromUserString, string toUserString)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var fromUser = GetUserFromEmailAddress(fromUserString);
                    var toUser = GetUserFromEmailAddress(toUserString);

                    if ((fromUser != null) &&
                       (toUser != null))
                    {
                        var request = new friendRequest
                            {
                                from_user = fromUser.ID,
                                to_user = toUser.ID,
                                creation_time = DateTime.UtcNow
                            };

                        db.friendRequests.InsertOnSubmit(request);

                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(string.Format(
                                    "Exception in Database.SendFriendRequest!\n Exception: {0} \n Callstack: {1}",
                                    e.Message, 
                                    e.StackTrace));
                        }
                    }
                }
            }
        }

        #endregion


        #region friendship

        //
        // This method is private because friendships can only be created when a row in the
        // friendRequest table has been modified.
        //
        private void CreateFriendship(string user1, string user2)
        {
            var _user1 = GetUserFromEmailAddress(user1);
            var _user2 = GetUserFromEmailAddress(user2);

            if ((_user1 != null) &&
                (_user2 != null))
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    using (var db = new OzwegoDataClassesDataContext(connection))
                    {
                        var friendship = new friendship
                        {
                            user1 = _user1.ID,
                            user2 = _user2.ID,
                            creation_time = DateTime.UtcNow
                        };


                        db.friendships.InsertOnSubmit(friendship);

                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(string.Format(
                                    "Exception in Database.CreateFriendship!\n Exception: {0} \n Callstack: {1}",
                                    e.Message,
                                    e.StackTrace));
                        }
                    }
                }
            }
        }


        public void RemoveFriendship(string user1, string user2)
        {
            var _user1 = GetUserFromEmailAddress(user1);
            var _user2 = GetUserFromEmailAddress(user2);

            if ((_user1 != null) &&
                (_user2 != null))
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    using (var db = new OzwegoDataClassesDataContext(connection))
                    {
                        IQueryable<friendship> friendshipQuery =
                            from frd in db.friendships
                            where (frd.user1 == _user1.ID && frd.user2 == _user2.ID) || (frd.user1 == _user2.ID && frd.user2 == _user1.ID)
                            select frd;

                        foreach (var frd in friendshipQuery)
                        {
                            db.friendships.DeleteOnSubmit(frd);
                        }

                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(string.Format(
                                    "Exception in Database.RemoveFriendship!\n Exception: {0} \n Callstack: {1}",
                                    e.Message,
                                    e.StackTrace));
                        }
                    }
                }
            }
        }


        public List<user> GetFriends(string userName)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var curUser = GetUserFromEmailAddress(userName);

                    if (null == curUser)
                    {
                        return null;
                    }

                    var friendshipQueryOne =
                        from frd in db.friendships
                        where frd.user1 == curUser.ID
                        select frd.user2;

                    var friendshipQueryTwo =
                        from frd in db.friendships
                        where frd.user2 == curUser.ID
                        select frd.user1;

                    var userList = friendshipQueryOne.Select(id => 
                        (
                            from u in db.users
                            where u.ID == id
                            select u)
                        )
                        .Select(userQuery => userQuery.FirstOrDefault()).ToList();

                    userList.AddRange(friendshipQueryTwo.Select(id => 
                        (
                            from u in db.users
                            where u.ID == id
                            select u)
                        ).Select(userQuery => userQuery.FirstOrDefault()));

                    return userList.Count == 0 ? null : userList;
                }
            }
        }

        #endregion


        #region GameData


        public List<user_game> GetUserGameHistory(user user)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var userQuery =
                        from g in db.user_games
                        where g.userID == user.ID
                        select g;

                    return userQuery.ToList();
                }
            }
        }

        public void AddNewGameData(GameData gameData)
        {
            if (null == gameData)
            {
                Trace.WriteLine(string.Format(
                    "Invalid Game Data in Database.AddNewGameData!\n GameData is null."));

                return;
            }

            var winnerUser = GetUserFromEmailAddress(gameData.Winner);

            if (null == winnerUser)
            {
                Trace.WriteLine(string.Format(
                    "Invalid Game Data in Database.AddNewGameData!\n Winner {0} not recognized.",
                    gameData.Winner));

                return;
            }

            foreach (var kvp in gameData.PlayerDictionary)
            {
                var player = GetUserFromEmailAddress(kvp.Key);

                if (null == player)
                {
                    Trace.WriteLine(string.Format(
                        "Invalid Game Data in Database.AddNewGameData!\n Player {0} not recognized.",
                        kvp.Key));

                    return;
                }
            }

            if (gameData.GameDuration < 0)
            {
                Trace.WriteLine(string.Format(
                    "Invalid Game Data in Database.AddNewGameData!\n GameDuration of value {0} is invalid.",
                    gameData.GameDuration));

                return;
            }


            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {

                    var newData = new game
                        {
                            winner = winnerUser.ID,
                            gameStartTime = gameData.GameStartTime,
                            gameDuration = gameData.GameDuration
                        };

                    // ToDo: newData.gameDataFile = ??


                    db.games.InsertOnSubmit(newData);

                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(string.Format(
                                "Exception in Database.AddNewGameData!\n Exception: {0} \n Callstack: {1}",
                                e.Message,
                                e.StackTrace));
                    }

                    var gameInstance = GetGameID(GetUserFromEmailAddress(gameData.Winner).ID, gameData.GameStartTime);

                    if (gameInstance == null)
                    {
                        return;
                    }

                    foreach (var kvp in gameData.PlayerDictionary)
                    {
                        var userGame = new user_game();
                        userGame.avgTimeBetweenDumps = kvp.Value.AvgTimeBetweenDumps;
                        userGame.avgTimeBetweenPeels = kvp.Value.AvgTimeBetweenPeels;
                        userGame.isWinner = kvp.Value.IsWinner;
                        userGame.numberOfDumps = kvp.Value.NumberOfDumps;
                        userGame.numberOfPeels = kvp.Value.NumberOfPeels;
                        userGame.performedFirstPeel = kvp.Value.PerformedFirstPeel;
                        userGame.userID = GetUserFromEmailAddress(kvp.Key).ID;
                        userGame.gameID = gameInstance.gameID;

                        db.user_games.InsertOnSubmit(userGame);
                    }

                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(string.Format(
                                "Exception in Database.AddNewGameData!\n Exception: {0} \n Callstack: {1}",
                                e.Message,
                                e.StackTrace));
                    }
                }
            }
        }


        private game GetGameID(int winnerId, DateTime gameStartTime)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var userQuery =
                        from u in db.games
                        where ((u.winner == winnerId) && (u.gameStartTime == gameStartTime))
                        select u;

                    return userQuery.FirstOrDefault();
                }
            }
        }

        #endregion


        #region Helper Methods

        private user GetUserFromEmailAddress(string emailAddress)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var userQuery =
                        from u in db.users
                        where u.email == emailAddress
                        select u;

                    return userQuery.Count() == 1 ? userQuery.First() : null;
                }
            }
        }


        private void RemoveAllFriends(user user)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var friendshipQuery =
                        from frd in db.friendships
                        where (frd.user1 == user.ID || frd.user2 == user.ID)
                        select frd;

                    foreach (var frd in friendshipQuery)
                    {
                        db.friendships.DeleteOnSubmit(frd);
                    }

                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(string.Format(
                                "Exception in Database.RemoveAllFriends!\n Exception: {0} \n Callstack: {1}",
                                e.Message,
                                e.StackTrace));
                    }
                }
            }
        }


        private void RemovePendingFriendRequests(user user)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    var frdReqQuery =
                        from frdReq in db.friendRequests
                        where (frdReq.from_user == user.ID || frdReq.to_user == user.ID)
                        select frdReq;

                    foreach (var frdReq in frdReqQuery)
                    {
                        db.friendRequests.DeleteOnSubmit(frdReq);
                    }

                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(string.Format(
                                "Exception in Database.RemovePendingFriendRequests!\n Exception: {0} \n Callstack: {1}",
                                e.Message,
                                e.StackTrace));
                    }
                }
            }
        }


        private void RemoveFriendRequest(string fromUser, string toUser)
        {
            var _fromUser = GetUserFromEmailAddress(fromUser);
            var _toUser = GetUserFromEmailAddress(toUser);

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var db = new OzwegoDataClassesDataContext(connection))
                {
                    if ((_fromUser != null) &&
                        (_toUser != null))
                    {
                        var requestQuery =
                            from request in db.friendRequests
                            where (request.from_user == _fromUser.ID) && (request.to_user == _toUser.ID)
                            select request;

                        foreach (var request in requestQuery)
                        {
                            db.friendRequests.DeleteOnSubmit(request);
                        }

                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(string.Format(
                                    "Exception in Database.RemoveFriendRequest!\n Exception: {0} \n Callstack: {1}",
                                    e.Message,
                                    e.StackTrace));
                        }
                    }
                }
            }
        }

        #endregion

    }
}
