using System;
using System.Collections.Generic;

namespace TichuAI
{
    public class TichuDeck
    {
        List<Card> _allCards;
        Stack<Card> _deck = new Stack<Card>();
        public int Count => _deck.Count;
        public Card DealCard() => _deck.Pop();
        public IEnumerable<Card> Cards => _allCards;

        public TichuDeck(IEnumerable<Card> allCards)
        {
            _allCards = new List<Card>(allCards);
        }

        /// <summary>
        /// Creates a random deck of cards excluding dog, dragon, phoenix, mahjong
        /// </summary>
        public static TichuDeck CreateWithoutSpecials()
        {
            List<Card> drawPool = new List<Card>();
            for (int suit = 0; suit < 4; suit++)
            {
                for (int rank = 0; rank < 13; rank++)
                {
                    Card c = new Card((CardSuit)suit, (CardRank)rank);
                    drawPool.Add(c);
                }
            }
            TichuDeck deck = new TichuDeck(drawPool);
            Random random = new Random();
            for (int i = 0; i < 52; i++)
            {
                int randomCard = random.Next(drawPool.Count);
                deck._deck.Push(drawPool[randomCard]);
                drawPool.RemoveAt(randomCard);
            }

            return deck;
        }
    }
}