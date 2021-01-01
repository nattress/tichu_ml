using System;
using System.Collections.Generic;
using System.Linq;

namespace TichuAI
{
    public class SixNimmtSuggestMoveHarness
    {
        private const int PlayerCount = 5;
        public static void Run()
        {
            Random random = new Random();
            IPlayGenerator<int>[] playGenerators = new IPlayGenerator<int>[PlayerCount];
            playGenerators[0] = new Mcts<int>(10000, 20, random);
            playGenerators[1] = new ConsoleInputSixNimmtPlayGenerator();
            playGenerators[2] = new ConsoleInputSixNimmtPlayGenerator();
            playGenerators[3] = new ConsoleInputSixNimmtPlayGenerator();
            playGenerators[4] = new ConsoleInputSixNimmtPlayGenerator();
            Logger.Log.Enabled = true;
            while (true)
            {
                double[] scores = null;
                while (true)
                {
                    var state = CreateNewGame(random, scores);

                    if (true)
                    {
                        Logger.Log.WriteLine("Starting cards:");
                        var playerCards = state.PlayerCards[0].ToList();
                        playerCards.Sort();
                        Logger.Log.WriteLine($"Player {0} : {string.Join(" ", playerCards)}");
                    }

                    while (true)
                    {
                        if (state.CurrentPlayerTurn == 0 && state.PlayInputState == SixNimmtInputState.CardSelection)
                        {
                            Logger.Log.WriteLine(state.ToString());
                        }
                        state.SetPointOfViewPlayer(state.CurrentPlayerTurn);
                        var play = playGenerators[state.CurrentPlayerTurn].FindPlay(state);
                        state.CommitPlay(play);
                        if (state.GameOver())
                            break;
                    }

                    scores = state.Evaluate();
                    if (scores.Any( x => x < 0))
                    {
                        // Somebody went below 0 - game over
                        double highestScore = double.MinValue;
                        foreach (var s in scores)
                        {
                            if (s > highestScore)
                                highestScore = s;
                        }
                        
                        int winningPlayer = 0;
                        for (int i = 0; i < PlayerCount; i++)
                        {
                            if (scores[i] == highestScore)
                            {
                                winningPlayer = i;
                            }
                        }

                        Console.WriteLine($"Game over. Winning player number: {winningPlayer}. Highest score is {highestScore}");
                        break;
                    }
                }
            }
        }

        static SixNimmtGameState CreateNewGame(Random random, double[] scores)
        {
            SixNimmtDeck deck = SixNimmtDeck.Create();
            SixNimmtGameState state = SixNimmtGameState.Create(random, deck, PlayerCount);

            if (scores != null)
            {
                state.SetInitialScores(scores);
            }

            // Get pov player's hand
            while (true)
            {
                Console.WriteLine("Enter hand (ie, '1 3 43 104')");
                string handInput = Console.ReadLine();
                string[] handInputs = handInput.Split(" ");
                if (handInputs.Length != 10)
                {
                    Console.WriteLine($"Enter 10 cards. I counted {handInput.Length}");
                    continue;
                }

                for (int i = 0; i < handInputs.Length; i++)
                {
                    state.DealCard(0, int.Parse(handInputs[i]));
                }
                break;
            }
            
            state.SetCurrentPlayer(0);

            while (true)
            {
                Console.WriteLine("Enter starting row cards top to bottom (ie, '43 32 45 98')");
                string rowsInput = Console.ReadLine();
                string[] rowsInputs = rowsInput.Split(" ");
                if (rowsInputs.Length != SixNimmtGameState.BoardRowCount)
                {
                    Console.WriteLine($"Enter {SixNimmtGameState.BoardRowCount} cards. I counted {rowsInputs.Length}");
                    continue;
                }

                for (int i = 0; i < SixNimmtGameState.BoardRowCount; i++)
                {
                    state.AddStartingCard(i, int.Parse(rowsInputs[i]));
                }
                break;
            }
            
            return state;
        }
    }
}