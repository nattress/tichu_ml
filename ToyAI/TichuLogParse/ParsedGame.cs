using System;
using System.Collections.Generic;

namespace TichuAI
{
    class ParsedGame
    {
        //
        // Players
        // List of Hands:
        //      First 8
        //      Starting cards
        //      Card exchange
        //      Players with bombs
        //      List of Plays:
        //          Player number
        //          Action (play card, wish, give dragon, bomb, pass, ...)
        //          CardGroup (describes shape and identifier)
        //      Calculated Score
        //
        public string[] Players = new string[4];
        public List<ParsedHand> Hands = new List<ParsedHand>();
    }

    class ParsedHand
    {
        public TichuCard[][] GrandTichuCards = new TichuCard[][] { new TichuCard[8], new TichuCard[8], new TichuCard[8], new TichuCard[8] };
        public TichuCard[][] StartingCards = new TichuCard[][] { new TichuCard[14], new TichuCard[14], new TichuCard[14], new TichuCard[14] };
        // For each player, the 3 cards they passed: left, across, right
        public TichuCard[][] ExchangedCards = new TichuCard[][] { new TichuCard[3], new TichuCard[3], new TichuCard[3], new TichuCard[3] };
        public bool[] HasBomb = new bool[4];
        public List<Play> Plays = new List<Play>();
        public int[] Scores = new int[2];
    }
}