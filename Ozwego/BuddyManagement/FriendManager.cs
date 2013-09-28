using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;

namespace Ozwego.BuddyManagement
{
    public class FriendManager
    {
        private static FriendManager _instance;
        private List<Buddy> CompleteFriendList;

        public readonly ObservableCollection<Buddy> OnlineFriendList;
        public readonly ObservableCollection<Buddy> OfflineFriendList;
        public readonly ObservableCollection<Buddy> FriendSearchResultsList;

        /// <summary>
        /// Private Constructor
        /// </summary>
        private FriendManager()
        {
            CompleteFriendList = new List<Buddy>();
            FriendSearchResultsList = new ObservableCollection<Buddy>();
            OfflineFriendList = new ObservableCollection<Buddy>();
            OnlineFriendList = new ObservableCollection<Buddy>();

            //ToDo: Remove temporary code.
            //for (int i = 0; i < 10; i++)
            //{
            //    var tempBuddy = new Buddy()
            //    {
            //        Alias = "GoodFriend" + i.ToString(),
            //        EmailAddress = "GoodFriend" + i.ToString() + "@address.com"
            //    };

            //    CompleteFriendList.Add(tempBuddy);
            //    OfflineFriendList.Add(tempBuddy);

            //    if (i <= 5)
            //    {
            //        OnBuddySignIn(tempBuddy);
            //    }
            //}
        }


        /// <summary>
        /// Public method to instantiate ServerMessageSender singleton.
        /// </summary>
        /// <returns></returns>
        public static FriendManager GetInstance()
        {
            return _instance ?? (_instance = new FriendManager());
        }


        /// <summary>
        /// Used on first sign in; populates the buddy list of all friends both online and offline.
        /// </summary>
        /// <param name="buddyList"></param>
        public void InitializeCompleteBuddyList(List<Buddy> buddyList)
        {
            CompleteFriendList.Clear();

            foreach (var buddy in buddyList)
            {
                CompleteFriendList.Add(buddy);
                OnBuddySignOut(buddy);
            }
        }


        /// <summary>
        /// Used on first sign in; populates the buddy list of all are online.
        /// </summary>
        /// <param name="buddyList"></param>
        public void InitializeOnlineBuddyList(List<Buddy> buddyList)
        {
            foreach (var buddy in buddyList)
            {
                if (null != buddy)
                {
                    OnBuddySignIn(buddy);
                }
            }
        }


        /// <summary>
        /// Used when a friend request is accepted while the user is still online.
        /// </summary>
        /// <param name="buddy"></param>
        public void AddNewBuddy(Buddy buddy)
        {
            CompleteFriendList.Add(buddy);
            OnBuddySignIn(buddy);
        }



        /// <summary>
        /// Buddy signed in, add him to the buddy list.
        /// </summary>
        /// <param name="buddy"></param>
        public void OnBuddySignIn(Buddy buddy)
        {
            if (buddy == null)
            {
                return;
            }

            var buddyToRemove = OfflineFriendList.FirstOrDefault((Buddy b) => b.EmailAddress == buddy.EmailAddress);

            if (null != buddyToRemove)
            {
                OfflineFriendList.Remove(buddyToRemove);
            }

            if (OnlineFriendList.FirstOrDefault((Buddy b) => b.EmailAddress == buddy.EmailAddress) == null)
            {
                OnlineFriendList.Add(buddy);
            }
        }


        /// <summary>
        /// Buddy signed out, move him from the online list to the offline list.
        /// </summary>
        /// <param name="buddy"></param>
        public void OnBuddySignOut(Buddy buddy)
        {
            if (buddy == null)
            {
                return;
            }

            var buddyToRemove = OnlineFriendList.FirstOrDefault((Buddy b) => b.EmailAddress == buddy.EmailAddress);

            if (null != buddyToRemove)
            {
                OnlineFriendList.Remove(buddyToRemove);
            }

            if (OfflineFriendList.FirstOrDefault((Buddy b) => b.EmailAddress == buddy.EmailAddress) == null)
            {
                OfflineFriendList.Add(buddy);
            }
        }


        /// <summary>
        /// Buddy signed in, move him from the offline list to the online list.
        /// </summary>
        /// <param name="buddy"></param>
        public void AddSearchResults(List<Buddy> results)
        {
            FriendSearchResultsList.Concat(results);

            if (FriendSearchResultsList.Count == 0)
            {
                FriendSearchResultsList.Add(new Buddy { Alias = "No Results" });
            }
        }


    }
}
