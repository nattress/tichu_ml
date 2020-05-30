using System;

namespace TichuAI
{
    internal class ConsoleInputTicTacToePlayGenerator : IPlayGenerator<TicTacToePlay>
    {
        // Human player input
        public TicTacToePlay FindPlay(IGameState<TicTacToePlay> gameState)
        {
            string[] playerMarkers = new string[] { "O", "X" };
            while(true)
            {
                Console.WriteLine(gameState.ToString());
                Console.WriteLine($"Enter a play. Playing as {TicTacToeGameState.PlayerMarkers[gameState.CurrentPlayerTurn]}s");
                string playInput = Console.ReadLine();
                if (string.IsNullOrEmpty(playInput))
                {
                    Console.WriteLine("Invalid play. Try again.");
                    continue;
                }
                string[] parsedInput = playInput.Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (parsedInput.Length != 2 || !int.TryParse(parsedInput[0], out int row) || !int.TryParse(parsedInput[1], out int col))
                {
                    Console.WriteLine("Invalid play. Try again.");
                    continue;
                }
                var attemptedPlay = new TicTacToePlay(row, col);
                if (!gameState.GetPlays().Contains(attemptedPlay))
                {
                    Console.WriteLine("That spot is taken. Try again.");
                    continue;
                }
                return attemptedPlay;
            }
        }
    }

    /// <summary>
    /// Runs iterations of MCTS with TTT
    /// </summary>
    public class TicTacToeHarness
    {
        public static void Run(int iterations)
        {
            Random random = new Random();
            int[] wins = new int[2];
            int draws = 0;
            IPlayGenerator<TicTacToePlay>[] playGenerators = new IPlayGenerator<TicTacToePlay>[2];
            playGenerators[0] = new Mcts<TicTacToePlay>(10000, 20);
            // playGenerators[1] = new RandomPlayGenerator<TicTacToePlay>();
            // playGenerators[0] = new ConsoleInputTicTacToePlayGenerator();
            // playGenerators[0] = new RandomPlayGenerator<TicTacToePlay>();
            playGenerators[1] = new Mcts<TicTacToePlay>(10000, 20);
            //Logger.Log.Enabled = true;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                var state = CreateNewGame(random, 1);

                while (true)
                {
                    state.SetPointOfViewPlayer(state.CurrentPlayerTurn);
                    var play = playGenerators[state.CurrentPlayerTurn].FindPlay(state);
                    state.CommitPlay(play);
                    if (state.GameOver())
                        break;
                }

                var scores = state.Evaluate();
                if (scores[0] == scores[1])
                {
                    draws++;
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        wins[i] += scores[i] > scores[1-i] ? 1 : 0;
                    }
                }
            }

            string header = string.Format("{0, 6}{1, 10}{2, 7}", "Player", "Wins", "%");
            Console.WriteLine(header);

            for (int i = 0; i < 2; i++)
            {
                string output = string.Format("{0, 6}{1, 10}{2, 7:P1}", i, wins[i], (double)wins[i] / (iterations - draws));
                Console.WriteLine(output);
            }
            Console.WriteLine($"Draws: {draws}");
        }

        static TicTacToeGameState CreateNewGame(Random random, int startingPlayer = -1)
        {
            TicTacToeGameState state = new TicTacToeGameState();
            state.SetCurrentPlayer(startingPlayer == -1 ? random.Next(2) : startingPlayer);
            return state;
        }
    }
}