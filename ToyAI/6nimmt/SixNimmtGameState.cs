using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TichuAI
{
    public enum SixNimmtInputState
    {
        /// <summary>
        /// Game phase where each player chooses a card to place on the board.
        /// </summary>
        SelectCard,
        PlaceCards,
        /// <summary>
        /// During card placement, when a card is lower than any of the current row heads, the player who played the lowest of those cards selects a row to clear (and takes points accordingly)
        /// On Pro mode, this happens if a card also can't be played on the beginning of any row.
        /// </summary>
        TakeRow
    }

    public class SixNimmtGameState : IGameState<int>
    {
        public static readonly int BoardRowCount = 4; // The standard size of a 6nimmt board
        public static readonly int MaxCardsPerRow = 5;
        public static readonly int StartingScore = 66;
        private const int RowNotFound = -1;
        private const int RowPlacementAmbiguous = -2;

        private class SharedState
        {
            public Random Random;
            public int PlayerCount;
            public bool ProMode;
        }

        private SharedState _sharedState;
        int[][] _board;
        /// <summary>
        /// How many cards have been played in each row. Indexes into _board[][]
        /// </summary>
        int[] _boardRowCounts;
        public List<int>[] PlayerCards;
        int[] _scores;
        private int _pointOfViewPlayer;
        public SixNimmtInputState PlayInputState {get; private set;}
        public int PlayerCount => _sharedState.PlayerCount;
        private List<int> _currentRoundCards = new List<int>();
        private HashSet<int> _remainingCards = new HashSet<int>();
        private HashSet<int> _remainingPlayoutCards = new HashSet<int>();
        public Dictionary<int, int> CardToPlayerDictionary = new Dictionary<int, int>();
        private int _currentRoundCardsProcessingIndex = 0;

        private SixNimmtGameState(SharedState sharedState, SixNimmtDeck deck)
        {
            PlayInputState = SixNimmtInputState.SelectCard;
            _sharedState = sharedState;
            _board = new int[BoardRowCount][];
            for (int row = 0; row < BoardRowCount; row++)
            {
                _board[row] = new int[MaxCardsPerRow];
            }
            _scores = new int[PlayerCount];
            for (int i = 0; i < _scores.Length; i++)
            {
                _scores[i] = StartingScore;
            }
            _boardRowCounts = new int[BoardRowCount];
            PlayerCards = new List<int>[PlayerCount];
            for (int i = 0; i < PlayerCards.Length; i++)
            {
                PlayerCards[i] = new List<int>();
            }
            _remainingCards = new HashSet<int>(deck.Cards);
        }

        private SixNimmtGameState() {}

        public static SixNimmtGameState Create(Random random, SixNimmtDeck deck, int playerCount, bool proMode)
        {
            return new SixNimmtGameState(new SharedState() { Random = random, PlayerCount = playerCount, ProMode = proMode }, deck);
        }

        public int CurrentPlayerTurn { get; private set; }

        public IGameState<int> Clone()
        {
            SixNimmtGameState clonedState = new SixNimmtGameState();
            clonedState._sharedState = _sharedState;
            clonedState.CurrentPlayerTurn = CurrentPlayerTurn;
            clonedState._pointOfViewPlayer = _pointOfViewPlayer;
            clonedState._board = _board.Select(s => s.ToArray()).ToArray();
            clonedState._boardRowCounts = _boardRowCounts.ToArray();
            clonedState._scores = new int[PlayerCount];
            Array.Copy(_scores, clonedState._scores, _scores.Length);
            clonedState.PlayerCards = new List<int>[PlayerCount];
            for (int i = 0; i < PlayerCount; i++)
            {
                clonedState.PlayerCards[i] = new List<int>(PlayerCards[i]);
            }
            clonedState.PlayInputState = PlayInputState;
            clonedState._currentRoundCards = new List<int>(_currentRoundCards);
            clonedState._remainingCards = new HashSet<int>(_remainingCards);
            clonedState._remainingPlayoutCards = new HashSet<int>(_remainingPlayoutCards);
            clonedState.CardToPlayerDictionary = new Dictionary<int, int>(CardToPlayerDictionary);
            return clonedState;
        }
        
        /// <summary>
        /// Given a card, where will it land when played on the board. Returns -1 if card is lower than all row high-cards.
        /// Returns RowPlacementAmbiguous (-2) if the card could be placed in more than one location and puts the two
        /// ambiguous rows into "ambiguousRows".
        /// </summary>
        private int ComputeCardRow(int card, out int[] ambiguousRows)
        {
            int closestCardRow = -1;
            int closestCard = -1;
            // For pro-mode where cards can be placed on the left, too
            int closestLowCardRow = -1;
            int closestLowCard = 105;
            ambiguousRows = null;
            for (int row = 0; row < BoardRowCount; row++)
            {
                if (card > HighCardForRow(row) && HighCardForRow(row) > closestCard)
                {
                    closestCard = HighCardForRow(row);
                    closestCardRow = row;
                }
                if (this._sharedState.ProMode)
                {
                    if (card < LowCardForRow(row) && LowCardForRow(row) < closestLowCard)
                    {
                        closestLowCard = LowCardForRow(row);
                        closestLowCardRow = row;
                    }
                }
            }

            if (this._sharedState.ProMode)
            {
                if (closestCardRow == -1 && closestLowCardRow == -1)
                {
                    return RowNotFound;
                }
                else if (closestCardRow == -1)
                {
                    return closestLowCardRow;
                }
                else if (closestLowCardRow == -1)
                {
                    return closestCardRow;
                }
                else if (closestCard - card == card - closestLowCard)
                {
                    // In pro mode, a tie between the closest board card matching the played card. Ie:
                    // 49 50
                    // 54 58
                    // 
                    // Played: 52
                    ambiguousRows = new int[2];
                    ambiguousRows[0] = closestCardRow;
                    ambiguousRows[1] = closestLowCardRow;
                    return RowPlacementAmbiguous;
                }
                else
                {
                    return card - closestCard > closestLowCard - card ? closestLowCardRow : closestCardRow;
                }
            }

            return closestCardRow != -1 ? closestCardRow : RowNotFound;
        }

        /// <summary>
        /// Returns the current highest card on a row
        /// </summary>
        private int HighCardForRow(int row) => _board[row][_boardRowCounts[row] - 1];
   
        private int LowCardForRow(int row) => _board[row][0];
        public void SetInitialScores(double[] scores)
        {
            Debug.Assert(scores.Length == _scores.Length);
            for (int i = 0; i < _scores.Length; i++)
            {
                _scores[i] = (int)scores[i];
            }
        }

        public void DealCard(int player, int card)
        {
            PlayerCards[player].Add(card);
        }

        public void AddStartingCard(int row, int card)
        {
            _remainingCards.Remove(card);
            _remainingPlayoutCards.Remove(card);
            AddCardToRow(row, card);
        }

        private void AddCardToRow(int row, int card)
        {
            if (_sharedState.ProMode && card < _board[row][0])
            {
                for (int i = _boardRowCounts[row]; i > 0; i--)
                {
                    _board[row][i] = _board[row][i-1];
                }
                _board[row][0] = card;
                ++_boardRowCounts[row];
            }
            else
            {
                _board[row][_boardRowCounts[row]++] = card;
            }
        }

        private static int GetCardScore(int card)
        {
            if (card == 55)
            {
                return 7;
            }
            else if (card % 11 == 0)
            {
                return 5;
            }
            else if (card % 10 == 0)
            {
                return 3;
            }
            else if (card % 5 == 0)
            {
                return 2;
            }

            return 1;
        }

        private int ComputePointsOnRow(int row)
        {
            int sum = 0;
            for (int i = 0; i < _boardRowCounts[row]; i++)
            {
                sum += GetCardScore(_board[row][i]);
            }

            return sum;
        }

        private void SetGameState(SixNimmtInputState newInputState)
        {
            switch (newInputState)
            {
                case SixNimmtInputState.SelectCard:
                    Debug.Assert(_currentRoundCards.Count == 0);
                    break;
                case SixNimmtInputState.TakeRow:
                case SixNimmtInputState.PlaceCards:
                    Debug.Assert(_currentRoundCards.Count == PlayerCount);
                    break;
                default:
                    throw new NotImplementedException("Best that we handle all states there.");
            }

            PlayInputState = newInputState;
        }

        public void CommitPlay(int play)
        {
            int playerChosenRow = -1;
            if (PlayInputState == SixNimmtInputState.SelectCard)
            {
                _currentRoundCards.Add(play);
                CardToPlayerDictionary.Add(play, CurrentPlayerTurn);
                if (CurrentPlayerTurn == _pointOfViewPlayer)
                {
                    // For move suggest where opponent cards aren't in the state PlayerCards is empty so no need to remove
                    if (PlayerCards[CurrentPlayerTurn].Count > 0)
                    {
                        PlayerCards[CurrentPlayerTurn].Remove(play);
                    }
                }
                else
                {
                    _remainingPlayoutCards.Remove(play);
                }
                _remainingCards.Remove(play);

                if (_currentRoundCards.Count == PlayerCount)
                {
                    // Each player has selected a card for this round.

                    // Sort the cards
                    _currentRoundCards.Sort();
                    SetGameState(SixNimmtInputState.PlaceCards);
                }
                else
                {
                    CurrentPlayerTurn = (CurrentPlayerTurn + 1) % PlayerCount;
                    return;
                }
            }
            else
            {
                // "play" is the row number to take
                playerChosenRow = play;
            }

            Debug.Assert(_currentRoundCards.Count == PlayerCount);
            
            for (int currentRoundCardIndex = _currentRoundCardsProcessingIndex; currentRoundCardIndex < _currentRoundCards.Count; currentRoundCardIndex++)
            {
                var card = _currentRoundCards[currentRoundCardIndex];
                int computedCardRow = ComputeCardRow(card, out _);
                int targetRow = computedCardRow;

                if (PlayInputState == SixNimmtInputState.PlaceCards && targetRow < 0)
                {
                    SetGameState(SixNimmtInputState.TakeRow);
                    SetCurrentPlayer(CardToPlayerDictionary[card]);
                    return;
                }

                if (PlayInputState == SixNimmtInputState.TakeRow)
                {
                    Debug.Assert(targetRow < 0);
                    targetRow = playerChosenRow;
                }

                if (_boardRowCounts[targetRow] == MaxCardsPerRow || (PlayInputState == SixNimmtInputState.TakeRow && computedCardRow == RowNotFound))
                {
                    // The row is full; compute the points
                    int rowTaker = CardToPlayerDictionary[card];
                    _scores[rowTaker] -= ComputePointsOnRow(targetRow);
                    _boardRowCounts[targetRow] = 0;
                    for (int i = 0; i < MaxCardsPerRow; i++)
                    {
                        _board[targetRow][i] = 0;
                    }
                }
                AddCardToRow(targetRow, card);
                SetGameState(SixNimmtInputState.PlaceCards);
            }
            _currentRoundCards.Clear();
            _currentRoundCardsProcessingIndex = 0;
            SetGameState(SixNimmtInputState.SelectCard);
            SetCurrentPlayer(0);
        }

        public double[] Evaluate()
        {
            double[] result = new double[_scores.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (double)_scores[i];
            }
            return result;
        }

        public bool GameOver()
        {
            if (_currentRoundCards.Count == 0 && PlayerCards[0].Count == 0)
            {
                return true;
            }
            return false;
        } 

        public IList<int> GetPlays()
        {
            if (PlayInputState == SixNimmtInputState.SelectCard)
            {
                if (CurrentPlayerTurn == _pointOfViewPlayer)
                {
                    // For move suggest where opponent cards aren't in the state
                    if (PlayerCards[CurrentPlayerTurn].Count == 0)
                    {
                        return new List<int>(_remainingCards);
                    }

                    // AI player can use any card in their hand
                    return new List<int>(PlayerCards[CurrentPlayerTurn]);
                }
                else
                {
                    // For simulated players we choose a random card from those unused. Also exclude
                    // cards from the pov player's hand since from the AI agent's pov those cards are
                    // determined (ie, we're not cheating by looking at their hand here).
                    // We could use a more sophisticated selection policy here to guide the search.
                    foreach (var c in _remainingPlayoutCards)
                    {
                        if (CardToPlayerDictionary.ContainsKey(c))
                        {
                            Console.WriteLine($"Already contains {c}");
                        }
                    }
                    return new List<int>(_remainingPlayoutCards);
                }
            }
            else if (PlayInputState == SixNimmtInputState.TakeRow)
            {
                var card = _currentRoundCards[_currentRoundCardsProcessingIndex];
                int computedCardRow = ComputeCardRow(card, out int[] ambiguousRows);
                Debug.Assert(computedCardRow < 0);
                if (computedCardRow == RowNotFound)
                {
                    return new int[] {0, 1, 2, 3};
                }
                else
                {
                    return ambiguousRows;
                }
                
            }

            throw new InvalidOperationException();
        }

        public int GetRandomPlay()
        {
            if (_sharedState.Random == null)
                _sharedState.Random = new Random();

            var plays = GetPlays();
            return plays[_sharedState.Random.Next(plays.Count)];
        }

        public void SetCurrentPlayer(int player)
        {
            CurrentPlayerTurn = player;
        }

        // Unused since this is a perfect information game
        public void SetPointOfViewPlayer(int player)
        {
            _pointOfViewPlayer = player;
            _remainingPlayoutCards = new HashSet<int>(_remainingCards.Except(PlayerCards[_pointOfViewPlayer]));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < BoardRowCount; i++)
            {
                if (CurrentPlayerTurn == i)
                {
                    sb.Append($"[{i}]");
                }
                else
                {
                    sb.Append($" {i} ");
                }
                sb.AppendLine(string.Join(" ", _board[i]));
            }
            sb.AppendLine($"Scores: ");
            for (int i = 0; i < _scores.Length; i++)
            {
                sb.AppendLine($"{i}: {_scores[i]}");
            }
            
            sb.Append("Current round cards: ");
            if (_currentRoundCards.Count > 0)
            {
                sb.AppendLine(string.Join(",", _currentRoundCards));
            }
            else
            {
                sb.AppendLine("-");
            }

            return sb.ToString();
        }
    }
}