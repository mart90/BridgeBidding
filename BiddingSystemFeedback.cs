using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeBidding
{
    class BiddingSystemFeedback
    {
        public BiddingSystemFeedback() { }

        public BiddingSystemFeedback(Bid bid)
        {
            Bid = bid;
        }

        public Bid Bid { get; set; }
        public string ExtraRequirements { get; set; }
        public string Reason { get; set; }
        public string NextBid { get; set; }
        public string PartnerInfo { get; set; }

        public void ToConsole()
        {
            if (PartnerInfo != null)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\nPartner has {PartnerInfo}.");
                Console.ResetColor();
            }

            if (Bid.IsPass)
            {
                if (!Reason.StartsWith("Opener's rebids not yet implemented"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("\nPass");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"\nBid {Bid.ToString()}");

                if (NextBid != null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($" then {NextBid}");
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            if (ExtraRequirements != null)
            {
                Console.Write(" " + ExtraRequirements);
            }

            Console.WriteLine();

            if (Reason != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                if (!Reason.StartsWith("Opener's rebids not yet implemented"))
                {
                    Console.WriteLine($"Reason: {Reason}");
                }
                else
                {
                    Console.WriteLine(Reason);
                }
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        public void ModifyLevelIfOpponentRaised(BiddingRound round)
        {
            Bid lastOpponentBid = round.LastOpponentBid();

            if (lastOpponentBid.IsPass)
            {
                return;
            }

            if (Bid.IsHigherThan(lastOpponentBid))
            {
                return;
            }

            int levelDiff = Bid.LevelDifference(lastOpponentBid, Bid);

            if (levelDiff == 0)
            {
                // We wanted to bid the same as our opponent
                levelDiff = 1;
            }

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\nPartner was overcalled. We need to raise our bid {levelDiff} level(s) to be able to respond.\nPartner will interpret it as if it was {levelDiff} level(s) lower to get the correct read on our hand.\nOnly bid this if it's within reason.");
            Console.ResetColor();

            Bid.NumberOfTricks += levelDiff;
        }
    }
}
