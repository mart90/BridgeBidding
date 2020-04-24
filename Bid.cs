namespace BridgeBidding
{
    class Bid
    {
        public Bid() { }

        public Bid(Suit suit, int numberOfTricks)
        {
            Suit = suit;
            NumberOfTricks = numberOfTricks;
        }

        public bool IsPass { get; set; }
        public bool IsDouble { get; set; }
        public bool IsRedouble { get; set; }
        public int NumberOfTricks { get; set; }
        public Suit Suit { get; set; }
        public PlayerPosition PlayerPosition { get; set; }

        public bool IsRaise => NumberOfTricks > 0;

        public Bid SetPass()
        {
            IsPass = true;
            return this;
        }

        public Bid SetDouble()
        {
            IsDouble = true;
            return this;
        }

        public Bid SetRedouble()
        {
            IsRedouble = true;
            return this;
        }

        public override string ToString()
        {
            string suit = Suit == Suit.NoTrump ? "NT" : Suit.ToString();
            return $"{NumberOfTricks} {suit}";
        }

        /// <summary>
        /// Returns 1 if level is the same
        /// </summary>
        public static int LevelDifference(Bid higherBid, Bid lowerBid)
        {
            int baseLevelDifference = higherBid.NumberOfTricks - lowerBid.NumberOfTricks;

            if ((int)higherBid.Suit < (int)lowerBid.Suit)
            {
                // Higher bid is a higher ranking suit. Add 1 to the base level diff
                return baseLevelDifference + 1;
            }

            return baseLevelDifference;
        }
    }
}
