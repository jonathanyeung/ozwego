using Microsoft.Live;
using Ozwego.BuddyManagement;

using Ozwego.ViewModels;
using System;
using System.Threading.Tasks;
using Shared;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Popups;
using Ozwego.Storage;

namespace Ozwego.Server
{
    public class ServerProxy
    {

        private static ServerProxy _instance;

        internal static StreamSocket TcpSocket;

        public MessageReceiver messageReceiver;
        public MessageSender messageSender;

        /// <summary>
        /// Private Constructor
        /// </summary>
        private ServerProxy()
        {
        }


        /// <summary>
        /// Public method to instantiate ServerMessageReceiver singleton.
        /// </summary>
        /// <returns></returns>
        public static ServerProxy GetInstance()
        {
            return _instance ?? (_instance = new ServerProxy());
        }


        public async void Connect()
        {
            var mainPageViewModel = MainPageViewModel.GetInstance();

            if (mainPageViewModel.ConnectionStatus)
            {
                return;
            }

            var localPort = "4032";
            var localHostName = new HostName("192.168.1.2");

            // Currently doesn't work.
            var OzwegoProduction = new HostName("138.91.91.144");
            var OzwegoPort = "4032";

            var OzwegoStaging = new HostName("138.91.89.40");
            var OzwegoStagingPort = "4032";

            TcpSocket = new StreamSocket();

            try
            {
                //await TcpSocket.ConnectAsync(localHostName, localPort);
                await TcpSocket.ConnectAsync(OzwegoStaging, OzwegoStagingPort);

                mainPageViewModel.ConnectionStatus = true;

                messageReceiver = MessageReceiver.GetInstance();
                messageSender = MessageSender.GetInstance();

                await messageSender.SendMessage(PacketType.c_LogIn, Settings.userInstance);


                //
                // Now wait for incoming messages from the server.
                //

                await messageReceiver.WaitForData();
            }
            catch (Exception e)
            {
                //ToDo: Re-enable the code below.
//#if DEBUG
//                // Throw an exception if it's not due to the server not running.
//                if (e.HResult != -2147014835)
//                {
//                    throw;
//                }
//#endif
//                //
//                // If this is an unknown status, it means that the error is fatal and retry will
//                // likely fail.
//                //

//                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
//                {
//                    throw;
//                }

                // ToDo: Retry the connection here.

                //ToDo: This call throws a taskCanceled exception if you jam the button...
                Disconnect();
            }
        }


        public async void Disconnect()
        {
            if (TcpSocket != null)
            {
                TcpSocket.Dispose();
                TcpSocket = null;
            }

            var mainPageViewModel = MainPageViewModel.GetInstance();
            mainPageViewModel.ConnectionStatus = false;

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var roomManager = RoomManager.GetInstance();
                    roomManager.LeaveRoom();
                });
        }

        private LiveConnectSession _session;


        public async Task Authenticate()
        {
            var liveIdClient = new LiveAuthClient();

            while (_session == null)
            {
                //
                // ToDo: Determine if this code is necessary:
                // Force a logout to make it easier to test with multiple Microsoft Accounts
                //

                if (liveIdClient.CanLogout)
                    liveIdClient.Logout();


                LiveLoginResult result = await liveIdClient.LoginAsync(new[] {"wl.basic", "wl.emails"});
                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    _session = result.Session;
                    var client = new LiveConnectClient(result.Session);
                    LiveOperationResult meResult = await client.GetAsync("me");
                    dynamic dynamicResult = meResult.Result;

                    string userName = "";
                    string email = "";

                    try
                    {
                        userName = string.Format("{0} {1}", dynamicResult.first_name, dynamicResult.last_name);
                        email = dynamicResult.emails.preferred;
                        Settings.EmailAddress = email;
                        //Settings.EmailAddress = "mobius02@gmail.com";

                        var roomManager = RoomManager.GetInstance();
                        roomManager.AddMemberToRoom(Settings.userInstance);
                        roomManager.ChangeRoomHost(Settings.userInstance);
                    }
                    catch
                    {
                        // Todo: determine what to do here if this fails...
                    }

                    var mainPageViewModel = MainPageViewModel.GetInstance();
                    mainPageViewModel.UserName = string.Format("Welcome {0}!", meResult.Result["first_name"]);

                }
                else
                {
                    _session = null;
                    var dialog = new MessageDialog("You must log in.", "Login Required");
                    dialog.Commands.Add(new UICommand("OK"));
                    await dialog.ShowAsync();
                }
            }
        }
    }
}
