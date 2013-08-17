using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Microsoft.Live;
using Windows.UI.Core;
using Windows.UI.Popups;

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
            if (App.MainPageViewModel.ConnectionStatus)
            {
                return;
            }
            
            HostName hostName = new HostName("127.0.0.1");
            //var hostName = new HostName("192.168.1.2");
            //var hostName = new HostName("137.116.188.108");
            //var hostName = new HostName("138.91.91.144");
            //var hostName = new HostName("138.91.89.40");
        
            TcpSocket = new StreamSocket();

            try
            {
                //await TcpSocket.ConnectAsync(hostName, "10100");
                await TcpSocket.ConnectAsync(hostName, "4029");
                App.MainPageViewModel.ConnectionStatus = true;

                messageReceiver = MessageReceiver.GetInstance();
                messageSender = MessageSender.GetInstance();

                await messageSender.SendMessage(PacketType.LogIn, App.ClientBuddyInstance.MicrosoftAccountAddress);


                //
                // Now wait for incoming messages from the server.
                //

                await messageReceiver.WaitForData();
            }
            catch (Exception e)
            {
                //
                // If this is an unknown status, it means that the error is fatal and retry will
                // likely fail.
                //

                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

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
            App.MainPageViewModel.ConnectionStatus = false;

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.MainPageViewModel.BuddyList.Clear();
                App.MainPageViewModel.RoomMembers.Clear();
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


                LiveLoginResult result = await liveIdClient.LoginAsync(new[] { "wl.basic", "wl.emails" });
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
                        App.ClientBuddyInstance.MicrosoftAccountAddress = email;
                        App.RoomManager.AddMemberToRoom(email);
                        App.RoomManager.ChangeRoomHost(email);
                    }
                    catch
                    {
                        // Todo: determine what to do here if this fails...
                    }

                    App.MainPageViewModel.UserName = string.Format("Welcome {0}!", meResult.Result["first_name"]);

                    //string title = string.Format("Welcome {0}!", meResult.Result["first_name"]);
                    //var message = string.Format("You are now logged in - {0}", email);
                    //var dialog = new MessageDialog(message, title);
                    //dialog.Commands.Add(new UICommand("OK"));
                    //await dialog.ShowAsync();
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

    //ToDo: Consolidate this enum to both the client and the server
    /// <summary>
    /// Contains the types of messages that can be sent by the client and received from the server.
    /// There are two categories: out of game messages, which are messages that do not involve a 
    /// game of Ozwego, and in game messages, which directly affect gameplay.
    /// </summary>
    public enum PacketType : byte
    {
        //
        // Client to Server
        //

        LogIn = 1,
        LogOut = 2,
        JoinRoom = 3,
        LeaveRoom = 4,

        // Sendable only by room host:
        InitiateGame = 5,          // Move everyone in the room to the GameUI page
        StartGame = 6,          // Start the actual round

        // In game packet types:
        ClientDump = 7,
        ClientPeel = 8,
        ClientVictory = 9,
        ClientChat = 10,
        ClientMaxValue = 11,


        //
        // Server to Client
        //

        UserLoggedOut = 100,
        UserLoggedIn = 101,
        UserJoinedRoom = 102,
        UserLeftRoom = 103,
        HostTransfer = 104,       // You became the new room host.
        ServerPeel = 105,
        ServerDump = 106,
        ServerGameStart = 107,
        ServerGameOver = 108,
        ServerChat = 109,
        ServerBuddyList = 110,      // User just signed in, send the global buddy list.
        ServerRoomList = 111,      // User just joined a room, send the room members list.
        ServerInitiateGame = 112, // On receiving this, the client is supposed to move to the Game UI, but the actual game has NOT started yet.
        ServerMaxValue = 113
    }
}
