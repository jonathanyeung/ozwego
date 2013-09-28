using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozwego.Gameplay
{
    /// <summary>
    /// Interface for a local player in the game.  This can either be a bot or the real end-user.
    /// </summary>
    public interface IPlayer
    {
        string Alias
        {
            get;
            set;
        }

        /// <summary>
        /// Method invoked to initialize the player object for a new game.
        /// </summary>
        void InitializeForGame();


        /// <summary>
        /// Method invoked when a tile gets played by the player
        /// </summary>
        void OnTilePlayed();


        /// <summary>
        /// Method invoked when the player performs a dump
        /// </summary>
        void PerformDumpAction();


        /// <summary>
        /// Method invoked when the player performs a peel
        /// </summary>
        void PerformPeelAction();


        /// <summary>
        /// Method called when someone else (not this player) has performed a peel.
        /// </summary>
        void PeelActionReceived(Tile incomingTile);


        /// <summary>
        /// Method invokoed when the player has won the game.
        /// </summary>
        void SignalVictory();


        /// <summary>
        /// Gets the current hand size of the player.
        /// </summary>
        int GetCurrentHandSize();

    }
}
