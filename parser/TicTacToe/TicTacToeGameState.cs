using System;
using System.Collections.Generic;

namespace TichuAI
{
    public class TicTacToeGameState : IGameState
    {
        int[][] _board; // [row][column]
        public int CurrentPlayerTurn => throw new NotImplementedException();

        public IGameState Clone()
        {
            TicTacToeGameState newState = new TicTacToeGameState();
            newState._board = new int[3][];
            for (int row = 0; row < 3; row++)
            {
                newState._board[row] = new int[3];
                for (int col = 0; col < 3; col++)
                {
                    newState._board[row][col] = _board[row][col];
                }
            }
            return newState;
        }

        public void CommitPlay(Play play)
        {
            throw new NotImplementedException();
        }

        public double[] Evaluate()
        {
            throw new NotImplementedException();
        }

        public bool GameOver()
        {
            throw new NotImplementedException();
        }

        public IList<Play> GetPlays()
        {
            throw new NotImplementedException();
        }

        public Play GetRandomPlay()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentPlayer(int player)
        {
            throw new NotImplementedException();
        }

        public void SetPointOfViewPlayer(int player)
        {
            throw new NotImplementedException();
        }
    }
}