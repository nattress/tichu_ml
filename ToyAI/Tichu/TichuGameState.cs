using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TichuAI
{
    public class TichuGameState : IGameState<Play>
    {
        public HashSet<TichuCard> RemainingCards = new HashSet<TichuCard>();
        public HashSet<TichuCard> PlayedCards = new HashSet<TichuCard>();
        public HashSet<TichuCard> RemainingPlayoutCards = new HashSet<TichuCard>();
        public TichuPlayerState[] Players;
        public int PlayerCount => Players.Length;
        /// <summary>
        /// Index of which player plays next
        /// </summary>
        public int CurrentPlayerTurn { get; private set; }
        private int[] _tricks = new int[4];
        private List<Play> _currentTrick = new List<Play>();
        private int _pointOfViewPlayer;
        private List<Play> _playHistory = new List<Play>();
        private Random _random = null;

        public TichuGameState(TichuDeck deck, Random random)
        {
            RemainingCards = new HashSet<TichuCard>(deck.Cards);
            _random = random;
        }

        private TichuGameState() {}

#region IGameState implementations
        public bool GameOver() => PlayedCards.Count == 52;

        public void SetPointOfViewPlayer(int player)
        {
            _pointOfViewPlayer = player;
            RemainingPlayoutCards = new HashSet<TichuCard>(RemainingCards.Except(Players[_pointOfViewPlayer].Cards));
        }

        public void SetCurrentPlayer(int player)
        {
            CurrentPlayerTurn = player;
        }

        public IGameState<Play> Clone()
        {
            TichuGameState clonedState = new TichuGameState();
            clonedState.CurrentPlayerTurn = CurrentPlayerTurn;
            clonedState.PlayedCards = new HashSet<TichuCard>(PlayedCards);
            clonedState.RemainingCards = new HashSet<TichuCard>(RemainingCards);
            clonedState.RemainingPlayoutCards = new HashSet<TichuCard>(RemainingPlayoutCards);
            clonedState.Players = new TichuPlayerState[PlayerCount];
            for (int i = 0; i < PlayerCount; i++)
            {
                clonedState.Players[i] = Players[i].Clone();
            }
            Array.Copy(_tricks, clonedState._tricks, _tricks.Length);
            clonedState._currentTrick = new List<Play>(_currentTrick);
            clonedState._playHistory = new List<Play>(_playHistory);
            clonedState._random = _random;
            clonedState._pointOfViewPlayer = _pointOfViewPlayer;
            return clonedState;
        }

        public void CommitPlay(Play play)
        {
            _playHistory.Add(play);
            _currentTrick.Add(play);
            
            Debug.Assert(!PlayedCards.Contains(play.Cards[0]));
            PlayedCards.Add(play.Cards[0]);
            RemainingCards.Remove(play.Cards[0]);
            if (CurrentPlayerTurn == _pointOfViewPlayer)
            {
                Players[CurrentPlayerTurn].Cards.Remove(play.Cards[0]);
            }
            else
            {
                RemainingPlayoutCards.Remove(play.Cards[0]);
            }
                
            
            if (_currentTrick.Count == 4)
            {
                // Suit of the first card
                CardSuit trickSuit = _currentTrick[0].Cards[0].Suit;

                // Highest card of that suit
                Play bestPlay = _currentTrick[0];
                CardRank highestRank = _currentTrick[0].Cards[0].Rank;
                foreach (var searchPlay in _currentTrick)
                {
                    if (searchPlay.Cards[0].Suit == trickSuit && searchPlay.Cards[0].Rank > highestRank)
                    {
                        bestPlay = searchPlay;
                        highestRank = searchPlay.Cards[0].Rank;
                    }
                }

                _tricks[bestPlay.Player]++;
                _currentTrick.Clear();
                CurrentPlayerTurn = bestPlay.Player;
            }
            else
            {
                CurrentPlayerTurn = (CurrentPlayerTurn + 1) % 4;
            }

            System.Diagnostics.Debug.Assert(13 - Players[_pointOfViewPlayer].Cards.Count <= (PlayedCards.Count) / 4 + 1);
        }

        public IList<Play> GetPlays()
        {
            // Any card that hasn't been played is a valid play
            List<Play> plays = new List<Play>();

            if (CurrentPlayerTurn == _pointOfViewPlayer)
            {
                if (_currentTrick.Count > 0)
                {
                    // Try and follow suit. If we have none of that suit, fall through into allowing all cards in our hand
                    var suitCards = TichuCard.GetCardsWithSuit(Players[CurrentPlayerTurn].Cards, _currentTrick[0].Cards[0].Suit);
                    if (suitCards != null)
                    {
                        foreach (var card in suitCards)
                        {
                            plays.Add(Play.PlayCards(CurrentPlayerTurn, new TichuCard[] {card}));
                        }
                    }
                }
                
                // If we didn't have to follow suit, any card is a valid play
                if (plays.Count == 0)
                {
                    // Make correct plays from our own hand 
                    foreach (var card in Players[CurrentPlayerTurn].Cards)
                    {
                        plays.Add(Play.PlayCards(CurrentPlayerTurn, new TichuCard[] {card}));
                    }
                }
            }
            else
            {
                // For simulated players we choose a random card from those unused. Also exclude cards
                // from the point-of-view player's hand since from the AI agent's pov those cards are
                // determined (ie, we're not cheating by looking at their hand here).
                // We could use a more sophisticated selection policy here to guide the search.
                //foreach (var card in RemainingCards.Except(Players[_pointOfViewPlayer].Cards))
                foreach (var card in RemainingPlayoutCards)
                {
                    plays.Add(Play.PlayCards(CurrentPlayerTurn, new TichuCard[] {card}));
                }
            }
            
            return plays;
        }

        public Play GetRandomPlay()
        {
            var plays = GetPlays();
            return plays[_random.Next(plays.Count)];
        }

        /// <summary>
        /// Calculates the scores for all players for the given game state
        /// </summary>
        public double[] Evaluate()
        {
            // What do we return for games that aren't finished? Do we even get asked that?
            double[] result = new double[_tricks.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (double)_tricks[i];
            }
            return result;
        }
#endregion

        public override string ToString()
        {
            // What state do we want to show?
            // - Current cards in each hand
            // - Current player turn
            // - Cards played in current trick
            // - Trick counts by player
            StringBuilder sb = new StringBuilder();
            for (int player = 0; player < 4; player++)
            {
                if (CurrentPlayerTurn == player)
                {
                    sb.Append($"[{player}] ");
                }
                else
                {
                    sb.Append($" {player}  ");
                }
                sb.AppendLine(TichuCard.PrintCardsSortedBySuit(Players[player].Cards));
            }

            sb.AppendLine(string.Join(" ", _currentTrick.Select(play => play.ToString())));
            sb.AppendLine(string.Join(" ", Evaluate()));
            return sb.ToString();
        }
    }
}