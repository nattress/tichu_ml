namespace ToyAI.Tests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TichuAI;

    [TestClass]
    public class SixNimmtTests
    {
        private readonly Random random;
        private readonly SixNimmtGameState gameState;
        
        // [TestInitialize]
        public SixNimmtTests()
        {
            this.random = new Random(0xbeef);
            SixNimmtDeck deck = SixNimmtDeck.Create(random);
            this.gameState = SixNimmtGameState.Create(random, deck, playerCount:5, proMode:false);
        }

        private void UseEvenlyDistributedStartingState()
        {
            int[][] board =
            {
                new[] { 55 },
                new[] { 56 },
                new[] { 57 },
                new[] { 58 },
            };

            int[][] hands =
            {
                new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 },
                new[] { 11, 21, 31, 41, 51, 61, 71, 81, 91, 101 },
                new[] { 12, 22, 32, 42, 52, 62, 72, 82, 92, 102 },
                new[] { 13, 23, 33, 43, 53, 63, 73, 83, 93, 103 },
                new[] { 14, 24, 34, 44, 54, 64, 74, 84, 94, 104 }
            };

            for (int player = 0; player < 5; ++player)
            {
                for (int cardIndex = 0; cardIndex < 10; ++cardIndex)
                {
                    this.gameState.DealCard(player, hands[player][cardIndex]);
                }
            }
            
            for (int row = 0; row < 4; ++row)
            {
                for (int rowPosition = 0; rowPosition < board[row].Length; ++rowPosition)
                {
                    this.gameState.AddStartingCard(row, board[row][rowPosition]);
                }
            }
        }

        /// <summary>
        /// Test that playing 6 cards on a row takes it
        /// </summary>
        [TestMethod]
        public void TestRowTake()
        {
            UseEvenlyDistributedStartingState();
            
            var player0Plays = this.gameState.GetPlays();
            CollectionAssert.AreEqual(new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }, player0Plays.ToArray());
            this.gameState.CommitPlay(60);
            this.gameState.CommitPlay(61);
            this.gameState.CommitPlay(62);
            this.gameState.CommitPlay(63);
            this.gameState.CommitPlay(64);

            //  1   2   3   4   5  |  X
            // -------------------------
            // 55
            // 56
            // 57
            // 58  60  61  62  63  | 64 <-- Player 4 takes 7 points
            ValidateScores(this.gameState, new int[] {66, 66, 66, 66, 59});
            Assert.IsTrue(this.gameState.CurrentPlayerTurn == 0);
            Assert.IsTrue(this.gameState.PlayInputState == SixNimmtInputState.SelectCard);
        }

        [TestMethod]
        public void TestLowCardRowTakeChoice()
        {
            UseEvenlyDistributedStartingState();
            
            this.gameState.CommitPlay(50);
            this.gameState.CommitPlay(101);
            this.gameState.CommitPlay(102);
            this.gameState.CommitPlay(103);
            this.gameState.CommitPlay(104);
            Assert.IsTrue(this.gameState.PlayInputState == SixNimmtInputState.TakeRow);
            CollectionAssert.AreEqual(new[] {0, 1, 2, 3}, this.gameState.GetPlays().ToArray());
            //  1   2   3   4   5  |  X
            // -------------------------
            // 55
            // 56
            // 57
            // 58
            //
            // Played: 50, 101, 102, 103, 104
            //
            //  1   2   3   4   5  |  X
            // -------------------------
            // 50                      <-- Player 0 takes row 0 and loses 7 points
            // 56
            // 57
            // 58  101 102 103 104 |
            this.gameState.CommitPlay(0);
            ValidateScores(this.gameState, new int[] {59, 66, 66, 66, 66});
        }

        [TestMethod]
        public void TestNotGameOverAtBeginning()
        {
            UseEvenlyDistributedStartingState();
            Assert.IsFalse(this.gameState.GameOver());
        }

        [TestMethod]
        public void TestGameOver()
        {
            UseEvenlyDistributedStartingState();
            
            CommitPlays(this.gameState, new int[] {10, 11, 12, 13, 14});
            // Take row 0
            // {
            //     0 55 0 0 0 0 (7 pts)
            //     1 56 0 0 0 0 (1 pts)
            //     2 57 0 0 0 0 (1 pts)
            //     3 58 0 0 0 0 (1 pts)
            //     Scores: 
            //     0: 66
            //     1: 66
            //     2: 66
            //     3: 66
            //     4: 66
            //     Current round cards: 10,11,12,13,14
            // }
            this.gameState.CommitPlay(0);
            ValidateScores(this.gameState, new int[] {59, 66, 66, 66, 66});
            Assert.IsFalse(this.gameState.GameOver());
            // Take row 0
            // {
            //     0 10 11 12 13 14 (7 pts)
            //     1 56 0 0 0 0 (1 pts)
            //     2 57 0 0 0 0 (1 pts)
            //     3 58 0 0 0 0 (1 pts)
            //     Scores: 
            //     0: 59
            //     1: 66
            //     2: 66
            //     3: 66
            //     4: 66
            //     Current round cards: 10,11,12,13,14
            // }
            
        }

        /// <summary>
        /// Batch a set of plays
        /// </summary>
        private static void CommitPlays(SixNimmtGameState gameState, int[] plays)
        {
            foreach (int play in plays)
            {
                gameState.CommitPlay(play);
            }
        }

        /// <summary>
        /// Tests the scores in "gameState" against expectedScores
        /// </summary>
        private static void ValidateScores(SixNimmtGameState gameState, int[] expectedScores)
        {
            int[] scores = gameState.Evaluate().Select(x => (int)x).ToArray();
            CollectionAssert.AreEqual(expectedScores, scores);
        }
    }
}
