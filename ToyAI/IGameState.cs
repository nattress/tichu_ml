using System;
using System.Collections.Generic;

namespace TichuAI
{
    public interface IGameState<Move>
    {
        /// <summary>
        /// Has the game reached an end state?
        /// </summary>
        bool GameOver();
        /// <summary>
        /// The player which an AI move selector is currently playing as. In games with information hidden 
        /// per player (such as a hand of cards), the POV player is allowed to make smart heuristic move choices,
        /// and the game state can be used to generate random moves for playouts.
        /// </summary>
        void SetPointOfViewPlayer(int player);
        /// <summary>
        /// Sets the player whose turn it currently is
        /// Todo: Maybe we only need this for the initial player selection
        /// </summary>
        void SetCurrentPlayer(int player);
        /// <summary>
        /// Returns a deep member-wise clone of the mutable parts of the game state
        /// </summary>
        IGameState<Move> Clone();
        /// <summary>
        /// Update the game state based on this play
        /// </summary>
        void CommitPlay(Move play);
        /// <summary>
        /// Returns the set of possible plays from the current state
        /// </summary>
        IList<Move> GetPlays();
        /// <summary>
        /// Returns a random play from the set of possible plays from the current state
        /// </summary>
        Move GetRandomPlay();
        /// <summary>
        /// Returns the scores for each player at the given game state
        /// </summary>
        double[] Evaluate();
        /// <summary>
        /// Returns the player id of 
        /// </summary>
        int CurrentPlayerTurn { get; }
    }
}