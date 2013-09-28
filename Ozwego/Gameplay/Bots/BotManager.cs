using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay.Bots
{
    /// <summary>
    /// This class manages the bots that may or may not be playing against the user.
    /// </summary>
    public class BotManager
    {
        private List<IRobot> _botList;

        public List<IRobot> BotList
        {
            get
            {
                return _botList;
            }
        }

        /// <summary>
        /// Singleton, private constructor.
        /// </summary>
        private static BotManager _instance;

        private BotManager()
        {
            _botList = new List<IRobot>();
        }


        /// <summary>
        /// Public method to instantiate BotManager singleton.
        /// </summary>
        /// <returns></returns>
        public static BotManager GetInstance()
        {
            return _instance ?? (_instance = new BotManager());
        }


        /// <summary>
        /// Create a new bot and add it to the list of bots.
        /// </summary>
        /// <param name="botLevel"></param>
        public void CreateBot(int botLevel)
        {
            var newBot = new TimerBot(botLevel);
            BotList.Add(newBot);
            //App.RoomManager.AddMemberToRoom(newBot.Alias);
        }


        /// <summary>
        /// Tells the bots to start going.
        /// </summary>
        public void StartBots()
        {
            foreach (IRobot b in BotList)
            {
                b.StartBot();
            }
        }


        /// <summary>
        /// Turns off all of the bots.
        /// </summary>
        public void StopBots()
        {
            foreach (IRobot b in BotList)
            {
                b.StopBot();
            }
        }

    }
}
