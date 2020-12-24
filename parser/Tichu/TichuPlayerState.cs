using System;
using System.Collections.Generic;

namespace TichuAI
{
    public class TichuPlayerState
    {
        public List<Card> Cards = new List<Card>();
        public bool TichuCall;
        public bool GrandCall;

        public TichuPlayerState Clone()
        {
            TichuPlayerState clonedState = new TichuPlayerState();
            clonedState.TichuCall = TichuCall;
            clonedState.GrandCall = GrandCall;
            clonedState.Cards = new List<Card>(Cards);
            return clonedState;
        }
    }
}