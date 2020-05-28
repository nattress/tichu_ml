using System;
using System.IO;

namespace TichuAI
{
    class Program
    {
        static void Main(string[] args)
        {
            //int i = 7462;
            // for (int i = 1; i < 2000000; i++)
            // {
            //     using (StreamReader reader = File.OpenText($@"c:\repro\tichuhands\{i}.txt"))
            //     {
            //         var game = GameParser.ParseGame(reader);
            //         //Console.WriteLine($"{i}.txt {game.Hands.Count} games");
            //     }
            // }

            // int numGames = 1;
            int numGames = 100;
            int[] cumulativeTrickCounts = new int[4]; // [playerId]
            IPlayGenerator[] playGenerators = new IPlayGenerator[4];
            playGenerators[0] = new Mcts();
            playGenerators[1] = new RandomPlayGenerator();
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
    }
}
