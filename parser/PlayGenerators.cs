using System;

namespace TichuAI
{
    /// <summary>
    /// Random generator uses only public surface area of IGameState&lt;T&gt; and can be
    /// used for most move generation purposes.
    /// </summary>
    public class RandomPlayGenerator<Move> : IPlayGenerator<Move>
    {
        public Move FindPlay(IGameState<Move> gameState)
        {
            return gameState.GetRandomPlay();
        }
    }

    public class HighestCardPlayGenerator : IPlayGenerator<Play>
    {
        public Play FindPlay(IGameState<Play> gameState)
        {
            //var plays = gameState.GetPlays();
            CardRank highestRank = CardRank.Two;
            Play highestPlay = null;
            foreach (var play in gameState.GetPlays())
            {
                if (highestPlay == null || play.Cards[0].Rank > highestRank)
                {
                    highestPlay = play;
                    highestRank = play.Cards[0].Rank;
                }
            }
            return highestPlay;
        }
    }
}