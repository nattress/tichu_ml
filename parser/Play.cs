using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TichuAI
{
    public enum PlayType
    {
        PlayShape,
        Wish,
        GotDragon,
        Bomb,
        Pass,
        Tichu,
        GrandTichu
    }  

    enum CardShape
    {
        Single,
        Pair,
        Triple,
        FourOfAKind,
        FullHouse,
        Straight_5
    }

    public class Play : IComparable
    {
        class PlayComparer : IEqualityComparer<(int player, PlayType playType, Card[] cards)>
        {
            public bool Equals((int player, PlayType playType, Card[] cards) left, (int player, PlayType playType, Card[] cards) right)
            {  
                if (left.player != right.player)
                    return false;

                if (left.playType != right.playType)
                    return false;

                if ((left.cards == null || right.cards == null) && right.cards != left.cards)
                    return false;

                if (left.cards.Length != right.cards.Length)
                    return false;

                for (int i = 0; i < left.cards.Length; i++)
                {
                    if (!left.cards[i].Equals(right.cards[i]))
                        return false;
                }

                return true;
            }

            public int GetHashCode((int, PlayType, Card[]) key)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + key.Item1.GetHashCode();
                    hash = hash * 23 + key.Item2.GetHashCode();
                    foreach (Card c in key.Item3)
                    {
                        hash = hash * 23 + c.GetHashCode();
                    }
                    return hash;
                }
            }
        }

        static ConcurrentDictionary<(int, PlayType, Card[]), Play> _internedPlays = new ConcurrentDictionary<(int, PlayType, Card[]), Play>(new PlayComparer());
        public readonly int Player;
        public readonly PlayType PlayType;
        public readonly Card[] Cards;
        private Play(int player, PlayType playType, Card[] cards = null)
        {
            Player = player;
            PlayType = playType;
            Cards = cards;
        }

        public static Play Tichu(int player) => _internedPlays.GetOrAdd((player, PlayType.Tichu, null), (x) => new Play(player, PlayType.Tichu));
        public static Play GrandTichu(int player) => new Play(player, PlayType.GrandTichu);
        public static Play Wish(int player, Card card) => new Play(player, PlayType.Wish, new Card[] {card});
        public static Play Pass(int player) => new Play(player, PlayType.Pass);
        public static Play GiveDragon(int player) => new Play(player, PlayType.GotDragon);
        public static Play PlayCards(int player, Card[] cards) => _internedPlays.GetOrAdd((player, PlayType.PlayShape, cards), (x) => new Play(player, PlayType.PlayShape, cards));

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var card in Cards)
            {
                sb.Append((sb.Length > 0 ? " " : "") + card.ToString());
            }
            return $"[{Player}]{PlayType}:{sb.ToString()}";
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Play other = obj as Play;
            if (Player != other.Player)
                return Player > other.Player ? 1 : -1;

            if (PlayType != other.PlayType)
                return PlayType > other.PlayType ? 1 : -1;

            if (Cards.Length != other.Cards.Length)
                return Cards.Length > other.Cards.Length ? 1 : -1;

            for (int i = 0; i < Cards.Length; i++)
            {
                int result = Cards[i].CompareTo(other.Cards[i]);
                if (result != 0)
                    return result;
            }

            return 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Player.GetHashCode();
                hash = hash * 23 + PlayType.GetHashCode();
                foreach (Card c in Cards)
                {
                    hash = hash * 23 + c.GetHashCode();
                }
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }
    }
}