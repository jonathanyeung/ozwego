using Ozwego.BuddyManagement;
using Ozwego.Common;
using Ozwego.Gameplay;
using Ozwego.Server;

using Ozwego.ViewModels;
using System;
using System.Collections.Generic;
using Shared;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Ozwego.Storage;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Ozwego.UI
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Lobby : Ozwego.Common.LayoutAwarePage
    {
        public Lobby()
        {
            this.InitializeComponent();

            var roomManager = RoomManager.GetInstance();
            RoomListUI.ItemsSource = roomManager.RoomMembers;

            var mainPageViewModel = MainPageViewModel.GetInstance();
            ChatWindow.ItemsSource = mainPageViewModel.ChatMessages;
            DataContext = mainPageViewModel;


            // This frame is hidden, meaning it is never shown.  It is simply used to load
            // each scenario page and then pluck out the input and output sections and
            // place them into the UserControls on the main page.
            HiddenFrame = new Windows.UI.Xaml.Controls.Frame();
            HiddenFrame.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //ContentRoot.Children.Add(HiddenFrame);

            LoadColumnView(typeof(FriendsList));
        }

        private void JoinButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //ToDo: Re-enable
            //var tempBuddy = BuddyListUI.SelectedItem as Buddy;

            //if (tempBuddy != null)
            //{
            //    App.RoomManager.JoinRoom(tempBuddy);
            //}
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
                await serverProxy.messageSender.SendMessage(PacketType.ClientInitiateGame);
            }

            Frame.Navigate(typeof(GameBoardPrototype), args);
        }


        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            string messageToSend = MessageChatBox.Text;
            MessageChatBox.Text = "";

            var mainPageViewModel = MainPageViewModel.GetInstance();
            mainPageViewModel.ChatMessages.Add("me:" + messageToSend);

            var roommanager = RoomManager.GetInstance();
            roommanager.InitiateMessageSend(messageToSend);
        }


        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
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
            }
        }

        private void ChatWindow_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChatWindow.SelectedIndex = -1;
        }

        #region List Pane

        private Frame HiddenFrame = null;

        public void LoadColumnView(Type columnViewType)
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
            LoadColumnView(typeof(FriendsList));
        }


        private void RequestsColumnView_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            LoadColumnView(typeof(RequestsList));
        }

        private async void Matchmaking_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var messageSender = MessageSender.GetInstance();
            await messageSender.SendMessage(PacketType.ClientStartingMatchmaking);
        }

        #endregion
    }
}
