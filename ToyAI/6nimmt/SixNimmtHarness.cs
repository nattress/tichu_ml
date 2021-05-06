using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TichuAI
{
    internal class ConsoleInputSixNimmtPlayGenerator : IPlayGenerator<int>
    {
        // Human player input
        public int FindPlay(IGameState<int> iGameState)
        {
            SixNimmtGameState gameState = iGameState as SixNimmtGameState;
            while(true)
            {
                Console.WriteLine(gameState.ToString());
                List<int> playsList = new List<int>(gameState.GetPlays());
                playsList.Sort();
                string plays = string.Join(", ", playsList.ToArray());
                if (gameState.PlayInputState == SixNimmtInputState.SelectCard)
                {
                    // string cards = string.Join(", ", gameState.PlayerCards[gameState.CurrentPlayerTurn].ToArray());
                    Console.WriteLine($"Enter a play. Cards: {plays}");
                }
                else
                {
                    // string plays = string.Join(", ", gameState.GetPlays().ToArray());
                    Console.WriteLine($"Choose a row to take: {plays}");
                }
                
                string playInput = Console.ReadLine();
                if (string.IsNullOrEmpty(playInput))
                {
                    Console.WriteLine("Invalid play. Try again.");
                    continue;
                }
                
                if (!int.TryParse(playInput, out int attemptedPlay))
                {
                    Console.WriteLine("Invalid play. Input must be a number.");
                    continue;
                }

                if (!gameState.GetPlays().Contains(attemptedPlay))
                {
                    Console.WriteLine($"Chosen play '{attemptedPlay}' is not valid.");
                    continue;
                }
                return attemptedPlay;
            }
        }
    }

    /// <summary>
    /// Runs iterations of MCTS with TTT
    /// </summary>
    public class SixNimmtHarness
    {
        private const int PlayerCount = 5;
        private const bool ProMode = true;
        public static void Run(int iterations)
        {
            Random random = new Random();
            int[] wins = new int[PlayerCount];
            int draws = 0;
            IPlayGenerator<int>[] playGenerators = new IPlayGenerator<int>[PlayerCount];
            // playGenerators[0] = new ConsoleInputSixNimmtPlayGenerator();
            // playGenerators[1] = new RandomPlayGenerator<int>();
            // playGenerators[2] = new RandomPlayGenerator<int>();
            // playGenerators[3] = new RandomPlayGenerator<int>();
            // playGenerators[4] = new RandomPlayGenerator<int>();

            playGenerators[0] = new Mcts<int>(numIterations: 2000, simulationDepth: 10, random);
            playGenerators[1] = new RandomPlayGenerator<int>();
            playGenerators[2] = new RandomPlayGenerator<int>();
            playGenerators[3] = new RandomPlayGenerator<int>();
            playGenerators[4] = new RandomPlayGenerator<int>();
            Logger.Log.Enabled = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                double[] scores = null;
                while (true)
                {
                    var state = CreateNewGame(random, scores, SixNimmtHarness.ProMode);

                    if (true)
                    {
                        Logger.Log.WriteLine("Starting cards:");
                        for (int i = 0; i < PlayerCount; i++)
                        {
                            var playerCards = state.PlayerCards[i].ToList();
                            playerCards.Sort();
                            
                            Logger.Log.WriteLine($"Player {i} : {string.Join(" ", playerCards)}");
                        }
                    }

                    while (true)
                    {
                        if (state.CurrentPlayerTurn == 0 && state.PlayInputState == SixNimmtInputState.SelectCard)
                        {
                            Logger.Log.WriteLine(state.ToString());
                        }
                        state.SetPointOfViewPlayer(state.CurrentPlayerTurn);
                        var play = playGenerators[state.CurrentPlayerTurn].FindPlay(state);
                        // Console.WriteLine($"Player {state.CurrentPlayerTurn} plays {play}");
                        state.CommitPlay(play);
                        if (state.GameOver())
                            break;
                    }

                    scores = state.Evaluate();
                    if (scores.Any(x => x < 0))
                    {
                        // Somebody went below 0 - game over
                        double highestScore = double.MinValue;
                        foreach (var s in scores)
                        {
                            if (s > highestScore)
                                highestScore = s;
                        }

                        for (int i = 0; i < PlayerCount; i++)
                        {
                            if (scores[i] == highestScore)
                            {
                                wins[i]++;
                            }
                        }
                        break;
                    }
                }
            }

            string header = string.Format("{0, 6}{1, 10}{2, 7}", "Player", "Wins", "%");
            Console.WriteLine(header);

            for (int i = 0; i < PlayerCount; i++)
            {
                string output = string.Format("{0, 6}{1, 10}{2, 7:P1}", i, wins[i], (double)wins[i] / (iterations - draws));
                Console.WriteLine(output);
            }
            Console.WriteLine($"Draws: {draws}");
            stopwatch.Stop();
            Console.WriteLine($"Finished after {stopwatch.ElapsedMilliseconds:N0}ms.");
        }

        static SixNimmtGameState CreateNewGame(Random random, double[] scores, bool proMode)
        {
            SixNimmtDeck deck = SixNimmtDeck.Create(random);
            SixNimmtGameState state = SixNimmtGameState.Create(random, deck, PlayerCount, proMode);

            if (scores != null)
            {
                state.SetInitialScores(scores);
            }

            for (int player = 0; player < PlayerCount; player++)
            {
                for (int i = 0; i < 10; i++)
                {
                    state.DealCard(player, deck.DealCard());
                }
            }
            
            state.SetCurrentPlayer(0);

            // Give each row a starting card
            for (int i = 0; i < SixNimmtGameState.BoardRowCount; i++)
            {
                state.AddStartingCard(i, deck.DealCard());
            }

            return state;
        }
    }
}