using System;
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

    public class Play
    {
        public int Player;
        public PlayType PlayType;
        public Card[] Cards;
        public static Play Tichu(int player) => new Play() { Player = player, PlayType = PlayType.Tichu };
        public static Play GrandTichu(int player) => new Play() { Player = player, PlayType = PlayType.GrandTichu };
        public static Play Wish(int player, Card card) => new Play() { Player = player, PlayType = PlayType.Wish, Cards = new Card[] {card} };
        public static Play Pass(int player) => new Play() { Player = player, PlayType = PlayType.Pass };
        public static Play Dragon(int player) => new Play() { Player = player };
        public static Play PlayCards(int player, Card[] cards) => new Play() { Player = player, Cards = cards, PlayType = PlayType.PlayShape };

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var card in Cards)
            {
                sb.Append((sb.Length > 0 ? " " : "") + card.ToString());
            }
            return $"[{Player}]{PlayType}:{sb.ToString()}";
        }
    }
}