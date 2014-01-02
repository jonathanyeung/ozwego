using System.IO;
using Ozwego.BuddyManagement;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozwego.Storage;
using Ozwego.ViewModels;
using Shared;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Ozwego.Server
{
    public class MessageSender
    {
        private static MessageSender _instance;

        private DispatcherTimer _timer;


        /// <summary>
        /// Private Constructor
        /// </summary>
        private MessageSender()
        {
            _timer = new DispatcherTimer();
            _InitializeTimer();
        }


        /// <summary>
        /// TCP Heart beat timer.
        /// </summary>
        private void _InitializeTimer()
        {
            _timer.Interval = new TimeSpan(0, 0, 5);
            _timer.Tick += async (s, e) => { await SendMessage(PacketType.c_HeartBeat, null, null); };
            _timer.Start();
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
            var recipientList = new List<Friend>();
            await SendMessage(packetType, recipientList, null);
        }


        public async Task SendMessage(PacketType packetType, object data)
        {
            var recipientList = new List<Friend>();
            await SendMessage(packetType, recipientList, data);
        }


        private async Task SendMessage(PacketType packetType, IEnumerable<Friend> recipientList, object data)
        {
            var mainPageViewModel = MainPageViewModel.GetInstance();

            if ((ServerProxy.TcpSocket == null) || mainPageViewModel.ConnectionStatus == false)
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

            using (var dataWriter = new DataWriter(ServerProxy.TcpSocket.OutputStream))
            {
                byte[] msgSizeBuffer;
                dynamic baseBuffer = null;

                if (PacketType.c_HeartBeat == packetType)
                {
                    // Message Size is specified by the size of an int.
                    msgSizeBuffer = new byte[] {0, 0, 0, 0};
                }
                else
                {
                    var packetBase = new PacketBase {PacketVersion = PacketVersion.Version1};

                    var packetV1 = new PacketV1
                        {
                            PacketType = packetType,
                            Data = data,
                            Sender = Settings.EmailAddress
                        };

                    foreach (var b in recipientList)
                    {
                        packetV1.Recipients.Add(b.EmailAddress);
                    }

                    packetBase.Data = packetV1;

                    using (var stream = new MemoryStream())
                    {
                        var binaryWriter = new BinaryWriter(stream);

                        packetBase.Write(binaryWriter);

                        baseBuffer = stream.ToArray();
                    }


                    var messageSize = baseBuffer.Length;

                    msgSizeBuffer = BitConverter.GetBytes(messageSize);

                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(msgSizeBuffer);
                    }
                }


                //
                // 1. Write the message size
                // 2. Write the object buffer (unless it's a heartbeat)
                //

                dataWriter.WriteBytes(msgSizeBuffer);

                if (baseBuffer != null)
                {
                    dataWriter.WriteBytes(baseBuffer);
                }

                await dataWriter.StoreAsync();

                dataWriter.DetachStream();
            }
        }
    }
}
