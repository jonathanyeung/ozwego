using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkerRole;
using WorkerRole.Matchmaking;
using System.Threading;

namespace ServerUnitTests
{
    [TestClass]
    public class MatchmakingTests
    {
        private ClientManager clientManager;

        Client validTestClient;
        private const int k_clientCount = 10;


        [TestInitialize()]
        public void Initialize()
        {
            //
            // Generate the client manager and add a number of clients to the server.
            //

            clientManager = ClientManager.GetInstance();

            for (int i = 0; i < k_clientCount; i++)
            {
                Client newClient = new Client(null);
                newClient.UserName = "TestUser" + i.ToString();

                clientManager.AddClient(newClient);
            }
        }


        //[TestCleanup()]
        //public void Cleanup()
        //{
        //    clientManager = ClientManager.GetInstance();
        //    var clientList = clientManager.GetClientList();


        //    foreach (Client c in clientList)
        //    {
        //        clientManager.RemoveClient(c);
        //    }
        //}


        /// <summary>
        /// This tests that the client will get dequeued from matchmaking if the client has 
        /// waited for longer than the maximum wait interval.
        /// </summary>
        [TestMethod]
        public void TestMatchmakingTimeOut()
        {
            Client client1 = clientManager.GetClientFromEmailAddress("TestUser1");
            Client client2 = clientManager.GetClientFromEmailAddress("TestUser2");
            Client client3 = clientManager.GetClientFromEmailAddress("TestUser3");
            Client client4 = clientManager.GetClientFromEmailAddress("TestUser4");

            var matchmaker = Matchmaker.GetInstance();

            matchmaker.JoinMatchmakingQueue(client1);

            Thread.Sleep(matchmaker.MaxClientWaitTime + 1000);

            matchmaker.JoinMatchmakingQueue(client2);
            matchmaker.JoinMatchmakingQueue(client3);
            matchmaker.JoinMatchmakingQueue(client4);

            var roomManager = RoomManager.GetInstance();

            var clientList = roomManager.GetRoomMembers(client1);

            Assert.AreEqual(clientList.Count, 1);
        }


        /// <summary>
        /// Test that no matchmaking games are made when there are not enough players in the queue.
        /// </summary>
        [TestMethod]
        public void TestNotEnoughPlayersInQueue()
        {
            Client client1 = clientManager.GetClientFromEmailAddress("TestUser1");
            Client client2 = clientManager.GetClientFromEmailAddress("TestUser2");
            Client client3 = clientManager.GetClientFromEmailAddress("TestUser3");

            var matchmaker = Matchmaker.GetInstance();

            matchmaker.JoinMatchmakingQueue(client1);
            matchmaker.JoinMatchmakingQueue(client2);
            matchmaker.JoinMatchmakingQueue(client3);

            Thread.Sleep(matchmaker.MaxClientWaitTime + 1000);

            var roomManager = RoomManager.GetInstance();

            var clientList = roomManager.GetRoomMembers(client1);
            Assert.AreEqual(clientList.Count, 1);

            clientList = roomManager.GetRoomMembers(client2);
            Assert.AreEqual(clientList.Count, 1);

            clientList = roomManager.GetRoomMembers(client3);
            Assert.AreEqual(clientList.Count, 1);
        }


        /// <summary>
        /// Tests the basic case of having 4 people queued, and having 1 matchmade game produced.
        /// </summary>
        [TestMethod]
        public void TestSingleMatchmadeGame()
        {
            Client client1 = clientManager.GetClientFromEmailAddress("TestUser1");
            Client client2 = clientManager.GetClientFromEmailAddress("TestUser2");
            Client client3 = clientManager.GetClientFromEmailAddress("TestUser3");
            Client client4 = clientManager.GetClientFromEmailAddress("TestUser4");

            var matchmaker = Matchmaker.GetInstance();

            matchmaker.JoinMatchmakingQueue(client1);
            matchmaker.JoinMatchmakingQueue(client2);
            matchmaker.JoinMatchmakingQueue(client3);
            matchmaker.JoinMatchmakingQueue(client4);

            // Slightly more than one interval.
            Thread.Sleep(11000);

            var roomManager = RoomManager.GetInstance();

            var clientList = roomManager.GetRoomMembers(client1);
            Assert.AreEqual(clientList.Count, 4);
        }


    }
}
