using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ozwego.Shared;
using System;
using System.Diagnostics;
using System.Text;
using WorkerRole;

namespace ServerUnitTests
{
    [TestClass]
    public class MessageReceiverTests
    {
        private ClientManager clientManager;
        Client validTestClient;
        private const int k_clientCount = 5;

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


        #region LogIn Tests

        [TestMethod]
        public void InvalidUserLogIn()
        {
            Client client = clientManager.GetClientFromEmailAddress("TestUser1");

            var receiver = MessageReceiver.GetInstance();

            var messageBuffer = CreateMessage(PacketType.LogIn, "InvalidString");
            receiver.HandleMessage(ref client, messageBuffer);
        }


        [TestMethod]
        public void NewUserLogIn()
        {
            Client client = clientManager.GetClientFromEmailAddress("TestUser1");

            var receiver = MessageReceiver.GetInstance();

            var messageBuffer = CreateMessage(PacketType.LogIn, @"sender=TestUser1&message=crapmessage");
            receiver.HandleMessage(ref client, messageBuffer);
        }


        [TestMethod]
        public void ExistingUserLogIn()
        {
            throw new NotImplementedException();
        }


        #endregion


        #region Stress Tests

        /// <summary>
        /// This test sends the server random packet types with invalid data.  This tests the 
        /// robustness of the server in handling bad data.
        /// </summary>
        [TestMethod]
        public void RandomInvalidPacketStressTest()
        {
            const int k_iterations = 500;

            Client client = clientManager.GetClientFromEmailAddress("TestUser1");
            var receiver = MessageReceiver.GetInstance();

            Array packetTypes = Enum.GetValues(typeof(PacketType));

            Random r = new Random();

            for (int i = 0; i < k_iterations; i++)
            {
                var index = r.Next(0, packetTypes.Length - 1);
                PacketType selectedPacketType = (PacketType)packetTypes.GetValue(index);

                Debug.WriteLine(String.Format("Sending a packet of type {0}", selectedPacketType.ToString()));
                var messageBuffer = CreateMessage(selectedPacketType, "InvalidString");
                receiver.HandleMessage(ref client, messageBuffer);
            }
        }


        /// <summary>
        /// This test sends the server random packet types with invalid data.  This tests the 
        /// robustness of the server in handling bad data.
        /// </summary>
        [TestMethod]
        public void RandomValidPacketStressTest()
        {
            const int k_iterations = 500;

            Client client = clientManager.GetClientFromEmailAddress("TestUser1");

            var receiver = MessageReceiver.GetInstance();

            Array packetTypes = Enum.GetValues(typeof(PacketType));

            Random r = new Random();

            for (int i = 0; i < k_iterations; i++)
            {
                var index = r.Next(0, packetTypes.Length - 1);

                PacketType selectedPacketType = (PacketType)packetTypes.GetValue(index);

                Debug.WriteLine(String.Format("Sending a packet of type {0}", selectedPacketType.ToString()));

                var messageBuffer = CreateMessage(selectedPacketType, "sender=TestUser1&message=validtestmessage");

                receiver.HandleMessage(ref client, messageBuffer);
            }
        }

        #endregion


        private byte[] CreateMessage(PacketType packetType, string messageString)
        {
            byte[] buffer = new byte[1000];

            buffer[0] = (byte)packetType;

            var stringArray = Encoding.UTF8.GetBytes(messageString);

            Array.Copy(stringArray, 0, buffer, 1, stringArray.Length);

            return buffer;
        }
    }
}
