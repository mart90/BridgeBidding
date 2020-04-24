using System;

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
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine($"\nPartner has {PartnerInfo}.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (Bid.IsPass)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("\nPass");
                Console.ForegroundColor = ConsoleColor.White;
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
                Console.WriteLine($"Reason: {Reason}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine();
        }
    }
}
