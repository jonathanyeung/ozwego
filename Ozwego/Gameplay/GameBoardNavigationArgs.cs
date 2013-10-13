using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay
{
    public enum GameConnectionType
    {
        Local,
        Online
    }

    public enum GameMode
    {
        Matchmaking,
        Friendly
    }


    /// <summary>
    /// Class used to pass navigation arguments to the GameBoard Page
    /// </summary>
    public class GameBoardNavigationArgs
    {
        public GameConnectionType GameConnectionType;
        public GameMode GameMode;
        public int BotCount;
    }
}
