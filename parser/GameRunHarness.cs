using System;

namespace TichuAI
{
    /// <summary>
    /// Runs a series of games with configured AI agents and measures the win statistics
    /// of each agent.
    /// </summary>
    public class GameRunHarness
    {
        public static void Run(int numGames)
        {
            int[] cumulativeTrickCounts = new int[4]; // [playerId]
            IPlayGenerator<Play>[] playGenerators = new IPlayGenerator<Play>[4];
            playGenerators[0] = new Mcts<Play>(10000, 50);
            playGenerators[1] = new RandomPlayGenerator<Play>();
            playGenerators[2] = new HighestCardPlayGenerator();
            playGenerators[3] = new HighestCardPlayGenerator();
            for (int gameNumber = 0; gameNumber < numGames; gameNumber++)
            {
                var gameState = GameRunHarness.SetupFourPlayerGame();
                
                if (true)
                {
                    Logger.Log.WriteLine("Starting cards:");
                    for (int i = 0; i < 4; i++)
                    {
                        Logger.Log.WriteLine($"Player {i} {Card.PrintCardsSortedBySuit(gameState.Players[i].Cards)}");
                    }
                }
                while (true)
                {
                    gameState.SetPointOfViewPlayer(gameState.CurrentPlayerTurn);
                    Play play = playGenerators[gameState.CurrentPlayerTurn].FindPlay(gameState);
                    Logger.Log.WriteLine($"Player {play.Player} played {play.Cards[0].ToString()}");
                    gameState.CommitPlay(play);
                    if (gameState.GameOver())
                        break;

                    if (gameState.PlayedCards.Count % 4 == 0)
                        Logger.Log.WriteLine("----");
                }

                var scores = gameState.Evaluate();
                for (int i = 0; i < 4; i++)
                {
                    cumulativeTrickCounts[i] += (int)scores[i];
                }

                Console.WriteLine($"Tricks: {scores[0]} {scores[1]} {scores[2]} {scores[3]} ");
            }
            
            string header = string.Format("{0, 6}{1, 10}{2, 10}", "Player", "Tricks", "Average");
            Console.WriteLine(header);

            for (int i = 0; i < 4; i++)
            {
                string output = string.Format("{0, 6}{1, 10}{2, 10:N1}", i, cumulativeTrickCounts[i], (double)cumulativeTrickCounts[i] / numGames);
                Console.WriteLine(output);
            }
        }

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