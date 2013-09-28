using Ozwego.BuddyManagement;
using Ozwego.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
            await SendMessage(packetType, "", recipientList);
        }


        public async Task SendMessage(PacketType packetType, string messageString)
        {
            var recipientList = new List<Buddy>();
            await SendMessage(packetType, messageString, recipientList);
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
            await SendMessage(packetType, messageString, recipientList);
        }



        private async Task SendMessage(PacketType packetType, string messageString, IEnumerable<Buddy> recipientList)
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

                string recipients = "";
                foreach (var buddy in recipientList)
                {
                    messageString += buddy.EmailAddress;
                    messageString += ',';
                }
                recipients = recipients.TrimEnd(',');

                var messageFields = new Dictionary<string, string>
                    {
                        {"recipients", recipients},
                        {"sender", App.ClientBuddyInstance.EmailAddress},
                        {"message", messageString}
                    };

                var message = CreateUrlQueryString(messageFields);

                //
                // 1 represents the size of the PacketType enum.
                //

                var messageSize = (uint)(1 + Encoding.UTF8.GetByteCount(message));
                byte[] bytes = BitConverter.GetBytes(messageSize);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }


                //
                // 1. Write the message size
                // 2. Write the packet type
                // 3. Write the message string, which contains the message followed by a list of 
                //    recipients delimited by "$$"
                //

                dataWriter.WriteBytes(bytes);
                dataWriter.WriteByte((byte)packetType);
                uint stringSize = dataWriter.WriteString(message);
                // ToDo: Examine stringSize and close the connection if it's invalid.

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
