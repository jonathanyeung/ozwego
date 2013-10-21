using System;
using System.Collections.Generic;
using Ozwego.BuddyManagement;
using Ozwego.Server;
using Ozwego.UI;
using Shared;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Ozwego.ViewModels;
using Ozwego.Storage;

using Ozwego.Gameplay;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ozwego
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Ozwego.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var mainPageViewModel = MainPageViewModel.GetInstance();
            DataContext = mainPageViewModel;
            UserStatsUI.DataContext = App.ClientBuddyInstance;
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }

                var serverProxy = ServerProxy.GetInstance();
                await serverProxy.Authenticate();
                serverProxy.Connect();
            }
            catch (Exception)
            {
                //ToDo: This has thrown a LiveConnectException during debugging.  Need to handle it properly here.
                throw;
            }
        }

        #region Button EH

        private void LogInButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var serverProxy = ServerProxy.GetInstance();
            serverProxy.Connect();
        }

        private void LogOutButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var serverProxy = ServerProxy.GetInstance();
            serverProxy.Disconnect();
        }

        private void OnSinglePlayerButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            GameBoardNavigationArgs args = new GameBoardNavigationArgs()
            {
                GameConnectionType = GameConnectionType.Local,
                BotCount = 1
            };

            Frame.Navigate(typeof(GameBoardPrototype), args);
        }

        private void OnMultiPlayerButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Lobby));
        }

        private async void GameHistoryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var gdh = GameDataHistory.GetInstance();

            var dataSet1 = new GameData { GameDuration = 50, GameHost = "abc@outlook.com", Winner = "abc@outlook.com", GameStartTime = new DateTime(2013, 10, 14)};

            dataSet1.GameMoves.Add(new GameMoveDataPoint("abc@outlook.com", 5, MoveType.Peel));

            var stats = new PlayerGameStats()
            {
                AvgTimeBetweenDumps = 5,
                AvgTimeBetweenPeels = 5,
                NumberOfDumps = 3,
                NumberOfPeels = 3,
                PerformedFirstPeel = true,
                IsWinner = false,
                RawGameData = new List<GameMoveDataPoint>() { new GameMoveDataPoint("abc@outlook.com", 1, MoveType.Peel) }
            };

            var playerTuple = new PlayerTuple { Name = "abc@outlook.com", Stats = stats };
            var tupleList = new List<PlayerTuple> { playerTuple };
            dataSet1.Players = tupleList;


            //
            // Add one set and commit it to storage.
            //

            await gdh.ClearAllStoredData();

            await gdh.StoreGameData(dataSet1);
            

            var crap = await gdh.RetrieveGameData();

            await gdh.UploadPendingGameData();
        }

        private async void OnMatchmakingButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            var serverProxy = ServerProxy.GetInstance();

            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.ClientStartingMatchmaking);
                Frame.Navigate(typeof(MatchmakingWaitPage));
            }
            else
            {
                GameBoardNavigationArgs args = new GameBoardNavigationArgs()
                {
                    GameConnectionType = GameConnectionType.Local,
                    BotCount = 1
                };

                Frame.Navigate(typeof(GameBoardPrototype), args);
            }
        }

        #endregion

        private void ShowPopupOffsetClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
        }

        private void CloseButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }

        private string _enteredAlias;

        private async void CheckIfAvailableClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DataBaseMessageProcessor.DataBaseMessageReceivedEvent += AliasAvailableCallback;

            _enteredAlias = AliasTextBox.Text;

            var serverProxy = ServerProxy.GetInstance();

            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.ClientQueryIfAliasAvailable, _enteredAlias);
            }
        }
        

        private void AliasAvailableCallback(object sender, string message)
        {
            if (message == "true")
            {
                Settings.Alias = _enteredAlias;
                PopUpStatus.Text = "Alias Available and Set!";
            }
            else
            {
                PopUpStatus.Text = "Alias Taken!";
            }

            // ToDo: Once the alias has been accepted, then upload it to the server.
        }
    }
}
