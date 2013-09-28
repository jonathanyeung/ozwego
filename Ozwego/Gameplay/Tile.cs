using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay
{
    /// <summary>
    /// Represents a single game tile
    /// </summary>
    public class Tile
    {
        public string TileContents;

        public Tile()
        {
            TileContents = "";
        }

        public Tile(string tileContents)
        {
            TileContents = tileContents;
        }
    }
}
