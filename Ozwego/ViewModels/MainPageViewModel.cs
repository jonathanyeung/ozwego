using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Ozwego.BuddyManagement;

namespace Ozwego.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private static MainPageViewModel _instance;

        //public ObservableCollection<Buddy> BuddyList;

        public ObservableCollection<string> ChatMessages = new ObservableCollection<string>();

        /// <summary>
        /// A list of all of the people in the room.
        /// </summary>
        //public ObservableCollection<Buddy> RoomMembers;

        private MainPageViewModel()
        {
            //FriendListManager = new ObservableCollection<Buddy>();
            //RoomMembers = new ObservableCollection<Buddy>();

            //ToDo: Remove these temporary names:

            //RoomMembers.Add(new Buddy("Michael Jordan"));
            //RoomMembers.Add(new Buddy("Charles Barkley"));
            //RoomMembers.Add(new Buddy("Derrick Rose"));
            //RoomMembers.Add(new Buddy("Marc Gasol"));
            //RoomMembers.Add(new Buddy("Father of Grind"));
            //RoomMembers.Add(new Buddy("Metta World Peace"));
        }

        /// <summary>
        /// Public method to instantiate ServerMessageReceiver singleton.
        /// </summary>
        /// <returns></returns>
        public static MainPageViewModel GetInstance()
        {
            return _instance ?? (_instance = new MainPageViewModel());
        }

        //
        // Shows whether the client is connected to the Ozwego server or not.
        //

        private bool _connectionStatus;

        public bool ConnectionStatus
        {
            get
            {
                return _connectionStatus;
            }

            set
            {
                if (value != _connectionStatus)
                {
                    _connectionStatus = value;
                    NotifyPropertyChanged("ConnectionStatus");
                }
            }
        }

        private string _userName;

        public string UserName
        {
            get
            {
                return _userName;
            }

            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }


        private string _roomHost;

        public string RoomHost
        {
            get
            {
                return _roomHost;
            }

            set
            {
                if (value != _roomHost)
                {
                    _roomHost = value;
                    NotifyPropertyChanged("RoomHost");
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
