using System;
using System.Collections.Generic;

namespace TichuAI
{
    public class SixNimmtDeck
    {
        private const int DeckSize = 104;
        int[] _shuffledCards; 
        int[] _allCards;
        /// <summary>
        /// Cards are popped by incrementing the index into _shuffledCards
        /// </summary>
        int _shuffledCardIndex;
        public int Count => _allCards.Length;
        public int DealCard()
        {
            return _shuffledCards[_shuffledCardIndex++];
        }
        public IEnumerable<int> Cards => _allCards;

        public SixNimmtDeck(int[] allCards)
        {
            _allCards = allCards;
        }

        private void Shuffle()
        {
            Random random = new Random();
            _shuffledCards = new int[DeckSize];
            Array.Copy(_allCards, _shuffledCards, DeckSize);
            int shuffle = DeckSize;
            while (shuffle-- > 0)
            {
                int replaceIndex = random.Next(shuffle);
                int swap = _shuffledCards[shuffle];
                _shuffledCards[shuffle] = _shuffledCards[replaceIndex];
                _shuffledCards[replaceIndex] = swap;
            }
        }

        /// <summary>
        /// Creates a random deck of cards excluding dog, dragon, phoenix, mahjong
        /// </summary>
        public static SixNimmtDeck Create()
        {
            int[] unshuffled = new int[DeckSize];
            for (int i = 0; i < DeckSize; i++)
            {
                unshuffled[i] = i + 1;
            }
            SixNimmtDeck deck = new SixNimmtDeck(unshuffled);
            deck.Shuffle();

            return deck;
        }
    }
}
