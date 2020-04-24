namespace BridgeBidding
{
    static class ExtensionMethods
    {
        public static bool IsBetween(this int number, int lowerLimit, int upperLimit)
        {
            return number >= lowerLimit && number <= upperLimit;
        }

        public static bool IsMajor(this Suit suit)
        {
            return suit == Suit.Spades || suit == Suit.Hearts;
        }
    }
}
