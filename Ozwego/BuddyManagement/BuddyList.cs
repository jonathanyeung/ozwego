using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;

namespace Ozwego.BuddyManagement
{
    public class BuddyList
    {
        private static BuddyList _instance;


        /// <summary>
        /// Private Constructor
        /// </summary>
        private BuddyList()
        {
        }


        /// <summary>
        /// Public method to instantiate ServerMessageSender singleton.
        /// </summary>
        /// <returns></returns>
        public static BuddyList GetInstance()
        {
            return _instance ?? (_instance = new BuddyList());
        }


        /// <summary>
        /// Used on first sign in; populates the buddy list of all people who are online.
        /// </summary>
        /// <param name="buddyList"></param>
        public void InitializeBuddyList(List<Buddy> buddyList)
        {
            foreach (var buddy in buddyList)
            {
                OnBuddySignIn(buddy);
            }
        }

        /// <summary>
        /// Buddy signed in, add him to the buddy list.
        /// </summary>
        /// <param name="buddy"></param>
        public async void OnBuddySignIn(Buddy buddy)
        {
            await App.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, 
                () => App.MainPageViewModel.BuddyList.Add(buddy));
        }


        /// <summary>
        /// Buddy signed out, remove him from the buddy list.
        /// </summary>
        /// <param name="buddy"></param>
        public async void OnBuddySignOut(Buddy buddy)
        {
            await App.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                    {
                        foreach (var b in App.MainPageViewModel.BuddyList
                            .Where(b => b.MicrosoftAccountAddress == buddy.MicrosoftAccountAddress))
                        {
                            App.MainPageViewModel.BuddyList.Remove(b);
                            break;
                        }
                    });
        }
    }
}
