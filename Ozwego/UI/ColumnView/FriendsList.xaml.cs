using Ozwego.BuddyManagement;
using Ozwego.Server;
using Ozwego.Shared;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ozwego.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FriendsList : Page
    {
        public FriendsList()
        {
            this.InitializeComponent();

            var friendManager = FriendManager.GetInstance();
            OnlineFriendsListUI.ItemsSource = friendManager.OnlineFriendList;
            OfflineFriendsListUI.ItemsSource = friendManager.OfflineFriendList;
            FriendsSearchUI.ItemsSource = friendManager.FriendSearchResultsList;

            SearchBar.KeyUp += SearchBar_KeyUp;
        }

        async void SearchBar_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (SearchBar.Text != "")
            {
                FriendsSearchUI.Visibility = Visibility.Visible;

                var serverProxy = ServerProxy.GetInstance();
                if (null != serverProxy.messageSender)
                {
                    await serverProxy.messageSender.SendMessage(PacketType.ClientFindBuddyFromGlobalList, SearchBar.Text);
                }
            }
            else
            {
                FriendsSearchUI.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }


        private async void AddFriendButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            var serverProxy = ServerProxy.GetInstance();

            if (null != serverProxy.messageSender)
            {
                await serverProxy.messageSender.SendMessage(PacketType.ClientSendFriendRequest, SearchBar.Text);
            }
        }

        private void OnSearchResultTapped(object sender, TappedRoutedEventArgs e)
        {
            var pressedText = sender as TextBlock;
            SearchBar.Text = pressedText.Text;
        }

        private void OnlineBuddyDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;

            var roomManager = RoomManager.GetInstance();
            roomManager.JoinRoom(textBlock.Text);
        }
    }
}
