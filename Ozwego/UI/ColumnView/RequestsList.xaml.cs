using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Ozwego.BuddyManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Ozwego.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RequestsList : Page
    {
        public RequestsList()
        {
            this.InitializeComponent();

            var requestManager = RequestManager.GetInstance();
            FriendRequestListUI.ItemsSource = requestManager.PendingFriendRequests;

            //ToDo:Line below.
            //GameRequestListUI.ItemsSource = App.FriendRequestManager.PendingGameRequests;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void OnGameRequestDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnYesTapped(object sender, TappedRoutedEventArgs e)
        {
            var pressedButton = sender as Button;
            var selectedBuddy = pressedButton.DataContext as Buddy;

            var requestManager = RequestManager.GetInstance();
            requestManager.AcceptFriendRequest(selectedBuddy);
        }

        private void OnQsnTapped(object sender, TappedRoutedEventArgs e)
        {
            //ToDo: Implement.
        }

        private void OnNoTapped(object sender, TappedRoutedEventArgs e)
        {
            var pressedButton = sender as Button;
            var selectedBuddy = pressedButton.DataContext as Buddy;

            var requestManager = RequestManager.GetInstance();
            requestManager.RejectFriendRequest(selectedBuddy);
        }
    }
}
