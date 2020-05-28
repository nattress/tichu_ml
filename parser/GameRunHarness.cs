using System;

namespace TichuAI
{
    /// <summary>
    /// Runs a series of games with configured AI agents and measures the win statistics
    /// of each agent.
    /// </summary>
    public class GameRunHarness
    {
        public static GameState SetupFourPlayerGame()
        {
            Deck deck = Deck.CreateWithoutSpecials();
            GameState gameState = new GameState(deck);

            Random random = new Random();
            gameState.SetCurrentPlayer(random.Next(4));
            gameState.Players = new PlayerState[4];
            for (int i = 0; i < 4; i++)
                gameState.Players[i] = new PlayerState();

            int player = 0;
            while (deck.Count > 0)
            {
                gameState.Players[player].Cards.Add(deck.DealCard());
                player = (player + 1) % 4;
            }
            
            return gameState;
        }
    }
}