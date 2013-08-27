using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkerRole;
using WorkerRole.Datacore;

namespace ServerUnitTests
{
    /// <summary>
    /// Tests for the database.  Note: An active internet connection to the database is required 
    /// for these tests to work.
    /// </summary>
    [TestClass]
    public class DatabaseTests
    {
        private Database _db = Database.GetInstance();

        private const string TestUserOneEmail = "TestUser1@testaccount.com";
        private const string TestUserTwoEmail = "TestUserTwo@testaccount.com";
        private const string TestUserThreeEmail = "TestUserThree@testaccount.com";
        private const string InvalidEmail = "UnusedInvalidEmail@nonexistant.com";


        private List<user> testUserList = new List<user>()
            {
                new user() {alias = "TestUser1", email = TestUserOneEmail},
                new user() {alias = "TestUser2", email = TestUserTwoEmail},
                new user() {alias = "TestUser3", email = TestUserThreeEmail}
            };

        [TestInitialize()]
        public void Initialize()
        {
            Debug.WriteLine("DatabaseTests.Initialize()");

            foreach (user u in testUserList)
            {
                _db.AddNewUser(u.email, u.alias);
            }
        }

        [TestCleanup()]
        public void Cleanup()
        {
            Debug.WriteLine("DatabaseTests.Cleanup()");
            foreach (user u in testUserList)
            {
                _db.RemoveUser(u.email);
            }
        }


        #region User Tests

        [TestMethod]
        public void GetUserByEmail()
        {
            var user = _db.GetUserByEmail(TestUserOneEmail);
            Assert.IsTrue(user.email == TestUserOneEmail);

            user = _db.GetUserByEmail(InvalidEmail);
            Assert.IsNull(user);
        }

        [TestMethod]
        public void GetMatchingUsersByEmail()
        {

        }

        [TestMethod]
        public void GetUserByAlias()
        {

        }

        [TestMethod]
        public void GetMatchingUsersByAlias()
        {

        }

        #endregion


        #region Friendship Tests

        [TestMethod]
        public void GetPendingFriendRequests()
        {

        }

        [TestMethod]
        public void AcceptFriendRequests()
        {

        }

        [TestMethod]
        public void RejectFriendRequest()
        {

        }

        [TestMethod]
        public void SendFriendRequest()
        {

        }

        [TestMethod]
        public void RemoveFriendship()
        {

        }

        [TestMethod]
        public void GetFriends()
        {

        }

        #endregion

    }
}
