using System.Collections.Generic;
using System.Linq;

namespace BridgeBidding
{
    class Hand
    {
        public Hand(int highCardPoints, int numberOfSpades, int numberOfHearts, int numberOfDiamonds, int numberOfClubs)
        {
            HighCardPoints = highCardPoints;

            SuitQualities = new List<SuitQuality>
            {
                new SuitQuality { Suit = Suit.Spades, AmountOfCards = numberOfSpades },
                new SuitQuality { Suit = Suit.Hearts, AmountOfCards = numberOfHearts },
                new SuitQuality { Suit = Suit.Diamonds, AmountOfCards = numberOfDiamonds },
                new SuitQuality { Suit = Suit.Clubs, AmountOfCards = numberOfClubs }
            };
        }

        public int HighCardPoints { get; set; }
        public List<SuitQuality> SuitQualities { get; private set; }
        
        public int MaxAmountOfCardsInASuit => SuitQualities.Max(e => e.AmountOfCards);
        public int MinAmountOfCardsInASuit => SuitQualities.Min(e => e.AmountOfCards);

        public SuitQuality Spades => SuitQualities.Single(e => e.Suit == Suit.Spades);
        public SuitQuality Hearts => SuitQualities.Single(e => e.Suit == Suit.Hearts);
        public SuitQuality Diamonds => SuitQualities.Single(e => e.Suit == Suit.Diamonds);
        public SuitQuality Clubs => SuitQualities.Single(e => e.Suit == Suit.Clubs);

        /// <summary>
        /// 4 3 3 3, 4 4 3 2 or 5 3 3 2
        /// </summary>
        public bool IsBalanced()
        {
            if (MaxAmountOfCardsInASuit > 5)
            {
                // False if 6+ card suit
                return false;
            }
            if (MinAmountOfCardsInASuit < 2)
            {
                // False if singleton or void suits
                return false;
            }
            if (MaxAmountOfCardsInASuit == 5)
            {
                // False if 5 4 2 2
                return !SuitQualities.Any(e => e.AmountOfCards == 4);
            }

            return true;
        }

        public List<SuitQuality> LongestSuits => SuitQualities
            .Where(e => e.AmountOfCards == MaxAmountOfCardsInASuit)
            .ToList();

        public SuitQuality LowestRankingLongestSuit => LongestSuits
            .OrderByDescending(e => (int)e.Suit)
            .First();

        public SuitQuality LowestRankingLongestMajor => SuitQualities
            .Where(e => e.Suit.IsMajor())
            .OrderByDescending(e => e.AmountOfCards)
            .ThenByDescending(e => (int)e.Suit)
            .First();

        public SuitQuality LowestRankingLongestNonClubsSuit => SuitQualities
            .Where(e => e.Suit != Suit.Clubs)
            .OrderByDescending(e => e.AmountOfCards)
            .ThenByDescending(e => (int)e.Suit)
            .First();

        public List<SuitQuality> MajorSuits => SuitQualities
            .Where(e => (int)e.Suit <= 2)
            .ToList();

        public List<SuitQuality> MinorSuits => SuitQualities
            .Where(e => (int)e.Suit > 2)
            .ToList();

        public int GetAmountOfCardsBySuit(Suit suit) => SuitQualities
            .Single(e => e.Suit == suit)
            .AmountOfCards;

        public int LongSuitPoints()
        {
            int longSuitPoints = 0;
            foreach (SuitQuality suitQuality in SuitQualities)
            {
                if (suitQuality.AmountOfCards > 4)
                {
                    longSuitPoints += suitQuality.AmountOfCards - 4;
                }
            }

            return longSuitPoints;
        }

        public int ShortSuitPoints()
        {
            int shortSuitPoints = 0;
            foreach (SuitQuality suitQuality in SuitQualities)
            {
                if (suitQuality.AmountOfCards <= 2)
                {
                    shortSuitPoints += suitQuality.AmountOfCards == 0 ? 5 : suitQuality.AmountOfCards == 1 ? 3 : 1;
                }
            }

            return shortSuitPoints;
        }
    }
}
