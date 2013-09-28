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
        protected string _alias;

        [DataMember(Name = "Alias")]
        public string Alias
        {
            get
            {
                return _alias;
            }

            set
            {
                if (value != _alias)
                {
                    _alias = value;
                    NotifyPropertyChanged("Alias");
                }
            }
        }

        [IgnoreDataMember]
        private string _emailAddress;

        [DataMember(Name = "EmailAddress")] 
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }

            set
            {
                if (value != _emailAddress)
                {
                    _emailAddress = value;
                    NotifyPropertyChanged("EmailAddress");
                }
            }
        }


        //[IgnoreDataMember]
        //private string _roomGuid;

        //[DataMember(Name = "RoomIdentifier")] 
        //public string RoomGuid
        //{
        //    get
        //    {
        //        return _roomGuid;
        //    }

        //    set
        //    {
        //        if (value != _roomGuid)
        //        {
        //            _roomGuid = value;
        //            NotifyPropertyChanged("RoomIdentifier");
        //        }
        //    }
        //}


        //
        // The time that the account was created.
        //

        [IgnoreDataMember]
        private string _creationTime;

        [DataMember(Name = "CreationTime")]
        public string CreationTime
        {
            get
            {
                return _creationTime;
            }

            set
            {
                if (value != _creationTime)
                {
                    _creationTime = value;
                    NotifyPropertyChanged("CreationTime");
                }
            }
        }


        [IgnoreDataMember]
        protected string _ranking;

        [DataMember(Name = "Ranking")]
        public string Ranking
        {
            get
            {
                return _ranking;
            }

            set
            {
                if (value != _ranking)
                {
                    _ranking = value;
                    NotifyPropertyChanged("Ranking");
                }
            }
        }


        public Buddy()
        {
        }

        public Buddy(string _accountAddress)
        {
            EmailAddress = _accountAddress;
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
