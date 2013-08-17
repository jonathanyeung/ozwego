using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ozwego.BuddyManagement;
using Ozwego.Common;
using Ozwego.Server;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            BuddyListUI.ItemsSource = App.MainPageViewModel.BuddyList;
            RoomListUI.ItemsSource = App.MainPageViewModel.RoomMembers;
            ChatWindow.ItemsSource = App.MainPageViewModel.ChatMessages;
            DataContext = App.MainPageViewModel;
        }

        private void JoinButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var tempBuddy = BuddyListUI.SelectedItem as Buddy;

            if (tempBuddy != null)
            {
                App.RoomManager.JoinRoom(tempBuddy);
            }
        }

        private void LeaveRoomButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            App.RoomManager.LeaveRoom();
        }


        private async void StartGame_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //
            // Only allow the room host to initiate a game.
            //

            if (App.MainPageViewModel.RoomHost != App.ClientBuddyInstance.MicrosoftAccountAddress)
            {
                return;
            }

            Frame.Navigate(typeof(GameBoardPrototype));

            if (App.ServerProxy.messageSender != null)
            {
                await App.ServerProxy.messageSender.SendMessage(PacketType.InitiateGame);
            }
        }


        private void OnBuddyDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var panel = sender as StackPanel;
            var textBlock = panel.Children[0] as TextBlock;
            App.RoomManager.JoinRoom(textBlock.Text);
        }


        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            string messageToSend = MessageChatBox.Text;
            MessageChatBox.Text = "";
            App.MainPageViewModel.ChatMessages.Add("me:" + messageToSend);

            App.RoomManager.InitiateMessageSend(messageToSend);
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
            // ToDo: Routing of the unused e is a bit hacky.
            if ((e.Key == VirtualKey.Enter) && MessageChatBox.Text != "")
            {
                string messageToSend = MessageChatBox.Text;
                MessageChatBox.Text = "";
                App.MainPageViewModel.ChatMessages.Add("me: " + messageToSend);

                App.RoomManager.InitiateMessageSend(messageToSend);
            }
        }

        private void ChatWindow_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChatWindow.SelectedIndex = -1;
        }
    }
}
