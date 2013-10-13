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


        // Player Ranking.  This is monotonically increasing, increases with experience and is not
        // an indicator of how good a player is.
        [IgnoreDataMember]
        protected int _ranking;

        [DataMember(Name = "Ranking")]
        public int Ranking
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


        // Skill level is NOT monotonically increasing.  This is meant to be an indicator of how
        // good a player actually is, and it varies with win/loss ratio.
        [IgnoreDataMember]
        protected int _skillLevel;

        [DataMember(Name = "SkillLevel")]
        public int SkillLevel
        {
            get
            {
                return _skillLevel;
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
