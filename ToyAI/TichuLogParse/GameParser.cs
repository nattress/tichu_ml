using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TichuAI
{
    class ParseException : Exception {}
    
    class GameParser
    {
        public static ParsedGame ParseGame(StreamReader reader)
        {
            ParsedGame game = new ParsedGame();
            while(true)
            {
                var hand = ParseHand(reader, game);
                if (hand == null)
                    break;

                game.Hands.Add(hand);
            }

            return game;
        }

        private static ParsedHand ParseHand(StreamReader reader, ParsedGame game)
        {
            ParsedHand hand = new ParsedHand();

            // Tichu cards
            string line = reader.ReadLine();
            if (line == null)
            {
                return null;
            }

            if (line != @"---------------Gr.Tichukarten------------------")
            {
                throw new ParseException();
            }

            // Parse grand tichu cards (first 8 dealt)
            for (int i = 0; i < 4; i++)
            {
                line = reader.ReadLine();
                int playerNum = int.Parse(line.Substring(1, 1));

                string[] grandTichuCards = line.Substring(3).Split(" ");
                for (int cardIndex = 0; cardIndex < 8; cardIndex++)
                {
                    hand.GrandTichuCards[i][cardIndex] = GetCard(grandTichuCards[cardIndex + 1]);
                }
            }
            
            line = reader.ReadLine();
            if (line != @"---------------Startkarten------------------")
            {
                throw new ParseException();
            }
                                                                                                                                                                                                                                                                                                                          
            // Parse starting hand (14 cards dealt pre-pass)
            for (int i = 0; i < 4; i++)
            {
                line = reader.ReadLine();
                int playerNum = int.Parse(line.Substring(1, 1));
                string[] startingCards = line.Substring(3).Split(" ");

                for (int cardIndex = 0; cardIndex < 14; cardIndex++)
                {
                    hand.StartingCards[i][cardIndex] = GetCard(startingCards[cardIndex + 1]);
                }
            }

            line = reader.ReadLine();

            while (line.StartsWith("Grosses Tichu: ") || line.StartsWith("Tichu: "))
            {
                if (line.StartsWith("Grosses Tichu: "))
                {
                    string[] grandPlayers = line.Substring(15).Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    foreach (string player in grandPlayers)
                    {
                        count++;
                        if (count > 1)
                        {
                            throw new Exception("Multi grand in one line");
                        }
                        
                        hand.Plays.Add(Play.GrandTichu(int.Parse(player.Substring(1,1))));
                    }
                }
                else
                {
                    string[] tichuPlayers = line.Substring(7).Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    foreach (string player in tichuPlayers)
                    {
                        count++;
                        if (count > 1)
                        {
                            throw new Exception("Multi tichu in one line");
                        }
                        
                        hand.Plays.Add(Play.GrandTichu(IndexFromPlayerTag(player)));
                    }
                }
                line = reader.ReadLine();
            }

            if (line != "Schupfen:")
            {
                throw new ParseException();
            }

            // Parse card exchange
            for (int i = 0; i < 4; i++)
            {
                line = reader.ReadLine();
                string[] passes = line.Substring(3).Split(" ", StringSplitOptions.RemoveEmptyEntries);
                string playerName = passes[0];
                if (game.Players[i] == null)
                {
                    // Seed the set of players names the first time we parse them
                    game.Players[i] = playerName;
                }
                
                //Debug.Assert(game.Players[i] == playerName); // The names should be consistent between hands in a game
                

                hand.ExchangedCards[i][0] = GetCard(passes[3]);
                hand.ExchangedCards[i][1] = GetCard(passes[6]);
                hand.ExchangedCards[i][2] = GetCard(passes[9]);
            }

            line = reader.ReadLine();

            // If there are bombs, the players with them are listed here
            if (line.StartsWith("BOMBE: "))
            {
                string[] bombPlayers = line.Substring(7).Split(" ");
                foreach (string player in bombPlayers)
                {
                    if (string.IsNullOrEmpty(player))
                        continue;

                    hand.HasBomb[int.Parse(player.Substring(1,1))] = true;
                }
                line = reader.ReadLine();
            }

            if (line != @"---------------Rundenverlauf------------------")
            {
                throw new ParseException();
            }

            // Remember who played the mahjong when we see the wish line
            int mahjongPlayer = 0;
            while (true)
            {
                // Parse plays until we see the scores indicating the end of the hand
                line = reader.ReadLine();

                // Scores line; we're done playing
                if (line.StartsWith("Ergebnis: "))
                    break;

                // Wish
                if (line.StartsWith("Wunsch:"))
                {
                    TichuCard wishCard = new TichuCard(default(CardSuit), RankFromParsedString(line.Substring(7)));
                    hand.Plays.Add(Play.Wish(mahjongPlayer, wishCard));
                }
                else if (line.StartsWith("Drache an: "))
                {
                    string dragonPlayer = line.Substring(11);
                    hand.Plays.Add(Play.GiveDragon(IndexFromPlayerTag(dragonPlayer)));
                }
                else if (line.StartsWith("Tichu: "))
                {
                    string tichuPlayer = line.Substring(7);
                    hand.Plays.Add(Play.Tichu(IndexFromPlayerTag(tichuPlayer)));
                }
                else if (line.StartsWith("("))
                {
                    // Options:
                    // - Playing cards
                    // - Passing
                    
                    string[] playLine = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    int playerIndex = IndexFromPlayerTag(playLine[0]);
                    if (playLine[1] == "passt.")
                    {
                        // Pass
                        hand.Plays.Add(Play.Pass(playerIndex));
                    }
                    else
                    {
                        // Play some number of cards
                        List<TichuCard> cards = new List<TichuCard>();
                        foreach (var x in playLine.AsSpan(1))
                        {
                            cards.Add(GetCard(x));
                        }
                        Play.PlayCards(playerIndex, cards.ToArray());
                    }
                }
                else
                {
                    Debug.Assert(false, $"Unparsed line: {line}");
                }
            }

            Debug.Assert(line.StartsWith("Ergebnis: "));
            string[] scores = line.Substring(10).Split(" ", StringSplitOptions.RemoveEmptyEntries);

            hand.Scores[0] = int.Parse(scores[0]);
            hand.Scores[1] = int.Parse(scores[2]);

            Debug.Assert((hand.Scores[0] + hand.Scores[1]) % 100 == 0);
            return hand;
        }

        /// <summary>
        /// Maps (N)Player -> N
        /// </summary>
        private static int IndexFromPlayerTag(string playerTag) => int.Parse(playerTag.Substring(1,1));
        private static TichuCard GetCard(string parsedCard)
        {
            SpecialCard special = SpecialCard.None;
            // Early out for specials
            switch (parsedCard)
            {
                case "Ph":
                    special = SpecialCard.Phoenix;
                    break;
                case "Hu":
                    special = SpecialCard.Dog;
                    break;
                case "Dr":
                    special = SpecialCard.Dragon;
                    break;
                case "Ma":
                    special = SpecialCard.Mahjong;
                    break;
            }

            // First character is suit
            string suit = parsedCard.Substring(0, 1);
            CardSuit cardSuit = default(CardSuit);
            switch (suit)
            {
                case "G":
                    cardSuit = CardSuit.Hearts;
                    break;
                case "R":
                    cardSuit = CardSuit.Clubs;
                    break;
                case "B":
                    cardSuit = CardSuit.Diamonds;
                    break;
                case "S":
                    cardSuit = CardSuit.Spades;
                    break;
            }

            // Rank is 1 or 2 characters
            string rank = parsedCard.Substring(1);
            CardRank cardRank = (CardRank)RankFromParsedString(rank);
            return new TichuCard(cardSuit, cardRank, special);
        }

        private static CardRank RankFromParsedString(string rank)
        {
            switch (rank)
            {
                case "B":
                    // B = Jack
                    return CardRank.Jack;
                case "D":
                    // D = Queen
                    return CardRank.Queen;
                case "K":
                    // K = King 
                    return CardRank.King;
                case "A":
                    // A = Ace
                    return CardRank.Ace;
                default:
                    // 2 - 10. Adjust down so a 2 has card index 0
                    return (CardRank)(int.Parse(rank) - 2);
            }
        }
    }
}