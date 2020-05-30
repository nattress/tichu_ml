using System;
using System.IO;

namespace TichuAI
{
    class Program
    {
        static void Main(string[] args)
        {
            // TicTacToeHarness.Run(100);
            GameRunHarness.Run(100);

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
