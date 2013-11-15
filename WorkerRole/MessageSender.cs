using System;
using System.Collections.Generic;
using System.IO;
using Shared;

namespace WorkerRole
{
    public static class MessageSender
    {
        private const string ServerName = "Ozwego_Server_V1";


        public static void SendMessage(Client recipient, PacketType packetType, object data)
        {
            if (recipient == null)
            {
                return;
            }

            
#if DEBUG
            //
            // If we're debugging, make sure that the data type matches that required by the packetType, or else throw an exception.
            //

            var type = DataPacket.PacketTypeMap[packetType];

            if (type != null)
            {
                var testObject = Convert.ChangeType(data, type);
            }
            else
            {
                if (data != null)
                {
                    throw new ArgumentException("Data from this packet type was expected to be null, but wasn't.");
                }
            }
#endif

            // ToDo: Don't hardcode Packet Version 1 here.  Instead, add a packet version property to Client, and switch on that.
            var packetBase = new PacketBase {PacketVersion = PacketVersion.Version1};

            var packetV1 = new PacketV1
            {
                PacketType = packetType,
                Data = data,
                Sender = ServerName
            };

            packetV1.Recipients.Add(recipient.UserInfo.EmailAddress);

            packetBase.Data = packetV1;

            dynamic baseBuffer;

            using (var stream = new MemoryStream())
            {
                var binaryWriter = new BinaryWriter(stream);

                packetBase.Write(binaryWriter);

                baseBuffer = stream.ToArray();
            }

            var messageSize = baseBuffer.Length;

            byte[] bytes = BitConverter.GetBytes(messageSize);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }


            var offset = 0;

            var messageBuffer = new byte[sizeof(uint) + messageSize];

            bytes.CopyTo(messageBuffer, offset);

            offset += sizeof(uint);

            baseBuffer.CopyTo(messageBuffer, offset);

            recipient.Send(messageBuffer);
        }


        /// <summary>
        /// Sends a message to everyone on the list of the recipients, with the exception of the 
        /// sender client, as you don't want to send a message to yourself.
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="packetType"></param>
        /// <param name="data"></param>
        /// <param name="sender">Client that is sending this message, set to null if this is not a
        /// user-initiated message</param>
        public static void BroadcastMessage(IEnumerable<Client> recipients, PacketType packetType, object data, Client sender)
        {
            foreach (var c in recipients)
            {
                if (c != sender)
                {
                    SendMessage(c, packetType, data);
                }
            }
        }


        /// <summary>
        /// In this version, the message gets broadcasted to everyone, including the client who
        /// initiated the sending of the message.
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="packetType"></param>
        /// <param name="data"></param>
        public static void BroadcastMessage(IEnumerable<Client> recipients, PacketType packetType, object data)
        {
            foreach (var c in recipients)
            {
                SendMessage(c, packetType, data);
            }
        }
    }
}
