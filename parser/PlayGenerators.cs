using System;

namespace TichuAI
{
    public class RandomPlayGenerator : IPlayGenerator
    {
        public Play FindPlay(IGameState gameState)
        {
            return gameState.GetRandomPlay();
        }
    }

    public class HighestCardPlayGenerator : IPlayGenerator
    {
        public Play FindPlay(IGameState gameState)
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