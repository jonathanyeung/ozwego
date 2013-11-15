using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ozwego.Server;
using Windows.UI.Core;

namespace Ozwego.BuddyManagement
{
    public class FriendManager
    {
        private static FriendManager _instance;
        private List<Friend> CompleteFriendList;

        public readonly ObservableCollection<Friend> OnlineFriendList;
        public readonly ObservableCollection<Friend> OfflineFriendList;
        public readonly ObservableCollection<Friend> FriendSearchResultsList;

        /// <summary>
        /// Private Constructor
        /// </summary>
        private FriendManager()
        {
            CompleteFriendList = new List<Friend>();
            FriendSearchResultsList = new ObservableCollection<Friend>();
            OfflineFriendList = new ObservableCollection<Friend>();
            OnlineFriendList = new ObservableCollection<Friend>();
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
        /// Used on first sign in; populates the Friend list of all friends both online and offline.
        /// </summary>
        /// <param name="buddyList"></param>
        public void InitializeCompleteBuddyList(List<Friend> buddyList)
        {
            CompleteFriendList.Clear();

            foreach (var friend in buddyList)
            {
                CompleteFriendList.Add(friend);
                OnBuddySignOut(friend);
            }
        }


        /// <summary>
        /// Used on first sign in; populates the Friend list of all are online.
        /// </summary>
        /// <param name="buddyList"></param>
        public void InitializeOnlineBuddyList(List<Friend> buddyList)
        {
            foreach (var friend in buddyList)
            {
                if (null != friend)
                {
                    OnBuddySignIn(friend);
                }
            }
        }


        /// <summary>
        /// Used when a friend request is accepted while the user is still online.
        /// </summary>
        /// <param name="buddy"></param>
        public void AddNewBuddy(Friend buddy)
        {
            CompleteFriendList.Add(buddy);
            OnBuddySignIn(buddy);
        }



        /// <summary>
        /// Friend signed in, add him to the Friend list.
        /// </summary>
        /// <param name="buddy"></param>
        public void OnBuddySignIn(Friend buddy)
        {
            if (buddy == null)
            {
                return;
            }

            var buddyToRemove = OfflineFriendList.FirstOrDefault((Friend b) => b.EmailAddress == buddy.EmailAddress);

            if (null != buddyToRemove)
            {
                OfflineFriendList.Remove(buddyToRemove);
            }

            if (OnlineFriendList.FirstOrDefault((Friend b) => b.EmailAddress == buddy.EmailAddress) == null)
            {
                OnlineFriendList.Add(buddy);
            }
        }


        /// <summary>
        /// Friend signed out, move him from the online list to the offline list.
        /// </summary>
        /// <param name="buddy"></param>
        public void OnBuddySignOut(Friend buddy)
        {
            if (buddy == null)
            {
                return;
            }

            var buddyToRemove = OnlineFriendList.FirstOrDefault((Friend b) => b.EmailAddress == buddy.EmailAddress);

            if (null != buddyToRemove)
            {
                OnlineFriendList.Remove(buddyToRemove);
            }

            if (OfflineFriendList.FirstOrDefault((Friend b) => b.EmailAddress == buddy.EmailAddress) == null)
            {
                OfflineFriendList.Add(buddy);
            }
        }


        /// <summary>
        /// Friend signed in, move him from the offline list to the online list.
        /// </summary>
        /// <param name="buddy"></param>
        public void AddSearchResults(List<Friend> results)
        {
            FriendSearchResultsList.Concat(results);

            if (FriendSearchResultsList.Count == 0)
            {
                FriendSearchResultsList.Add(new Friend { Alias = "No Results" });
            }
        }


    }
}
