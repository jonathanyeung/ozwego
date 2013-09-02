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
        private readonly Database _db = Database.GetInstance();

        private const string TestUserOneEmail = "TestUser1@testaccount.com";
        private const string TestUserTwoEmail = "TestUserTwo@testaccount.com";
        private const string TestUserThreeEmail = "TestUserThree@testaccount.com";
        private const string InvalidEmail = "UnusedInvalidEmail@nonexistant.com";

        private const string TestUserOneAlias = "TestUserOne";
        private const string TestUserTwoAlias = "TestUserTwo";
        private const string TestUserThreeAlias = "TestUserThree";
        private const string InvalidAlias = "UnusedInvalidEmail";


        private List<user> testUserList = new List<user>()
            {
                new user() {alias = TestUserOneAlias, email = TestUserOneEmail},
                new user() {alias = TestUserTwoAlias, email = TestUserTwoEmail},
                new user() {alias = TestUserThreeAlias, email = TestUserThreeEmail}
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
            var userList = _db.GetMatchingUsersByEmail("TestUserT");
            Assert.AreEqual(userList.Count, 2);
            if (((userList[0].email != TestUserTwoEmail) || (userList[1].email != TestUserThreeEmail)) &&
                ((userList[1].email != TestUserTwoEmail) || (userList[0].email != TestUserThreeEmail)))
            {
                Assert.Fail("GetMatchingUsersByEmail did not return the right users.");
            }
        }


        [TestMethod]
        public void GetUserByAlias()
        {
            var user = _db.GetUserByAlias(TestUserOneAlias);
            Assert.IsTrue(user.alias == TestUserOneAlias);

            user = _db.GetUserByAlias(InvalidAlias);
            Assert.IsNull(user);
        }


        [TestMethod]
        public void GetMatchingUsersByAlias()
        {
            var userList = _db.GetMatchingUsersByAlias("TestUserT");
            Assert.AreEqual(userList.Count, 2);
            if (((userList[0].alias != TestUserTwoAlias) || (userList[1].alias != TestUserThreeAlias)) &&
                ((userList[1].alias != TestUserTwoAlias) || (userList[0].alias != TestUserThreeAlias)))
            {
                Assert.Fail("GetMatchingUsersByAlias did not return the right users.");
            }
        }

        #endregion


        #region Friendship Tests

        [TestMethod]
        public void BasicFriendshipTest()
        {
            _db.SendFriendRequest(TestUserOneEmail, TestUserThreeEmail);
            _db.SendFriendRequest(TestUserTwoEmail, TestUserThreeEmail);


            //
            // Test GetPendingFriendRequests() method
            //

            var friendRequests = _db.GetPendingFriendRequests(TestUserOneEmail);
            Assert.IsNull(friendRequests);

            friendRequests = _db.GetPendingFriendRequests(TestUserTwoEmail);
            Assert.IsNull(friendRequests);

            friendRequests = _db.GetPendingFriendRequests(TestUserThreeEmail);
            Assert.AreEqual(friendRequests.Count, 2);

            var friendList = _db.GetFriends(TestUserThreeEmail);
            Assert.IsNull(friendList);


            //
            // Test accepting a friend request.
            //

            _db.AcceptFriendRequest(TestUserOneEmail, TestUserThreeEmail);

            friendRequests = _db.GetPendingFriendRequests(TestUserThreeEmail);
            Assert.AreEqual(friendRequests.Count, 1);

            friendList = _db.GetFriends(TestUserThreeEmail);
            Assert.AreEqual(friendList.Count, 1);

            friendList = _db.GetFriends(TestUserOneEmail);
            Assert.AreEqual(friendList.Count, 1);


            //
            // Test rejecting a friend request.
            //

            _db.RejectFriendRequest(TestUserTwoEmail, TestUserThreeEmail);

            friendRequests = _db.GetPendingFriendRequests(TestUserThreeEmail);
            Assert.IsNull(friendRequests);

            friendList = _db.GetFriends(TestUserThreeEmail);
            Assert.AreEqual(friendList.Count, 1);

            friendList = _db.GetFriends(TestUserTwoEmail);
            Assert.IsNull(friendList);


            //
            // Test Removing a friendship
            //

            _db.RemoveFriendship(TestUserOneEmail, TestUserThreeEmail);

            friendList = _db.GetFriends(TestUserThreeEmail);
            Assert.IsNull(friendList);

            friendList = _db.GetFriends(TestUserOneEmail);
            Assert.IsNull(friendList);

            _db.RemoveFriendship(TestUserOneEmail, TestUserThreeEmail);

        }

        #endregion

    }
}
