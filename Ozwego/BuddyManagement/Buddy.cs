using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Windows.UI.Core;

namespace Ozwego.BuddyManagement
{
    /// <summary>
    /// Represents an end-user of the application.
    /// </summary>
    public class Buddy : INotifyPropertyChanged
    {
        [IgnoreDataMember]
        private string _ScreenName;

        [DataMember(Name = "ScreenName")]
        public string ScreenName
        {
            get
            {
                return _ScreenName;
            }

            set
            {
                if (value != _ScreenName)
                {
                    _ScreenName = value;
                    NotifyPropertyChanged("ScreenName");
                }
            }
        }

        [IgnoreDataMember]
        private string _MicrosoftAccountAddress;

        [DataMember(Name = "MicrosoftAccountAddress")] 
        public string MicrosoftAccountAddress
        {
            get
            {
                return _MicrosoftAccountAddress;
            }

            set
            {
                if (value != _MicrosoftAccountAddress)
                {
                    _MicrosoftAccountAddress = value;
                    NotifyPropertyChanged("MicrosoftAccountAddress");
                }
            }
        }

        [IgnoreDataMember] private string _roomGuid;

        [DataMember(Name = "RoomIdentifier")] 
        public string RoomGuid
        {
            get
            {
                return _roomGuid;
            }

            set
            {
                if (value != _roomGuid)
                {
                    _roomGuid = value;
                    NotifyPropertyChanged("RoomIdentifier");
                }
            }
        }

        public Buddy()
        {
        }

        public Buddy(string _accountAddress)
        {
            MicrosoftAccountAddress = _accountAddress;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }
}
