using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ozwego.BuddyManagement;
using Ozwego.Server;
using Ozwego.Server.MessageProcessors;
using Ozwego.UI;
using Ozwego.UI.Background;
using Shared;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Ozwego.ViewModels;
using Ozwego.Storage;
using Ozwego.UI.OOBE;
using Ozwego.Gameplay;
using Ozwego.UI.MainPage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ozwego
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Ozwego.Common.LayoutAwarePage
    {
        //private BackgroundGrid _background;
        //private Friend ClientFriendInstance;

        public MainPage()
        {
            InitializeComponent();

            var mainPageViewModel = MainPageViewModel.GetInstance();
            DataContext = mainPageViewModel;

            UserStatsUI.DataContext = Settings.userInstance;

            var gameBoardViewModel = GameBoardViewModel.GetInstance();
            MatchmakingPane.DataContext = gameBoardViewModel;

            //_background = new BackgroundGrid();
        }

        #region overrides
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
                var serverProxy = ServerProxy.GetInstance();
                await serverProxy.Authenticate();
                serverProxy.Connect();
            }
            catch (Exception)
            {
                //ToDo: This has thrown a LiveConnectException during debugging.  Need to handle it properly here.
                throw;
            }


            //
            // Background grid initialization
            //

            //RootGrid.Children.Insert(0, _background.PolygonGrid);

            //_background.BeginSubtleAnimation();


            // This frame is hidden, meaning it is never shown.  It is simply used to load
            // each scenario page and then pluck out the input and output sections and
            // place them into the UserControls on the main page.
            HiddenFrame = new Windows.UI.Xaml.Controls.Frame();
            HiddenFrame.Visibility = Windows.UI.Xaml.Visibility.Collapsed;


            //
            // Load OOBE if this is first launch.
            //

            if (Settings.IsFirstLaunch)
            {
                if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
                LoadOOBEView(typeof(OOBEPage1));  
            }


            //
            // Friend Lobby Initialization
            //

            var roomManager = RoomManager.GetInstance();
            RoomListUI.ItemsSource = roomManager.RoomMembers;

            var mainPageViewModel = MainPageViewModel.GetInstance();
            ChatWindow.ItemsSource = mainPageViewModel.ChatMessages;
            DataContext = mainPageViewModel;



            //ContentRoot.Children.Add(HiddenFrame);

            LoadColumnView(typeof(FriendsList));
        }


        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            base.LoadState(navigationParameter, pageState);

            try
            {
                var navigationArgs = (MainPageNavigationArgs)navigationParameter;

                if (navigationArgs == null)
                {
                    return;
                }

                switch (navigationArgs.PageView)
                {
                    case MainPageView.Matchmaking:
                        OnMatchmakingButtonTapped(this, null);
                        break;

                    case MainPageView.FriendChallenge:

                        break;

                    default:
                        break;
                }
            }
            catch(InvalidCastException e)
            {
                // Swallow invalid cast exception if no navigation args were passed.
            }
        }

        #endregion overrides

        #region Main Menu

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


        private void OnGameDebugButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            GameBoardNavigationArgs args = new GameBoardNavigationArgs()
            {
                GameConnectionType = GameConnectionType.Local,
                BotCount = 0
            };

            Frame.Navigate(typeof(GameBoardPrototype), args);
        }


        private void OnFriendChallengeButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            MainToFriendChallenge.Begin();
        }

        private async void GameHistoryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var gdh = GameDataHistory.GetInstance();

            //var dataSet1 = new GameData { GameDuration = 50, GameHost = "abc@outlook.com", Winner = "abc@outlook.com", GameStartTime = new DateTime(2013, 10, 14) };

            //dataSet1.GameMoves.Add(new GameMoveDataPoint("abc@outlook.com", 5, MoveType.Peel));

            //var stats = new PlayerGameStats()
            //{
            //    AvgTimeBetweenDumps = 5,
            //    AvgTimeBetweenPeels = 5,
            //    NumberOfDumps = 3,
            //    NumberOfPeels = 3,
            //    PerformedFirstPeel = true,
            //    IsWinner = false,
            //    RawGameData = new List<GameMoveDataPoint>() { new GameMoveDataPoint("abc@outlook.com", 1, MoveType.Peel) }
            //};

            ////var playerTuple = new PlayerTuple { Name = "abc@outlook.com", Stats = stats };
            ////var tupleList = new List<PlayerTuple> { playerTuple };
            ////dataSet1.Players = tupleList;


            ////
            //// Add one set and commit it to storage.
            ////

            //await gdh.ClearAllStoredData();

            //await gdh.StoreGameData(dataSet1);


            //var crap = await gdh.RetrieveGameData();

            await gdh.UploadPendingGameData();
        }


        #endregion

        #region Matchmaking

        private DispatcherTimer _matchmakingTimer;

        private async void OnMatchmakingButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            //
            // Navigate to the page.
            //

            MainToMatchmaking.Begin();

            OnNavigatedToMatchmakingPane();


            //
            // Send the matchmaking begin packets to the server if connected.
            //

            var serverProxy = ServerProxy.GetInstance();

            // ToDo: Replace all of these null check calls of messageSender with checks to the connection status.
            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.c_StartingMatchmaking);
            }
            else
            {

                GameBoardNavigationArgs args = new GameBoardNavigationArgs()
                {
                    GameConnectionType = GameConnectionType.Local,
                    BotCount = 1
                };

                // ToDo: If the user navigates away from this page, then the navigation to the gameboard needs to be cancelled.
                await Task.Delay(5000);

                // ToDo: Re-enable.
                //Frame.Navigate(typeof(GameBoardPrototype), args);
            }
        }


        private void OnMainMenuTappedFromMatchmakingPane(object sender, TappedRoutedEventArgs e)
        {
            MatchmakingToMain.Begin();

            OnNavigatedFromMatchmakingPane();
        }


        private async void OnNavigatedToMatchmakingPane()
        {
            //
            // Set up Timer UI.
            //
            var viewModel = GameBoardViewModel.GetInstance();
            viewModel.MatchmakingWaitTime = 0;
            _matchmakingTimer = new DispatcherTimer();
            _matchmakingTimer.Interval = new TimeSpan(0, 0, 1);
            _matchmakingTimer.Tick += OnMatchmakingTimerTick;
            _matchmakingTimer.Start();


            //
            // Hook up the Matchmaking Message Processor Events
            //
            MatchmakingMessageProcessor.GameFoundEvent += OnGameFoundEvent;
            MatchmakingMessageProcessor.GameNotFoundEvent += OnGameNotFoundEvent;


            //
            // Change the background animation style
            //

            await Task.Delay(1000);

            //_background.BeginFlashAnimation();
        }


        private void OnGameNotFoundEvent(object sender)
        {
            GameBoardNavigationArgs args = new GameBoardNavigationArgs()
            {
                GameConnectionType = GameConnectionType.Local,
                BotCount = 1
            };

            Frame.Navigate(typeof(GameBoardPrototype), args);
        }


        private void OnGameFoundEvent(object sender)
        {
            GameBoardNavigationArgs args = new GameBoardNavigationArgs()
            {
                GameConnectionType = GameConnectionType.Online,
                BotCount = 0
            };

            Frame.Navigate(typeof(GameBoardPrototype), args);
        }


        private void OnMatchmakingTimerTick(object sender, object e)
        {
            var viewModel = GameBoardViewModel.GetInstance();
            viewModel.MatchmakingWaitTime++;
        }


        private void OnNavigatedFromMatchmakingPane()
        {
            _matchmakingTimer.Stop();
            _matchmakingTimer.Tick -= OnMatchmakingTimerTick;
            MatchmakingMessageProcessor.GameFoundEvent -= OnGameFoundEvent;
            MatchmakingMessageProcessor.GameNotFoundEvent -= OnGameNotFoundEvent;

            //
            // Change the background animation style
            //

            //_background.BeginSubtleAnimation();
        }

        #endregion

        #region Friend Lobby


        private Frame HiddenFrame = null;

        private void OnNavigatedToFriendLobby()
        {
        }





        private void OnMainMenuTappedFromFriendChallengePane(object sender, TappedRoutedEventArgs e)
        {
            FriendChallengeToMain.Begin();
        }


        private void LeaveRoomButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var roomManager = RoomManager.GetInstance();
            roomManager.LeaveRoom();
            roomManager.ChangeRoomHost(Settings.userInstance);
        }


        private async void StartGame_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //
            // Only allow the room host to initiate a game.
            //

            var roomManager = RoomManager.GetInstance();

            if (roomManager.Host.EmailAddress != Settings.EmailAddress)
            {
                return;
            }

            var args = new GameBoardNavigationArgs()
            {
                GameConnectionType = GameConnectionType.Online,
                BotCount = 0
            };

            var serverProxy = ServerProxy.GetInstance();
            if (serverProxy.messageSender != null)
            {
                await serverProxy.messageSender.SendMessage(PacketType.c_InitiateGame);
            }

            Frame.Navigate(typeof(GameBoardPrototype), args);
        }


        private void MessageChatBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var roomManager = RoomManager.GetInstance();

            // ToDo: Routing of the unused e is a bit hacky.
            if ((e.Key == VirtualKey.Enter) && MessageChatBox.Text != "")
            {
                string messageToSend = MessageChatBox.Text;
                MessageChatBox.Text = "";

                var mainPageViewModel = MainPageViewModel.GetInstance();
                mainPageViewModel.ChatMessages.Add("me: " + messageToSend);

                roomManager.InitiateMessageSend(messageToSend);

                ChatWindowScrollViewer.ScrollToVerticalOffset(ChatWindowScrollViewer.VerticalOffset + 100);
            }
        }


        //private void ChatWindow_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ChatWindow.SelectedIndex = -1;
        //}


        private void LoadColumnView(Type columnViewType)
        {
            // Load the ScenarioX.xaml file into the Frame.
            HiddenFrame.Navigate(columnViewType, this);

            // Get the top element, the Page, so we can look up the elements
            // that represent the input and output sections of the ScenarioX file.
            Page hiddenPage = HiddenFrame.Content as Page;

            UIElement columnContent = hiddenPage.FindName("ColumnContent") as UIElement;

            if (columnContent == null)
            {
                throw new ArgumentException("The column content could not be found in LoadColumnView()");
            }

            // Find the LayoutRoot which parents the input and output sections in the main page.
            Panel panel = hiddenPage.FindName("LayoutRoot") as Panel;

            if (panel != null)
            {
                // Get rid of the content that is currently in the intput and output sections.
                panel.Children.Remove(columnContent);

                // Populate the column content sections with the newly loaded content.
                ColumnContentSection.Content = columnContent;
            }
            else
            {
                throw new ArgumentException("The loaded layoutRoot could not be found in LoadColumnView()");
            }
        }


        private void FriendsColumnView_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FriendsColumnFocus.Begin();
            LoadColumnView(typeof(FriendsList));
        }


        private void RequestsColumnView_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            RequestsColumnFocus.Begin();
            LoadColumnView(typeof(RequestsList));
        }


        #endregion

        #region OOBE UI

        List<Type> OOBEPages = new List<Type> 
        {
            typeof(OOBEPage1),  
            typeof(OOBEPage3), 
            typeof(OOBEPage4), 
            typeof(OOBEPage5), 
            typeof(OOBEPage6), 
            typeof(OOBEPage7),
            typeof(OOBEPage2)
        };

        private int PageIndex = 0;

        private void LoadOOBEView(Type OOBEPageView)
        {
            // Load the ScenarioX.xaml file into the Frame.
            HiddenFrame.Navigate(OOBEPageView, this);

            // Get the top element, the Page, so we can look up the elements
            // that represent the input and output sections of the ScenarioX file.
            Page hiddenPage = HiddenFrame.Content as Page;

            UIElement columnContent = hiddenPage.FindName("OOBEContent") as UIElement;

            if (columnContent == null)
            {
                throw new ArgumentException("The OOBE content could not be found in LoadOOBEView()");
            }

            // Find the LayoutRoot which parents the input and output sections in the main page.
            Panel panel = hiddenPage.FindName("LayoutRoot") as Panel;

            if (panel != null)
            {
                // Get rid of the content that is currently in the intput and output sections.
                panel.Children.Remove(columnContent);

                // Populate the OOBE content sections with the newly loaded content.
                OOBESection.Content = columnContent;
            }
            else
            {
                throw new ArgumentException("The loaded layoutRoot could not be found in LoadOOBEView()");
            }
        }


        private void OnOOBEPreviousClick(object sender, RoutedEventArgs e)
        {
            if (PageIndex > 0)
            {
                PageIndex--;

                LoadOOBEView(OOBEPages[PageIndex]);
            }
            else if (PageIndex == 0)
            {
                OOBEPrevButton.IsEnabled = false;
            }
        }


        private void OnOOBENextClick(object sender, RoutedEventArgs e)
        {
            if (PageIndex < OOBEPages.Count - 1)
            {
                PageIndex++;

                LoadOOBEView(OOBEPages[PageIndex]);
            }
            else if (PageIndex == OOBEPages.Count - 1)
            {
                //
                // User has completed OOBE, so don't show it again.
                //

                Settings.IsFirstLaunch = false;

                if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
            }
        }

        #endregion
    }
}
