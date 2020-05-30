using System;

namespace TichuAI
{
    /// <summary>
    /// Provides an abstraction over different styles of Play generation heuristics
    /// </summary>
    public interface IPlayGenerator<Move>
    {
        Move FindPlay(IGameState<Move> gameState);
    }
}