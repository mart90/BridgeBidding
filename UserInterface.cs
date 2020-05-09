using System;
using System.Collections.Generic;

namespace BridgeBidding
{
    class UserInterface
    {
        public UserInterface()
        {
            BiddingRoundHistory = new List<BiddingRound>();
        }

        public List<BiddingRound> BiddingRoundHistory { get; private set; }

        public void Start()
        {
            PlayerPosition userPosition = GetPlayerPositionFromUserInput("Your position: ");
            PlayerPosition firstToBid = GetPlayerPositionFromUserInput("First player to bid: ");
            
            while (true)
            {
                BiddingRound round = new BiddingRound(userPosition, firstToBid);

                DoBidding(round);

                BiddingRoundHistory.Add(round);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNew hand\n");
                Console.ForegroundColor = ConsoleColor.White;

                // TODO Add tips for communication during play

                firstToBid = BiddingRound.GetNextPlayerPosition(firstToBid);
            }
        }

        private static void DoBidding(BiddingRound round)
        {
            Hand userHand = GetHandFromUserInput("Your hand: ");

            BiddingSystemLogic biddingLogic = new BiddingSystemLogic(round, userHand);

            while (true)
            {
                if (round.BiddingEnded)
                {
                    return;
                }

                Bid bid;

                if (round.CurrentBiddingPlayer == round.User)
                {
                    // It's our turn to bid

                    if (new List<BiddingMode> { BiddingMode.Improvise, BiddingMode.Overcall, BiddingMode.OvercallResponse }.Contains(round.BiddingMode))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("\nWe can't help you anymore. Waiting for next hand.");
                        Console.ForegroundColor = ConsoleColor.White;
                        return;
                    }

                    BiddingSystemFeedback feedback = null;

                    if (round.BiddingMode == BiddingMode.Opening)
                    {
                        feedback = biddingLogic.OpeningBid();
                    }
                    else if (round.BiddingMode == BiddingMode.Responding)
                    {
                        feedback = biddingLogic.RespondingBid();
                        feedback.ModifyLevelIfOpponentRaised(round);
                    }
                    else if (round.BiddingMode == BiddingMode.OpenersRebid)
                    {
                        feedback = biddingLogic.OpenersRebid();
                    }

                    if (feedback != null)
                    {
                        feedback.ToConsole();
                    }

                    if (round.BiddingMode == BiddingMode.Responding || round.BiddingMode == BiddingMode.OpenersRebid)
                    {
                        // Can't help the user after this
                        return;
                    }

                    bid = GetBidFromUserInput("Your bid: ");
                }
                else if (round.CurrentBiddingPlayer == round.Partner)
                {
                    bid = GetBidFromUserInput("Partner's bid: ");
                }
                else
                {
                    bid = GetBidFromUserInput($"{round.CurrentBiddingPlayer.Position.ToString()} bid: ");
                }

                if (bid == null)
                {
                    // User input "break"
                    return;
                }

                round.AddBid(bid);

                round.NextTurn();

                continue;
            }
        }

        private static Hand GetHandFromUserInput(string prompt)
        {
            string errMessage = "Invalid hand";
            string moreInfo = "Format: '{HCP} {#S} {#H} {#D} {#C}' where #S refers to the number of spades in hand. Example: '10 5 3 3 2'";

            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine();

                if (input == "")
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                }

                string[] splitInput = input.Split(' ');

                if (!int.TryParse(splitInput[0], out int highCardPoints))
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                    continue;
                }
                if (!int.TryParse(splitInput[1], out int numberOfSpades))
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                    continue;
                }
                if (!int.TryParse(splitInput[2], out int numberOfHearts))
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                    continue;
                }
                if (!int.TryParse(splitInput[3], out int numberOfDiamonds))
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                    continue;
                }
                if (!int.TryParse(splitInput[4], out int numberOfClubs))
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                    continue;
                }

                if (numberOfSpades + numberOfHearts + numberOfDiamonds + numberOfClubs != 13)
                {
                    WriteUserErrorToConsole(errMessage, "Your hand should contain 13 cards total. " + moreInfo);
                    continue;
                }

                return new Hand(highCardPoints, numberOfSpades, numberOfHearts, numberOfDiamonds, numberOfClubs);
            }
        }

        private static Bid GetBidFromUserInput(string prompt)
        {
            string errMessage = "Invalid bid";
            string moreInfo = "Format: 'p' for pass, 'dbl' for double, 'rdbl' for redouble or '{number of tricks}{trump suit}'. Valid trump suits are nt/s/h/d/c";

            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine();

                if (input == "")
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                }

                if (input == "break")
                {
                    // Shortcut to go to next hand
                    return null;
                }

                if (input == "p")
                {
                    return new Bid().SetPass();
                }
                if (input == "dbl")
                {
                    return new Bid().SetDouble();
                }
                if (input == "rdbl")
                {
                    return new Bid().SetRedouble();
                }

                if (!int.TryParse(input[0].ToString(), out int numberOfTricks))
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                    continue;
                }

                Suit? suit = GetSuitFromInputString(input.Substring(1));

                if (suit == null)
                {
                    WriteUserErrorToConsole(errMessage, moreInfo);
                    continue;
                }

                return new Bid(suit.Value, numberOfTricks);
            }
        }

        private static Suit? GetSuitFromInputString(string inputString)
        {
            if (inputString == "nt")
            {
                return Suit.NoTrump;
            }
            if (inputString == "s")
            {
                return Suit.Spades;
            }
            if (inputString == "h")
            {
                return Suit.Hearts;
            }
            if (inputString == "d")
            {
                return Suit.Diamonds;
            }
            if (inputString == "c")
            {
                return Suit.Clubs;
            }
            return null;
        }

        private static PlayerPosition GetPlayerPositionFromUserInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = ReadLine();

                if (input == "n")
                {
                    return PlayerPosition.North;
                }
                else if (input == "e")
                {
                    return PlayerPosition.East;
                }
                else if (input == "s")
                {
                    return PlayerPosition.South;
                }
                else if (input == "w")
                {
                    return PlayerPosition.West;
                }
                else
                {
                    WriteUserErrorToConsole("Invalid player position", "Valid player positions are n/e/s/w");
                }
            }
        }

        private static void WriteUserErrorToConsole(string errMessage, string moreInfo)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{errMessage}");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{moreInfo}\n");
        }

        private static string ReadLine()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            string input = Console.ReadLine().ToLower();

            Console.ForegroundColor = ConsoleColor.White;

            return input;
        }
    }
}
