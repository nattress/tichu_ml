using System;
using System.Collections.Generic;

namespace TichuAI
{
    public enum CardRank : int
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    public enum CardSuit
    {
        // Swords,
        // Jade,
        // Pagoda,
        // Star,
        Hearts,
        Clubs,
        Diamonds,
        Spades
    }

    public enum SpecialCard
    {
        None,
        Mahjong,
        Dog,
        Phoenix,
        Dragon
    }

    public class TichuCard : IComparable
    {
        public readonly CardSuit Suit;
        public readonly CardRank Rank;
        public readonly SpecialCard Special;

        public TichuCard(CardSuit suit, CardRank rank, SpecialCard special = SpecialCard.None)
        {
            Suit = suit;
            Rank = rank;
            Special = special;
        }

        public override string ToString()
        {
            string result;
            switch (Rank)
            {
                case CardRank.Jack:
                    result = "J";
                    break;
                case CardRank.Queen:
                    result = "Q";
                    break;
                case CardRank.King:
                    result = "K";
                    break;
                case CardRank.Ace:
                    result = "A";
                    break;
                default:
                    result = ((int)Rank + 2).ToString();
                    break;
            }

            switch (Suit)
            {  
                case CardSuit.Hearts:
                    result += "H";
                    break;
                case CardSuit.Clubs:
                    result += "C";
                    break;
                case CardSuit.Diamonds:
                    result += "D";
                    break;
                case CardSuit.Spades:
                    result += "S";
                    break;
            }

            return result;
        }

        public static IEnumerable<TichuCard> GetCardsWithSuit(IEnumerable<TichuCard> cards, CardSuit suit)
        {
            List<TichuCard> result = null;

            foreach (var card in cards)
            {
                if (card.Suit == suit)
                {
                    if (result == null)
                        result = new List<TichuCard>();

                    result.Add(card);
                }
            }

            return result;
        }

        public static string PrintCardsSortedBySuit(IEnumerable<TichuCard> cards)
        {
            List<TichuCard> cardList = new List<TichuCard>(cards);
            cardList.Sort();
            return string.Join(" ", cardList);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            TichuCard other = obj as TichuCard;
            if (Suit != other.Suit)
                return Suit > other.Suit ? 1 : -1;

            if (Rank != other.Rank)
                return Rank > other.Rank ? 1 : -1;
            
            if (Special != other.Special)
                return Special > other.Special ? 1 : -1;
            
            return 0;
        }
        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return ((int)Suit << 16) ^ ((int)Rank << 8) ^ (int)Special;
        }
    }
}
