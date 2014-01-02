using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.UI.MainPage
{
    public enum MainPageView
    {
        MainMenu,
        Matchmaking,
        FriendChallenge
    }

    public class MainPageNavigationArgs
    {
        private MainPageView _mainPageView;

        public MainPageNavigationArgs(MainPageView desiredView)
        {
            _mainPageView = desiredView;
        }

        public MainPageView PageView
        {
            get
            {
                return _mainPageView;
            }

            set
            {
                _mainPageView = value;
            }
        }


    }
}
