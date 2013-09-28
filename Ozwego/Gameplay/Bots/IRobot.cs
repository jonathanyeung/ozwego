using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay.Bots
{
    public interface IRobot : IPlayer
    {

        int Level
        {
            get;
            set;
        }

        /// <summary>
        /// Method that tells the bot to begin firing peel and dump actions.
        /// </summary>
        void StartBot();


        /// <summary>
        /// Method that tells the bot to stop all of its actions.
        /// </summary>
        void StopBot();

    }
}
