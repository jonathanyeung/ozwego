using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.ViewModels
{
    class PlayerPaneViewModel : INotifyPropertyChanged
    {
        public PlayerPaneViewModel(string playerName)
        {
            PlayerName = playerName;
            ActionString = "No Action";
        }


        private string _playerName;

        public string PlayerName
        {
            get
            {
                return _playerName;
            }

            set
            {
                if (value != _playerName)
                {
                    _playerName = value;
                    NotifyPropertyChanged("PlayerName");
                }
            }
        }

        private string _actionString;

        public string ActionString
        {
            get
            {
                return _actionString;
            }

            set
            {
                if (value != _actionString)
                {
                    _actionString = value;
                    NotifyPropertyChanged("ActionString");
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
