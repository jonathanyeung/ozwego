using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerRole
{
    public class MessageSender
    {
        // Singleton
        private static MessageSender _instance;

        private MessageSender() { }


        public static MessageSender GetMessageSender()
        {
            return _instance ?? (_instance = new MessageSender());
        }


        private void SendMessage(string recipient, PacketType packetType, Dictionary<string, string> arguments)
        {
            SendMessageInternal(recipient, packetType, CreateUrlQueryString(arguments));
        }


        public void SendMessage(string recipient, PacketType packetType, string arguments)
        {
            var msgFields = new Dictionary<string, string> {{"message", arguments}, {"recipients", recipient}};
            var messageToSend = CreateUrlQueryString(msgFields);

            SendMessageInternal(recipient, packetType, messageToSend);
        }


        /// <summary>
        /// Sends a message to the client if the client exists in the client list of this instance.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="packetType"></param>
        /// <param name="messageToSend"></param>
        private static void SendMessageInternal(string recipient, PacketType packetType, string messageToSend)
        {
            Client recipientClient = null;

            var recipientInfo = WorkerRole.ClientManager.GetClientInformation(recipient);

            if (recipientInfo == null)
            {
                return;
            }
            else if (recipientInfo.ServerInstanceId == WorkerRole.instanceID)
            {
                recipientClient = WorkerRole.ClientManager.GetLocalClient(recipient);
            }
            else
            {
                recipientClient = WorkerRole.InternalClients.FirstOrDefault
                        (c => c.ServerInstanceID == recipientInfo.ServerInstanceId);
            }


            //
            // Format the message
            //

            var messageSize = (uint) (1 + Encoding.UTF8.GetByteCount(messageToSend));

            byte[] bytes = BitConverter.GetBytes(messageSize);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            var offset = 0;
            var messageBuffer = new byte[sizeof (uint) + sizeof (PacketType) + messageToSend.Length];
            bytes.CopyTo(messageBuffer, offset);

            offset += sizeof (uint);
            messageBuffer[offset] = (byte) packetType;
            offset += sizeof (PacketType);

            byte[] newBytes = Encoding.UTF8.GetBytes(messageToSend);
            newBytes.CopyTo(messageBuffer, offset);

            if (recipientClient != null)
            {
                recipientClient.Send(messageBuffer);
            }
        }


        /// <summary>
        /// Sends a message to everyone on the list of the recipients, with the exception of the 
        /// sender client, as you don't want to send a message to yourself.
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="packetType"></param>
        /// <param name="arguments"></param>
        /// <param name="sender">Client that is sending this message, set to null if this is not a
        /// user-initiated message</param>
        public void BroadcastMessage(List<string> recipients, PacketType packetType, string arguments, string sender)
        {
            foreach (string recipient in recipients)
            {
                if (recipient != sender)
                {
                    SendMessage(recipient, packetType, arguments);
                }
            }
        }


        public void BroadcastMessage(List<string> recipients, PacketType packetType, Dictionary<string, string> arguments, string sender)
        {
            foreach (string recipient in recipients)
            {
                if (recipient != sender)
                {
                    SendMessage(recipient, packetType, arguments);
                }
            }
        }

        /// <summary>
        /// In this version, the message gets broadcasted to everyone, including the client who
        /// initiated the sending of the message.
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="packetType"></param>
        /// <param name="arguments"></param>
        public void BroadcastMessage(List<string> recipients, PacketType packetType, string arguments)
        {
            foreach (string recipient in recipients)
            {
                SendMessage(recipient, packetType, arguments);
            }
        }


        /// <summary>
        /// Takes in a list of clients who are the intended recipients of a message.
        /// It then returns a properly formatted string containing those recipients
        /// that can be used as the string argument in the message.
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        //public string GetRecipientListFormattedString(List<Client> recipients)
        //{
        //    const char delimiter = ',';

        //    string returnString = "";

        //    foreach (var client in recipients)
        //    {
        //        returnString += client.Information.UserName;
        //        returnString += delimiter;
        //    }

        //    return returnString.TrimEnd(delimiter);
        //}

        //ToDo: Consolidate overloaded methods.
        public string GetRecipientListFormattedString(List<string> recipients)
        {
            const char delimiter = ',';

            string returnString = "";

            foreach (var client in recipients)
            {
                returnString += recipients;
                returnString += delimiter;
            }

            return returnString.TrimEnd(delimiter);
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
