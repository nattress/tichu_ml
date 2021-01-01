using System;
using System.Collections.Generic;
using System.Text;

namespace TichuAI
{
    public class TicTacToePlay : IComparable
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public TicTacToePlay(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override string ToString()
        {
            return $"({Row}, {Col})";
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            TicTacToePlay other = obj as TicTacToePlay;
            if (Row != other.Row)
                return Row > other.Row ? 1 : -1;

            if (Col != other.Col)
                return Col > other.Col ? 1 : -1;
            return 0;
        }

        public override int GetHashCode()
        {
            return (Row << 16) ^ (Col << 8);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            TicTacToePlay other = obj as TicTacToePlay;
            if (Row != other.Row)
                return false;

            if (Col != other.Col)
                return false;
            return true;
        }
    }

    public class TicTacToeGameState : IGameState<TicTacToePlay>
    {
        public static string[] PlayerMarkers = new string[] { "O", "X" };
        int[][] _board; // [row][column] Unplayed cells are initialized to -1
        int _freeCells = 9;
        /// <summary>
        /// True if a player won the game without a draw
        /// </summary>
        bool _gameWon = false;
        int _winner = -1;
        Random _random = null;

        public TicTacToeGameState()
        {
            _board = new int[3][];
            for (int row = 0; row < 3; row++)
            {
                _board[row] = new int[3];
                for (int col = 0; col < 3; col++)
                {
                    _board[row][col] = -1;
                }
            }
        }

        public int CurrentPlayerTurn { get; private set; }

        public IGameState<TicTacToePlay> Clone()
        {
            TicTacToeGameState newState = new TicTacToeGameState();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    newState._board[row][col] = _board[row][col];
                }
            }
            newState._random = _random;
            newState._freeCells = _freeCells;
            newState._gameWon = _gameWon;
            newState._winner = _winner;
            newState.CurrentPlayerTurn = CurrentPlayerTurn;
            return newState;
        }

        public void CommitPlay(TicTacToePlay play)
        {
            _board[play.Row][play.Col] = CurrentPlayerTurn;
            _freeCells--;
            int c = CurrentPlayerTurn;
            if (
                // Horizontal wins
                _board[0][0] == c && _board[0][1] == c && _board[0][2] == c ||
                _board[1][0] == c && _board[1][1] == c && _board[1][2] == c ||
                _board[2][0] == c && _board[2][1] == c && _board[2][2] == c ||
                // Vertical wins
                _board[0][0] == c && _board[1][0] == c && _board[2][0] == c ||
                _board[0][1] == c && _board[1][1] == c && _board[2][1] == c ||
                _board[0][2] == c && _board[1][2] == c && _board[2][2] == c ||
                // Diagonal wins
                _board[0][0] == c && _board[1][1] == c && _board[2][2] == c ||
                _board[0][2] == c && _board[1][1] == c && _board[2][0] == c)
            {
                _gameWon = true;
                _winner = CurrentPlayerTurn;
            }

            CurrentPlayerTurn = 1 - CurrentPlayerTurn;
        }

        public double[] Evaluate()
        {
            double[] scores = new double[2];
            if (_gameWon)
            {
                scores[_winner] = 1;
                scores[1 - _winner] = 0;
            }
            else
            {
                scores[0] = scores[1] = 0.5;
            }

            return scores;
        }

        public bool GameOver() => _gameWon || _freeCells == 0;

        public IList<TicTacToePlay> GetPlays()
        {
            List<TicTacToePlay> plays = new List<TicTacToePlay>();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (_board[row][col] == -1)
                    {
                        plays.Add(new TicTacToePlay(row, col));
                    }
                }
            }

            return plays;
        }

        public TicTacToePlay GetRandomPlay()
        {
            if (_random == null)
                _random = new Random();

            var plays = GetPlays();
            return plays[_random.Next(plays.Count)];
        }

        public void SetCurrentPlayer(int player)
        {
            CurrentPlayerTurn = player;
        }

        // Unused since this is a perfect information game
        public void SetPointOfViewPlayer(int player) {}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (_board[row][col] == -1)
                        sb.Append(" ");
                    else
                        sb.Append(PlayerMarkers[_board[row][col]]);

                    if (col < 2)
                    {
                        sb.Append("|");
                    }
                }

                sb.AppendLine();
                if (row < 2)
                {
                    sb.AppendLine("-+-+-");
                }
            }

            return sb.ToString();
        }
    }
}