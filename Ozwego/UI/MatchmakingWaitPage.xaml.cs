using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ozwego.Common;
using Ozwego.Server.MessageProcessors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Ozwego.Server;
using Ozwego.Gameplay;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Ozwego.UI
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MatchmakingWaitPage : Ozwego.Common.LayoutAwarePage
    {
        private DispatcherTimer _timer;
        public int WaitTimeUI = 0;

        public MatchmakingWaitPage()
        {
            this.InitializeComponent();

            DataContext = GameBoardViewModel.GetInstance();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //
            // Set up Timer UI.
            //
            var viewModel = GameBoardViewModel.GetInstance();
            viewModel.MatchmakingWaitTime = 0;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += OnTimerTick;
            _timer.Start();


            //
            // Hook up the Matchmaking Message Processor Events
            //
            MatchmakingMessageProcessor.GameFoundEvent += OnGameFoundEvent;
            MatchmakingMessageProcessor.GameNotFoundEvent += OnGameNotFoundEvent;

            base.OnNavigatedTo(e);
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


        void OnTimerTick(object sender, object e)
        {
            var viewModel = GameBoardViewModel.GetInstance();
            viewModel.MatchmakingWaitTime++;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            MatchmakingMessageProcessor.GameFoundEvent -= OnGameFoundEvent;
            MatchmakingMessageProcessor.GameNotFoundEvent -= OnGameNotFoundEvent;
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

        private void OnMainMenuTappedFromMatchmakingPane(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }


    }
}
