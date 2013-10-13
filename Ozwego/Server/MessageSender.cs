using System.IO;
using System.Xml.Serialization;
using Ozwego.BuddyManagement;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Windows.Storage.Streams;

namespace Ozwego.Server
{
    public class MessageSender
    {
        private static MessageSender _instance;


        /// <summary>
        /// Private Constructor
        /// </summary>
        private MessageSender()
        {
        }


        /// <summary>
        /// Public method to instantiate ServerMessageSender singleton.
        /// </summary>
        /// <returns></returns>
        public static MessageSender GetInstance()
        {
            return _instance ?? (_instance = new MessageSender());
        }


        public async Task SendMessage(PacketType packetType)
        {
            var recipientList = new List<Buddy>();
            await SendMessage(packetType, recipientList, ""); //ToDo: Change this from "" to null
        }


        public async Task SendMessage(PacketType packetType, string messageString)
        {
            var recipientList = new List<Buddy>();
            await SendMessage(packetType, recipientList, messageString);
        }


        public async Task SendMessage(PacketType packetType, byte[] buffer)
        {
            var recipientList = new List<Buddy>();
            await SendMessage(packetType, recipientList, buffer);
        }


        public async Task SendMessage(PacketType packetType, List<Buddy> recipientList)
        {
            foreach (var b in recipientList)
            {
                await SendMessage(packetType, "", b);
            }
        }


        public async Task SendMessage(PacketType packetType, string messageString, Buddy buddy)
        {
            var recipientList = new List<Buddy> {buddy};
            await SendMessage(packetType, recipientList, messageString);
        }


        private async Task SendMessage(PacketType packetType, IEnumerable<Buddy> recipientList, object data)
        {
            if (ServerProxy.TcpSocket == null)
            {
                return;
            }

            using (var dataWriter = new DataWriter(ServerProxy.TcpSocket.OutputStream))
            {
                //
                // Generate the recipient list string.
                //

                var packetBase = new PacketBase {PacketVersion = PacketVersion.Version1};

                var packetV1 = new PacketV1();

                foreach (Buddy b in recipientList)
                {
                    packetV1.Recipients.Add(b.EmailAddress);
                }

                packetV1.PacketType = packetType;

                packetV1.Data = data;



                //ToDo: Does 3000 need to be adjusted?  Max seen value is 750.  Should handle an exception for buffer over run. Find a way to make these buffers dynamic.
                var buffer = new byte[3000];

                using (var stream = new MemoryStream(buffer))
                {
                    var ser = new XmlSerializer(typeof(PacketV1));

                    ser.Serialize(stream, packetV1);
                }

                packetBase.Data = buffer;

                // ToDo: Adjust this value.  Find a way to make these buffers dynamic.
                var baseBuffer = new byte[10000];

                using (var stream = new MemoryStream(baseBuffer))
                {
                    var ser = new XmlSerializer(typeof(PacketBase));

                    ser.Serialize(stream, packetBase);
                }


                //
                // 1 represents the size of the PacketType enum.
                //

                var messageSize = baseBuffer.Length;

                byte[] bytes = BitConverter.GetBytes(messageSize);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }


                //
                // 1. Write the message size
                // 2. Write the object buffer.
                //

                dataWriter.WriteBytes(bytes);
                dataWriter.WriteBytes(baseBuffer);

                await dataWriter.StoreAsync();

                dataWriter.DetachStream();
            }
        }


        public string CreateUrlQueryString(Dictionary<string, string> fields)
        {
            string returnString = "";

            foreach (KeyValuePair<string, string> kvp in fields)
            {
                returnString += kvp.Key;
                returnString += '=';
                returnString += kvp.Value;
                returnString += '&';
            }

            return returnString.TrimEnd('&');
        }
    }
}
