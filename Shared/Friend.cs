using System;
using System.ComponentModel;
using System.IO;
using Shared.Serialization;

#if CLIENT
using Windows.UI.Core;
#endif

namespace Ozwego.BuddyManagement
{
    /// <summary>
    /// Represents an end-user of the application.
    /// </summary>
    public class Friend : INotifyPropertyChanged, IBinarySerializable
    {
        protected string _alias;

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


        private string _emailAddress;

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

        private DateTime _creationTime;

        public DateTime CreationTime
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
        private int _ranking;

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
        protected int _level;

        public int Level
        {
            get
            {
                return _level;
            }

            set
            {
                if (value != _level)
                {
                    _level = value;
                    NotifyPropertyChanged("Level");
                }
            }
        }

        protected long _experience;

        public long Experience
        {
            get
            {
                return _experience;
            }

            set
            {
                if (value != _experience)
                {
                    _experience = value;
                    NotifyPropertyChanged("Experience");
                }
            }
        }


        public Friend()
        {
        }

        public Friend(string accountAddress)
        {
            EmailAddress = accountAddress;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private async void NotifyPropertyChanged(String propertyName)
        {
#if CLIENT
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                        handler(this, new PropertyChangedEventArgs(propertyName)));
            }
#endif
        }


        public void Write(BinaryWriter writer)
        {
            writer.Write(_alias);
            writer.Write(_emailAddress);
            writer.Write(_creationTime);
            writer.Write(_ranking);
            writer.Write(_level);
            writer.Write(_experience);
        }


        public void Read(BinaryReader reader)
        {
            _alias = reader.ReadString();
            _emailAddress = reader.ReadString();
            _creationTime = reader.ReadDateTime();
            _ranking = reader.ReadInt32();
            _level = reader.ReadInt32();
            _experience = reader.ReadInt64();
        }
    }
}
