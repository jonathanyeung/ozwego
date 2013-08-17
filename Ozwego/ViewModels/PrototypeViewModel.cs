using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay
{
    public class PrototypeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Tile> TileRack = new ObservableCollection<Tile>();

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

        private int _personalTileCount;

        public int PersonalTileCount
        {
            get
            {
                return _personalTileCount;
            }

            set
            {
                if (value != _personalTileCount)
                {
                    _personalTileCount = value;
                    NotifyPropertyChanged("PersonalTileCount");
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
