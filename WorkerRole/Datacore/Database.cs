using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace WorkerRole.Datacore
{
    public class Database
    {
        private const string ConnectionString = "Server=tcp:blkp55bbj6.database.windows.net,1433;Database=ozwego-db;user ID=jonathanyeung@blkp55bbj6;Password=Jy121242121!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        private SqlConnection _connection;
        private OzwegoDataClassesDataContext _db;

        private static Database _instance;

        public static Database GetInstance()
        {
            return _instance ?? (_instance = new Database());
        }

        private Database()
        {
            _connection = new SqlConnection(ConnectionString);
            _db = new OzwegoDataClassesDataContext(_connection);
        }


        ~Database()
        {
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
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
            IQueryable<user> userQuery =
                from u in _db.users
                where u.email.StartsWith(email)
                select u;

            return userQuery.ToList();
        }


        public user GetUserByEmail(string email)
        {
            IQueryable<user> userQuery =
                from u in _db.users
                where u.email == email
                select u;

            return userQuery.FirstOrDefault();
        }


        public List<user> GetMatchingUsersByAlias(string alias)
        {
            IQueryable<user> userQuery =
                from u in _db.users
                where u.alias.StartsWith(alias)
                select u;

            return userQuery.ToList();
        }


        public user GetUserByAlias(string alias)
        {
            IQueryable<user> userQuery =
                from u in _db.users
                where u.alias == alias
                select u;

            return userQuery.FirstOrDefault();
        }


        public void AddNewUser(string newEmail, string newAlias)
        {
            var newUser = new user
                {
                    email = newEmail,
                    alias = newAlias,
                    creation_time = DateTime.UtcNow,
                    last_seen_time = DateTime.UtcNow
                };

            _db.users.InsertOnSubmit(newUser);

            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format(
                        "Exception in Database.AddNewUser!\n Exception: {0} \n Callstack: {1}",
                        e.Message,
                        e.StackTrace));
            }
        }

        /// <summary>
        /// Test Hook Method for Unit Tests.  Not to be used IRL.
        /// </summary>
        public void RemoveUser(string emailAddr)
        {
            var _user = GetUserFromEmailAddress(emailAddr);

            if (_user != null)
            {
                IQueryable<user> userQuery =
                    from u in _db.users
                    where (u.email == emailAddr)
                    select u;

                foreach (var user in userQuery)
                {
                    RemovePendingFriendRequests(user);
                    RemoveAllFriends(user);
                    _db.users.DeleteOnSubmit(user);
                }

                try
                {
                    _db.SubmitChanges();
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

        #endregion


        #region friendRequest

        public List<user> GetPendingFriendRequests(string username)
        {
            var curUser = GetUserFromEmailAddress(username);
            var userList = new List<user>();

            IQueryable<friendRequest> requestQuery =
                from request in _db.friendRequests
                where request.to_user == curUser.ID
                select request;

            if (null == requestQuery.FirstOrDefault())
            {
                return null;
            }

            foreach (var frdRequest in requestQuery)
            {
                //
                // Resharper's suggestion here was that frdRequest needs to be copied to a local variable.
                //

                friendRequest request = frdRequest;

                IQueryable<user> userQuery =
                    from u in _db.users
                    where u.ID == request.from_user
                    select u;

                userList.Add(userQuery.First());
            }

            return userList;
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


        public void SendFriendRequest(string fromUser, string toUser)
        {
            var _fromUser = GetUserFromEmailAddress(fromUser);
            var _toUser = GetUserFromEmailAddress(toUser);

            if ((_fromUser != null) &&
               (_toUser != null))
            {
                var request = new friendRequest
                    {
                        from_user = _fromUser.ID,
                        to_user = _toUser.ID,
                        creation_time = DateTime.UtcNow
                    };

                _db.friendRequests.InsertOnSubmit(request);

                try
                {
                    _db.SubmitChanges();
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
                var friendship = new friendship 
                {
                    user1 = _user1.ID,
                    user2 = _user2.ID,
                    creation_time = DateTime.UtcNow
                };

                _db.friendships.InsertOnSubmit(friendship);

                try
                {
                    _db.SubmitChanges();
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


        public void RemoveFriendship(string user1, string user2)
        {
            var _user1 = GetUserFromEmailAddress(user1);
            var _user2 = GetUserFromEmailAddress(user2);

            if ((_user1 != null) &&
                (_user2 != null))
            {
                IQueryable<friendship> friendshipQuery =
                    from frd in _db.friendships
                    where (frd.user1 == _user1.ID && frd.user2 == _user2.ID) || (frd.user1 == _user2.ID && frd.user2 == _user1.ID)
                    select frd;

                foreach (var frd in friendshipQuery)
                {
                    _db.friendships.DeleteOnSubmit(frd);
                }

                try
                {
                    _db.SubmitChanges();
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


        public List<user> GetFriends(string userName)
        {
            var userList = new List<user>();
            var _user = GetUserFromEmailAddress(userName);

            IQueryable<int> friendshipQueryOne =
                from frd in _db.friendships
                where frd.user1 == _user.ID
                select frd.user2;

            IQueryable<int> friendshipQueryTwo =
                from frd in _db.friendships
                where frd.user2 == _user.ID
                select frd.user1;
            
            // ToDo: Re-evaluate query performance here.
            foreach (var frdId in friendshipQueryOne)
            {
                var id = frdId;
                IQueryable<user> userQuery =
                    from u in _db.users
                    where u.ID == id
                    select u;

                userList.Add(userQuery.FirstOrDefault());
            }

            foreach (var frdId in friendshipQueryTwo)
            {
                var id = frdId;
                IQueryable<user> userQuery =
                    from u in _db.users
                    where u.ID == id
                    select u;

                userList.Add(userQuery.FirstOrDefault());
            }

            if (userList.Count == 0)
            {
                return null;
            }

            return userList;
        }

        #endregion


        #region Helper Methods

        private user GetUserFromEmailAddress(string emailAddress)
        {
            IQueryable<user> userQuery =
                from u in _db.users
                where u.email == emailAddress
                select u;

            if (userQuery.Count() == 1)
            {
                return userQuery.First();
            }

            return null;
        }


        private void RemoveAllFriends(user user)
        {
            IQueryable<friendship> friendshipQuery =
                from frd in _db.friendships
                where (frd.user1 == user.ID || frd.user2 == user.ID)
                select frd;

            foreach (var frd in friendshipQuery)
            {
                _db.friendships.DeleteOnSubmit(frd);
            }

            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format(
                        "Exception in Database.RemoveAllFriends!\n Exception: {0} \n Callstack: {1}",
                        e.Message,
                        e.StackTrace));
            }
        }


        private void RemovePendingFriendRequests(user user)
        {
            IQueryable<friendRequest> frdReqQuery =
                from frdReq in _db.friendRequests
                where (frdReq.from_user == user.ID || frdReq.to_user == user.ID)
                select frdReq;

            foreach (var frdReq in frdReqQuery)
            {
                _db.friendRequests.DeleteOnSubmit(frdReq);
            }

            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format(
                        "Exception in Database.RemovePendingFriendRequests!\n Exception: {0} \n Callstack: {1}",
                        e.Message,
                        e.StackTrace));
            }
        }


        private void RemoveFriendRequest(string fromUser, string toUser)
        {
            var _fromUser = GetUserFromEmailAddress(fromUser);
            var _toUser = GetUserFromEmailAddress(toUser);

            if ((_fromUser != null) &&
                (_toUser != null))
            {
                IQueryable<friendRequest> requestQuery =
                    from request in _db.friendRequests
                    where (request.from_user == _fromUser.ID) && (request.to_user == _toUser.ID)
                    select request;

                foreach (var request in requestQuery)
                {
                    _db.friendRequests.DeleteOnSubmit(request);
                }

                try
                {
                    _db.SubmitChanges();
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

        #endregion

    }
}
