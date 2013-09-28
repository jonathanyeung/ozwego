using Ozwego.Shared;
using Ozwego.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace Ozwego.Server
{

    public class MessageReceiver
    {
        private static MessageReceiver _instance;


        /// <summary>
        /// Private Constructor
        /// </summary>
        private MessageReceiver()
        {
        }


        /// <summary>
        /// Public method to instantiate ServerMessageReceiver singleton.
        /// </summary>
        /// <returns></returns>
        public static MessageReceiver GetInstance()
        {
            return _instance ?? (_instance = new MessageReceiver());
        }


        public async Task WaitForData()
        {
            var mainPageViewModel = MainPageViewModel.GetInstance();

            using (var dr = new DataReader(ServerProxy.TcpSocket.InputStream))
            {
                while (mainPageViewModel.ConnectionStatus)
                {
                    var stringHeader = await dr.LoadAsync(4);

                    if (stringHeader == 0)
                    {
                        mainPageViewModel.ConnectionStatus = false;
                        return;
                    }

                    int messageLength = dr.ReadInt32();
                    uint numStrBytes = await dr.LoadAsync((uint)messageLength);
                    var packetType = (PacketType)dr.ReadByte();

                    string msg = dr.ReadString(numStrBytes - 1);

                    //ToDo: Potentially need to modify this delimiter. (Probably do).

                    var decoder = new WwwFormUrlDecoder(msg);

                    string message = decoder.GetFirstValueByName("message");
                    string senderAccountAddress = "";

                    try
                    {
                        senderAccountAddress = decoder.GetFirstValueByName("sender");
                    }
                    catch (Exception)
                    {
                    }

                    var incomingMessage = IncomingMessageFactory.GetMessage(
                            packetType,
                            message,
                            senderAccountAddress);

                    incomingMessage.HandleMessage();
                }
            }
        }
    }
}
