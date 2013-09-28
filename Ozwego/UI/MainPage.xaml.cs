using System;
using Ozwego.BuddyManagement;
using Ozwego.Server;
using Ozwego.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Ozwego.ViewModels;
using Ozwego.Storage;

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
            Frame.Navigate(typeof(GameBoardPrototype));
        }

        private void OnMultiPlayerButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Lobby));
        }

        private async void GameHistoryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var gdh = GameDataHistory.GetInstance();
            var data = await gdh.RetrieveGameData();
        }
    }
}
