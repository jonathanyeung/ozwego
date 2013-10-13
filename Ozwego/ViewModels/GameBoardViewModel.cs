using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay
{
    public class GameBoardViewModel : INotifyPropertyChanged
    {
        private static GameBoardViewModel _instance;


        private GameBoardViewModel()
        {
        }


        public static GameBoardViewModel GetInstance()
        {
            return _instance ?? (_instance = new GameBoardViewModel());
        }


        private int _gameTime;

        public int GameTime
        {
            get
            {
                return _gameTime;
            }

            set
            {
                if (value != _gameTime)
                {
                    _gameTime = value;
                    NotifyPropertyChanged("GameTime");
                }
            }
        }

        private int _tilePileCount;

        public int TilePileCount
        {
            get
            {
                return _tilePileCount;
            }

            set
            {
                if (value != _tilePileCount)
                {
                    _tilePileCount = value;
                    NotifyPropertyChanged("TilePileCount");
                }
            }
        }

        //
        // Not actually in Game Board, but used in the matchmaking wait page...
        //
        private int _matchmakingWaitTime;

        public int MatchmakingWaitTime
        {
            get
            {
                return _matchmakingWaitTime;
            }

            set
            {
                if (value != _matchmakingWaitTime)
                {
                    _matchmakingWaitTime = value;
                    NotifyPropertyChanged("MatchmakingWaitTime");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
