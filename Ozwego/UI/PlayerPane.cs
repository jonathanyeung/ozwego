using Ozwego.Gameplay;
using Ozwego.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Animation;

namespace Ozwego.UI
{
    /// <summary>
    /// Represents the stack panel that contains the players of the current game.  This UI element
    /// notifies the user of the dump, peel, and other information from other players.
    /// </summary>
    class PlayerPane
    {
        //private StackPanel _playerPane;

        public ObservableCollection<PlayerPaneViewModel> PlayerList = new ObservableCollection<PlayerPaneViewModel>();

        public PlayerPane()
        {
        }

        public void Initialize()
        {
            var gameController = GameController.GetInstance();

            foreach (IPlayer player in gameController.LocalPlayers)
            {
                PlayerList.Add(new PlayerPaneViewModel(player.Alias));
            }
        }


        public void UpdateUiForPeel(string player)
        {
            PlayerPaneViewModel curViewModel = null;
            foreach (PlayerPaneViewModel viewModel in PlayerList)
            {
                if (viewModel.PlayerName == player)
                {
                    curViewModel = viewModel;
                }
            }

            if (null != curViewModel)
            {
                curViewModel.ActionString = "Peel!";
            }
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
