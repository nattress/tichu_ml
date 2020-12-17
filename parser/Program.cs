using System;
using System.IO;

namespace TichuAI
{
    class Program
    {
        static void Main(string[] args)
        {
            string game = "";
            if (args.Length > 0)
            {
                game = args[0];
            }

            if (game.Equals("6nimmt"))
            {
                SixNimmtHarness.Run(1);
            }
            else if (game.Equals("6nimmthelp", StringComparison.OrdinalIgnoreCase))
            {
                SixNimmtSuggestMoveHarness.Run();
            }
            
            // TicTacToeHarness.Run(100);
            // GameRunHarness.Run(100);

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
            
        }
    }
}
