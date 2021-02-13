using System;
using System.Collections.Generic;

namespace TichuAI
{
    public class TichuDeck
    {
        List<TichuCard> _allCards;
        Stack<TichuCard> _deck = new Stack<TichuCard>();
        public int Count => _deck.Count;
        public TichuCard DealCard() => _deck.Pop();
        public IEnumerable<TichuCard> Cards => _allCards;

        public TichuDeck(IEnumerable<TichuCard> allCards)
        {
            _allCards = new List<TichuCard>(allCards);
        }

        /// <summary>
        /// Creates a random deck of cards excluding dog, dragon, phoenix, mahjong
        /// </summary>
        public static TichuDeck CreateWithoutSpecials()
        {
            List<TichuCard> drawPool = new List<TichuCard>();
            for (int suit = 0; suit < 4; suit++)
            {
                for (int rank = 0; rank < 13; rank++)
                {
                    TichuCard c = new TichuCard((CardSuit)suit, (CardRank)rank);
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