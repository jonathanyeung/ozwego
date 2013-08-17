using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozwego.BuddyManagement;
using Ozwego.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Ozwego.UI
{
    /// <summary>
    /// Represents the stack panel that contains the players of the current game.  This UI element
    /// notifies the user of the dump, peel, and other information from other players.
    /// </summary>
    class PlayerPane
    {
        //private StackPanel _playerPane;

        public readonly ObservableCollection<PlayerPaneViewModel> PlayerList = new ObservableCollection<PlayerPaneViewModel>();

        public PlayerPane()
        {
            Initialize();
        }

        //private Grid GeneratePlayerUiElement(string playerName)
        //{
        //    var grid = new Grid();

        //    var rect1 = new Rectangle();
        //    rect1.Fill = new SolidColorBrush(Colors.Green);
        //    rect1.Height = 150;
        //    rect1.Width = 150;

        //    var rect2 = new Rectangle();
        //    rect2.Fill = new SolidColorBrush(Colors.LimeGreen);
        //    rect2.Height = 30;
        //    rect2.Width = 150;

        //    var tb = new TextBlock();
        //    tb.Text = playerName;

        //    grid.Children.Add(rect1);
        //    grid.Children.Add(rect2);
        //    grid.Children.Add(tb);

        //    return grid;
        //}


        private void Initialize()
        {
            foreach (Buddy player in App.MainPageViewModel.RoomMembers)
            {
                PlayerList.Add(new PlayerPaneViewModel(player.MicrosoftAccountAddress));
            }
        }


        public void UpdateUiForPeel(string player)
        {
            throw new NotImplementedException();
        }


        public void UpdateUiForDump(string player)
        {
            throw new NotImplementedException();
        }


        public void UpdateUiForVictory(string player)
        {
            throw new NotImplementedException();
        }


        public void UpdateUiForPlayerQuitting(string player)
        {
            throw new NotImplementedException();
        }

    }
}
