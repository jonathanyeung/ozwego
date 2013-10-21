using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay.Ranking
{
    public class PlayerRank
    {
        private int _rank;

        public int Rank
        {
            get
            {
                return _rank;
            }

            set
            {
                if (value < 1 || value > 100)
                {
                    throw new ArgumentException("Rank must be between 1 and 100");
                }

                _rank = value;
            }
        }
    }
}
