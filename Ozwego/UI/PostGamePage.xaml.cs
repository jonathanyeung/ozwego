using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ozwego.Common;
using Ozwego.Gameplay;
using Ozwego.Server;
using Shared;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class PostGamePage : Ozwego.Common.LayoutAwarePage
    {
        private PostGamePageNavigationArgs _navigationArgs = null;

        public PostGamePage()
        {
            this.InitializeComponent();
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
            _navigationArgs = (PostGamePageNavigationArgs)navigationParameter;
            
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


        private async void OnPlayAgainClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            switch (_navigationArgs.GameMode)
            {
                case GameMode.Friendly:
                    Frame.Navigate(typeof(Lobby));
                    break;

                case GameMode.Matchmaking:
                    var serverProxy = ServerProxy.GetInstance();

                    if (serverProxy.messageSender != null)
                    {
                        await serverProxy.messageSender.SendMessage(PacketType.ClientStartingMatchmaking);
                        Frame.Navigate(typeof(MatchmakingWaitPage));
                    }
                    else
                    {
                        var args = new GameBoardNavigationArgs()
                        {
                            GameConnectionType = GameConnectionType.Local,
                            BotCount = 1
                        };

                        Frame.Navigate(typeof(GameBoardPrototype), args);
                    }

                    break;

                default:
                    Frame.Navigate(typeof(MainPage));
                    break;
            }
        }

        private void OnMainMenuClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
