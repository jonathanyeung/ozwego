using Ozwego.Storage;

namespace Ozwego.Gameplay
{
    public class PostGamePageNavigationArgs
    {
        public GameConnectionType GameConnectionType;
        public GameMode GameMode;
        public GameData GameData;

        //ToDo: Add some more fields here that contain information about the last game board; stats; etc.
    }
}
