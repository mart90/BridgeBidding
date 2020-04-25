using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeBidding
{
    class BiddingSystemLogic
    {
        public BiddingSystemLogic(BiddingRound biddingRound, Hand userHand)
        {
            BiddingRound = biddingRound;
            UserHand = userHand;
        }

        public BiddingRound BiddingRound { get; set; }
        public Hand UserHand { get; set; }

        public BiddingSystemFeedback Pass(string partnerInfo = null) => new BiddingSystemFeedback
        {
            Bid = new Bid().SetPass(),
            PartnerInfo = partnerInfo
        };

        /// <summary>
        /// We are opening the bidding
        /// </summary>
        public BiddingSystemFeedback OpeningBid()
        {
            int highestCardCount = UserHand.MaxAmountOfCardsInASuit;
            int lowestCardCount = UserHand.MinAmountOfCardsInASuit;
            int hcp = UserHand.HighCardPoints;

            if (hcp < 5)
            {
                return Pass();
            }

            if (hcp + UserHand.LongSuitPoints() < 12)
            {
                if (highestCardCount < 6)
                {
                    return Pass();
                }

                // We have a 6+ card suit

                List<SuitQuality> longestSuits = UserHand.LongestSuits;

                if (highestCardCount == 6)
                {
                    if (UserHand.LongestSuits.All(e => e.Suit == Suit.Clubs))
                    {
                        // A bid of 2 clubs means something else. Only bid 3 clubs if we are strong
                        if (hcp >= 8)
                        {
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = Suit.Clubs, NumberOfTricks = 3 },
                                ExtraRequirements = "only with 2+ honors",
                                Reason = "8-10 HCP and 6 Clubs. Can't bid 2 Clubs because it has special meaning, so we jump to 3 but only with good quality."
                            };
                        }
                        else
                        {
                            return Pass();
                        }
                    }

                    var feedback = new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = UserHand.LowestRankingLongestNonClubsSuit.Suit, NumberOfTricks = 2 },
                        ExtraRequirements = "only with 2+ honors",
                        Reason = "5-10 HCP but one or more 6-card suits. We bid the lowest ranking one to give more space for our partner to bid new suits."
                    };

                    if (UserHand.LongestSuits.Any(e => e.Suit == Suit.Clubs))
                    {
                        feedback.Reason += " We don't bid 2 Clubs because that has another, specific meaning.";
                    }

                    return feedback;
                }

                // There can only be one longest suit if we have more than 6 cards in it
                SuitQuality longestSuit = UserHand.LongestSuits.Single();

                if (highestCardCount == 7)
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = longestSuit.Suit, NumberOfTricks = 3 },
                        ExtraRequirements = "only with 2+ honors",
                        Reason = "5-10 HCP but a 7-card suit."
                    };
                }
                if (highestCardCount == 8)
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = longestSuit.Suit, NumberOfTricks = 4 },
                        ExtraRequirements = "only with 2+ honors",
                        Reason = "5-10 HCP but an 8+ card suit."
                    };
                }
            }

            // 12+ HCP

            if (UserHand.IsBalanced() && hcp < 15)
            {
                if (hcp == 12 && lowestCardCount == 3)
                {
                    // 4 3 3 3 hand doesn't pass rule of 20. Pass
                    return Pass();
                }

                if (UserHand.MaxAmountOfCardsInASuit == 5 && UserHand.LongestSuits.Single().Suit.IsMajor())
                {
                    // 5 3 3 2 with a 5-card major

                    Suit suit = UserHand.LongestSuits.Single().Suit;
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = suit, NumberOfTricks = 1 },
                        Reason = $"Balanced hand with 12-14 total points and 5 {suit.ToString()}."
                    };
                }

                // We're bidding a minor suit

                List<SuitQuality> minorSuits = UserHand.MinorSuits;

                if (minorSuits.All(e => e.AmountOfCards == 4))
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.Diamonds, NumberOfTricks = 1 },
                        Reason = "Balanced hand, less than 15 HCP, 4 Clubs and 4 Diamonds."
                    };
                }
                if (minorSuits.All(e => e.AmountOfCards == 3))
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.Clubs, NumberOfTricks = 1 },
                        Reason = "Balanced hand, less than 15 HCP, 3 Clubs and 3 Diamonds."
                    };
                }

                Suit minorSuitToBid = minorSuits.OrderByDescending(e => e.AmountOfCards).First().Suit;
                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = minorSuitToBid, NumberOfTricks = 1 },
                    Reason = $"Balanced hand, less than 15 HCP, {minorSuitToBid.ToString()} is the longer minor."
                };
            }

            // Balanced hand and 15+ HCP
            // OR unbalanced hand and 12+ HCP

            if (UserHand.IsBalanced())
            {
                if (hcp <= 18)
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 1 },
                        Reason = "Balanced hand, 15-18 HCP."
                    };
                }
                if (hcp <= 21)
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 2 },
                        Reason = "Balanced hand, 19-21 HCP."
                    };
                }

                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = Suit.Clubs, NumberOfTricks = 2 },
                    Reason = "Balanced hand, 22+ HCP."
                };
            }

            // Unbalanced hand and 12+ HCP

            if (hcp + UserHand.LongSuitPoints() >= 22)
            {
                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = Suit.Clubs, NumberOfTricks = 2 },
                    Reason = "22+ points with a 5+ card suit."
                };
            }

            // Unbalanced hand and 12-21 total points

            if (UserHand.LowestRankingLongestMajor.AmountOfCards >= 5)
            {
                // Prefer majors
                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = UserHand.LowestRankingLongestMajor.Suit, NumberOfTricks = 1 },
                    Reason = "Unbalanced hand with 12-21 total points. If there are multiple long major suits we bid the lowest ranking one first."
                };
            }

            // No 5+ card majors

            return new BiddingSystemFeedback
            {
                Bid = new Bid { Suit = UserHand.LowestRankingLongestSuit.Suit, NumberOfTricks = 1 },
                Reason = "Unbalanced hand with 12-21 total points, no 5+ card majors. If there are multiple long minor suits we bid the longest, lowest ranking one first."
            };
        }

        /// <summary>
        /// We are responding to partner's opening bid
        /// </summary>
        public BiddingSystemFeedback RespondingBid()
        {
            Bid lastPartnerBid = BiddingRound.LastPartnerBid;
            int partnerBidLevel = lastPartnerBid.NumberOfTricks;
            Suit partnerBidSuit = lastPartnerBid.Suit;

            int highestCardCount = UserHand.MaxAmountOfCardsInASuit;
            int lowestCardCount = UserHand.MinAmountOfCardsInASuit;

            int highestMajorCardCount = UserHand.MajorSuits.Max(e => e.AmountOfCards);
            int highestMinorCardCount = UserHand.MinorSuits.Max(e => e.AmountOfCards);

            int hcp = UserHand.HighCardPoints;

            string partnerInfo;

            if (partnerBidLevel == 1)
            {
                if (partnerBidSuit == Suit.NoTrump)
                {
                    // Partner bid 1 NT
                    partnerInfo = "15-17 HCP and a balanced hand";

                    if (hcp.IsBetween(0, 7))
                    {
                        if (UserHand.IsBalanced() || highestCardCount < 5)
                        {
                            return Pass(partnerInfo);
                        }
                        else
                        {
                            // Transfer

                            if (highestMajorCardCount >= 5 && highestMinorCardCount <= 6)
                            {
                                // We have a long major. Transfer to the lowest ranking one
                                Suit longMajor = UserHand.LowestRankingLongestMajor.Suit;

                                if (longMajor == Suit.Hearts)
                                {
                                    return new BiddingSystemFeedback
                                    {
                                        Bid = new Bid { Suit = Suit.Diamonds, NumberOfTricks = 2 },
                                        PartnerInfo = partnerInfo,
                                        NextBid = "Pass",
                                        Reason = "Response to 1 NT. 0-7 HCP, 5+ Hearts. Transfer bid. We bid Diamonds to show partner we have Hearts."
                                    };
                                }
                                else
                                {
                                    return new BiddingSystemFeedback
                                    {
                                        Bid = new Bid { Suit = Suit.Hearts, NumberOfTricks = 2 },
                                        PartnerInfo = partnerInfo,
                                        NextBid = "Pass",
                                        Reason = "Response to 1 NT. 0-7 HCP, 5+ Spades. Transfer bid. We bid Hearts to show partner we have Spades."
                                    };
                                }
                            }

                            if (highestMinorCardCount >= 6)
                            {
                                // We have at least one very long minor
                                return new BiddingSystemFeedback
                                {
                                    Bid = new Bid { Suit = Suit.Spades, NumberOfTricks = 2 },
                                    PartnerInfo = partnerInfo,
                                    NextBid = "Pass",
                                    Reason = "Response to 1 NT. 0-7 HCP, 6+ card minor. Partner MUST bid 3 Clubs, which we will then either pass or convert to 3 Diamonds. A fit is guaranteed because partner has at least 2 cards of each suit."
                                };
                            }

                            return Pass(partnerInfo);
                        }
                    }

                    if (hcp.IsBetween(8, 15))
                    {
                        if (UserHand.IsBalanced() || (highestMinorCardCount >= 5 && highestMajorCardCount <= 3))
                        {
                            if (hcp < 10)
                            {
                                return new BiddingSystemFeedback
                                {
                                    Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 2 },
                                    PartnerInfo = partnerInfo,
                                    Reason = "Response to 1 NT. 8-9 HCP, balanced hand or 5+ minor and no 5+ major."
                                };
                            }
                            else
                            {
                                return new BiddingSystemFeedback
                                {
                                    Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 3 },
                                    PartnerInfo = partnerInfo,
                                    Reason = "Response to 1 NT. 10-15 HCP, balanced hand or 5+ minor and no 5+ major."
                                };
                            }
                        }

                        // Unbalanced hand with at least one 4+ card major

                        if (highestMajorCardCount == 4)
                        {
                            // Stayman. We are checking if we have a major suit fit

                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = Suit.Clubs, NumberOfTricks = 2 },
                                PartnerInfo = partnerInfo,
                                Reason = "Response to 1 NT. Stayman convention. Partner will bid 2 Diamonds if they have no 4-card major. This does not mean they have many Diamonds."
                            };
                        }

                        // 10-15 HCP and at least one 5+ card major. Transfer

                        Suit longMajor = UserHand.LowestRankingLongestMajor.Suit;

                        string nextBid;

                        if (highestMajorCardCount == 6)
                        {
                            if (hcp >= 10)
                            {
                                nextBid = "4 of your longest, lowest ranking major";
                            }
                            else
                            {
                                nextBid = "3 of your longest, lowest ranking major";
                            }
                        }
                        else
                        {
                            if (hcp >= 10)
                            {
                                nextBid = "2 NT";
                            }
                            else
                            {
                                nextBid = "3 NT";
                            }
                        }

                        if (longMajor == Suit.Hearts)
                        {
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = Suit.Diamonds, NumberOfTricks = 2 },
                                PartnerInfo = partnerInfo,
                                NextBid = nextBid,
                                Reason = "Response to 1 NT. 8+ HCP, 5+ Hearts. Transfer bid. We bid Diamonds to show partner we have Hearts."
                            };
                        }
                        else
                        {
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = Suit.Hearts, NumberOfTricks = 2 },
                                PartnerInfo = partnerInfo,
                                NextBid = nextBid,
                                Reason = "Response to 1 NT. 8+ HCP, 5+ Spades. Transfer bid. We bid Hearts to show partner we have Spades."
                            };
                        }
                    }

                    // 16+ HCP

                    if (UserHand.IsBalanced())
                    {
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 4 },
                            PartnerInfo = partnerInfo,
                            Reason = "Response to 1 NT. 16+ HCP, balanced hand. We are inviting 6 NT."
                        };
                    }

                    // 16+ HCP, unbalanced hand

                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = UserHand.LowestRankingLongestSuit.Suit, NumberOfTricks = 4 },
                        PartnerInfo = partnerInfo,
                        Reason = "Response to 1 NT. 16+ HCP, unbalanced hand. We bid our lowest ranking, longest suit first. 11-12 tricks should be possible."
                    };
                }

                // Partner bid 1 of a suit (non-NT)

                if (partnerBidSuit.IsMajor())
                {
                    partnerInfo = $"12-21 total points and 5+ {partnerBidSuit.ToString()}";
                }
                else if (partnerBidSuit == Suit.Clubs)
                {
                    partnerInfo = "12-21 total points and either 5+ Clubs or 3 Clubs and 3 Diamonds";
                }
                else // Diamonds
                {
                    partnerInfo = "12-21 total points and either 5+ Diamonds or 4 Clubs and 4 Diamonds";
                }

                if (hcp + UserHand.ShortSuitPoints() <= 5)
                {
                    return Pass(partnerInfo);
                }

                bool majorSuitFit = false;

                if (partnerBidSuit.IsMajor())
                {
                    SuitQuality mySuit = UserHand.SuitQualities.Single(e => e.Suit == partnerBidSuit);

                    if (mySuit.AmountOfCards >= 3)
                    {
                        // Major suit fit

                        if (hcp + UserHand.ShortSuitPoints() <= 9)
                        {
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 2 },
                                PartnerInfo = partnerInfo,
                                Reason = $"Response to 1 {partnerBidSuit.ToString()}. 6-9 total points. Confirming fit."
                            };
                        }
                        if (hcp + UserHand.ShortSuitPoints() <= 12)
                        {
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 3 },
                                PartnerInfo = partnerInfo,
                                Reason = $"Response to 1 {partnerBidSuit.ToString()}. 10-12 total points. Confirming fit and inviting game."
                            };
                        }

                        majorSuitFit = true;
                    }
                }

                string majorSuitFitReason = $"Response to 1 {partnerBidSuit.ToString()}. 13+ total points, major suit fit. We bid a new suit to check NT compatibility, but can still come back to the original suit if needed.";
                string jumpIfStrong = " We jump a level because we have 19+ points and a 5+ card suit";

                // We either have no major suit fit, or we have a major suit fit and 13+ HCP

                var suitsWeCanBidAtOneLevel = UserHand.SuitQualities.Where(e => (int)e.Suit < (int)partnerBidSuit);

                foreach (SuitQuality suitQuality in suitsWeCanBidAtOneLevel)
                {
                    if (suitQuality.AmountOfCards >= 4)
                    {
                        if (hcp + UserHand.LongSuitPoints() >= 19 && suitQuality.AmountOfCards >= 5)
                        {
                            // Jump if we have 19+ points and a 5+ card suit
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = UserHand.LowestRankingLongestSuit.Suit, NumberOfTricks = 2 },
                                PartnerInfo = partnerInfo,
                                Reason = majorSuitFit ? majorSuitFitReason + jumpIfStrong : $"Response to 1 {partnerBidSuit.ToString()}. 19+ total points. We jump bid a new 5+ card suit."
                            };
                        }

                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = suitQuality.Suit, NumberOfTricks = 1 },
                            PartnerInfo = partnerInfo,
                            Reason = majorSuitFit ? majorSuitFitReason : $"Response to 1 {partnerBidSuit.ToString()}. 6+ total points. We bid a new 4+ card suit."
                        };
                    }
                }

                // We didn't have a 4+ card suit we could bit at the one level

                if (hcp + UserHand.LongSuitPoints() >= 11)
                {
                    // We have enough points to bid at the 2 level

                    if (UserHand.LowestRankingLongestSuit.AmountOfCards >= 4)
                    {
                        if (hcp >= 19 && UserHand.LowestRankingLongestSuit.AmountOfCards >= 5)
                        {
                            // Jump if we have 19+ points and a 5+ card suit
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = UserHand.LowestRankingLongestSuit.Suit, NumberOfTricks = 3 },
                                PartnerInfo = partnerInfo,
                                Reason = majorSuitFit ? majorSuitFitReason + jumpIfStrong : $"Response to 1 {partnerBidSuit.ToString()}. 19+ total points. We jump bid a new 5+ card suit."
                            };
                        }

                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = UserHand.LowestRankingLongestSuit.Suit, NumberOfTricks = 2 },
                            PartnerInfo = partnerInfo,
                            Reason = majorSuitFit ? majorSuitFitReason : $"Response to 1 {partnerBidSuit.ToString()}. 11+ total points. We bid a new suit at the 2 level."
                        };
                    }
                }

                // We might get here if we have a major suit fit and 13+ points, but no other 4+ card suits?
                // In that case, just confirm the fit

                if (hcp + UserHand.ShortSuitPoints() >= 13 && UserHand.SuitQualities.Single(e => e.Suit == partnerBidSuit).AmountOfCards >= 3)
                {
                    if (hcp + UserHand.ShortSuitPoints() >= 16)
                    {
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 4 },
                            PartnerInfo = partnerInfo,
                            Reason = $"Response to 1 {partnerBidSuit.ToString()}. 13+ total points and major suit fit. We had no new 4+ card suits to bid. Confirm the fit and bid game immediately."
                        };
                    }

                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 3 },
                        PartnerInfo = partnerInfo,
                        Reason = $"Response to 1 {partnerBidSuit.ToString()}. 13+ total points and major suit fit. We had no other 4+ card suits to bid. Confirm the fit and invite game."
                    };
                }

                // Who knows how we get here. But the cheat sheet says we can...

                if (!partnerBidSuit.IsMajor())
                {
                    // Partner bid 1 of a minor. See if we have a fit

                    int numberOfTricks = hcp + UserHand.ShortSuitPoints() >= 10 ? 3 : 2;

                    if (partnerBidSuit == Suit.Clubs && UserHand.Clubs.AmountOfCards >= 5)
                    {
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = Suit.Clubs, NumberOfTricks = numberOfTricks },
                            PartnerInfo = partnerInfo,
                            Reason = $"Reponse to 1 {partnerBidSuit.ToString()}. Clubs fit."
                        };
                    }
                    if (partnerBidSuit == Suit.Diamonds && UserHand.Diamonds.AmountOfCards >= 4)
                    {
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = Suit.Diamonds, NumberOfTricks = numberOfTricks },
                            PartnerInfo = partnerInfo,
                            Reason = $"Reponse to 1 {partnerBidSuit.ToString()}. Diamonds fit."
                        };
                    }
                }

                if (hcp <= 11)
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 1 },
                        PartnerInfo = partnerInfo,
                        Reason = $"Response to 1 {partnerBidSuit.ToString()}. 6-11 HCP. No fit and no new suits to bid."
                    };
                }

                if (hcp <= 15)
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 2 },
                        PartnerInfo = partnerInfo,
                        Reason = $"Response to 1 {partnerBidSuit.ToString()}. 12-15 HCP. No fit and no new suits to bid."
                    };
                }

                // 16+ HCP, no possible fits. We bid game in NT

                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 3 },
                    PartnerInfo = partnerInfo,
                    Reason = $"Response to 1 {partnerBidSuit.ToString()}. 16+ HCP. No fit and no new suits to bid."
                };
            }

            // Partner's bid was at the 2 level or higher

            if (partnerBidSuit == Suit.NoTrump && partnerBidLevel == 2)
            {
                // Partner bid 2 NT
                partnerInfo = "20-21 HCP and a balanced hand";

                if (hcp <= 3)
                {
                    return Pass(partnerInfo);
                }

                if (hcp < 12)
                {
                    if (UserHand.IsBalanced() || UserHand.LongestSuits.All(e => !e.Suit.IsMajor()))
                    {
                        // Balanced hand or long minor
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 3 },
                            PartnerInfo = partnerInfo,
                            Reason = $"Response to 2 NT. 4-11 HCP, balanced hand or long minor."
                        };
                    }

                    if (highestMajorCardCount >= 5)
                    {
                        // Transfer

                        Suit longMajor = UserHand.LowestRankingLongestMajor.Suit;

                        if (longMajor == Suit.Hearts)
                        {
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = Suit.Diamonds, NumberOfTricks = 3 },
                                PartnerInfo = partnerInfo,
                                NextBid = highestMajorCardCount >= 6 ? "4 Hearts" : "3 NT",
                                Reason = "Response to 2 NT. 4-11 HCP, 5+ Hearts. Transfer bid. We bid Diamonds to show partner we have Hearts."
                            };
                        }
                        else
                        {
                            return new BiddingSystemFeedback
                            {
                                Bid = new Bid { Suit = Suit.Hearts, NumberOfTricks = 3 },
                                PartnerInfo = partnerInfo,
                                NextBid = highestMajorCardCount >= 6 ? "4 Spades" : "3 NT",
                                Reason = "Response to 2 NT. 4-11 HCP, 5+ Spades. Transfer bid. We bid Hearts to show partner we have Spades."
                            };
                        }
                    }

                    if (highestMajorCardCount == 4)
                    {
                        // Stayman. We are checking if we have a major suit fit

                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = Suit.Clubs, NumberOfTricks = 3 },
                            PartnerInfo = partnerInfo,
                            Reason = "Response to 2 NT. Stayman convention. Partner will bid 3 Diamonds if they have no 4-card major. This does not mean they have many Diamonds."
                        };
                    }
                }

                // 12+ HCP

                if (UserHand.IsBalanced())
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 4 },
                        PartnerInfo = partnerInfo,
                        Reason = "Response to 2 NT. 12+ HCP, balanced hand. We are inviting 6 NT."
                    };
                }

                // According to cheat sheet we can't arrive here, but it seems possible

                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = UserHand.LongestSuits.First().Suit, NumberOfTricks = 4 },
                    PartnerInfo = partnerInfo,
                    Reason = "Response to 2 NT. 12+ HCP, unbalanced hand. The system doesn't have anything for this. Bid a long suit, and skip a level so partner doesn't misinterpret"
                };
            }

            // Partner bid 2+ of a suit

            if (partnerBidSuit == Suit.Clubs && partnerBidLevel == 2)
            {
                // Partner has 22+ HCP balanced or 22 total points unbalanced
                partnerInfo = "22+ HCP with a balanced hand, or 22+ total points with an unbalanced hand";

                if (hcp >= 7)
                {
                    // Positive response

                    if (UserHand.IsBalanced())
                    {
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 2 },
                            PartnerInfo = partnerInfo,
                            ExtraRequirements = hcp == 7 ? "only if you have an Ace and a King" : null,
                            Reason = "Response to 2 Clubs. 7+ HCP, balanced hand. We let partner know we have a balanced hand."
                        };
                    }

                    if (UserHand.LongestSuits.All(e => e.Suit == Suit.Diamonds))
                    {
                        // Jump a level to prevent partner from interpreting it as a negative response

                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = Suit.Diamonds, NumberOfTricks = 3 },
                            PartnerInfo = partnerInfo,
                            ExtraRequirements = hcp == 7 ? "only if you have an Ace and a King" : null,
                            Reason = "Response to 2 Clubs. 7+ HCP, long suit of Diamonds. We jump a level because 2 Diamonds is the negative response."
                        };
                    }

                    int numberOfTricks = UserHand.LowestRankingLongestSuit.Suit == Suit.Clubs ? 3 : 2;

                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = UserHand.LowestRankingLongestSuit.Suit, NumberOfTricks = numberOfTricks },
                        PartnerInfo = partnerInfo,
                        ExtraRequirements = hcp == 7 ? "only if you have an Ace and a King" : null,
                        Reason = "Reponse to 2 Clubs. 7+ HCP, unbalanced hand. We bid our longest suit."
                    };
                }

                // 0-6 HCP

                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = Suit.Diamonds, NumberOfTricks = 2 },
                    PartnerInfo = partnerInfo,
                    Reason = "Response to 2 Clubs. 0-6 HCP. Negative response."
                };
            }

            // Partner bid 2+ of a suit (and not 2 Clubs)

            if (partnerBidLevel == 2)
            {
                partnerInfo = $"5-10 HCP but 6 {partnerBidSuit.ToString()} with good suit quality";

                if (hcp + UserHand.ShortSuitPoints() < 15)
                {
                    if (UserHand.GetAmountOfCardsBySuit(partnerBidSuit) == 3)
                    {
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 3 },
                            PartnerInfo = partnerInfo,
                            Reason = $"Response to 2 {partnerBidSuit.ToString()}. 0-15 points and 3 card support."
                        };
                    }

                    if (UserHand.GetAmountOfCardsBySuit(partnerBidSuit) >= 4)
                    {
                        return new BiddingSystemFeedback
                        {
                            Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 4 },
                            PartnerInfo = partnerInfo,
                            Reason = $"Response to 2 {partnerBidSuit.ToString()}. 0-15 points and 4+ card support."
                        };
                    }

                    return Pass(partnerInfo);
                }

                // 15+ points

                if (UserHand.IsBalanced())
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 2 },
                        PartnerInfo = partnerInfo,
                        Reason = $"Response to 2 {partnerBidSuit.ToString()}. 15+ points and a balanced hand"
                    };
                }

                Suit lowestRankingLongestSuit = UserHand.LowestRankingLongestSuit.Suit;
                int numberOfTricks = (int)lowestRankingLongestSuit > (int)partnerBidSuit ? 3 : 2;

                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = lowestRankingLongestSuit, NumberOfTricks = numberOfTricks },
                    PartnerInfo = partnerInfo,
                    Reason = $"Response to 2 {partnerBidSuit.ToString()}. 15+ points and a long suit of {lowestRankingLongestSuit.ToString()}"
                };
            }

            // Partner bid 3+ of a suit

            if (partnerBidLevel == 3)
            {
                partnerInfo = $"5-10 HCP but 7 {partnerBidSuit.ToString()} with good suit quality";

                if (UserHand.GetAmountOfCardsBySuit(partnerBidSuit) >= 3)
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 4 },
                        PartnerInfo = partnerInfo,
                        Reason = $"Response to 3 {partnerBidSuit.ToString()}. 3+ card support."
                    };
                }

                if (hcp >= 16 && UserHand.IsBalanced())
                {
                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = Suit.NoTrump, NumberOfTricks = 3 },
                        PartnerInfo = partnerInfo,
                        ExtraRequirements = $"only if game is certain",
                        Reason = $"Response to 3 {partnerBidSuit.ToString()}. 16+ points and a balanced hand."
                    };
                }

                if (UserHand.LowestRankingLongestSuit.AmountOfCards >= 6)
                {
                    Suit lowestRankingLongestSuit = UserHand.LowestRankingLongestSuit.Suit;
                    int numberOfTricks = (int)lowestRankingLongestSuit > (int)partnerBidSuit ? 4 : 3;

                    return new BiddingSystemFeedback
                    {
                        Bid = new Bid { Suit = lowestRankingLongestSuit, NumberOfTricks = numberOfTricks },
                        PartnerInfo = partnerInfo,
                        Reason = $"Response to 3 {partnerBidSuit.ToString()}. 16+ points and 6+ {lowestRankingLongestSuit.ToString()}"
                    };
                }

                return Pass(partnerInfo);
            }

            if (partnerBidLevel <= 3)
            {
                // We somehow got here. Shouldn't have
                return new BiddingSystemFeedback
                {
                    Bid = new Bid().SetPass(),
                    Reason = "None of the programmed scenarios occurred. We shouldn't have arrived here. Improvise your bid."
                };
            }

            // Partner bid 4+ of a suit

            partnerInfo = $"5-10 HCP but 8+ {partnerBidSuit.ToString()} with good suit quality";

            if (UserHand.GetAmountOfCardsBySuit(partnerBidSuit) >= 3)
            {
                return new BiddingSystemFeedback
                {
                    Bid = new Bid { Suit = partnerBidSuit, NumberOfTricks = 5 },
                    PartnerInfo = partnerInfo,
                    Reason = $"Response to {partnerBidLevel} {partnerBidSuit.ToString()}. 3+ card support."
                };
            }

            return Pass(partnerInfo);
        }

        /// <summary>
        /// We opened and are responding to partner's response
        /// </summary>
        public BiddingSystemFeedback OpenersRebid()
        {
            Bid lastPartnerBid = BiddingRound.LastPartnerBid;
            int partnerBidLevel = lastPartnerBid.NumberOfTricks;
            Suit partnerBidSuit = lastPartnerBid.Suit;

            Bid openingBid = BiddingRound.OpeningBid;
            int openingBidLevel = openingBid.NumberOfTricks;
            Suit openingBidSuit = openingBid.Suit;

            Bid opponentOvercall = BiddingRound.LastBidBeforePartnerBid();

            int highestCardCount = UserHand.MaxAmountOfCardsInASuit;
            int lowestCardCount = UserHand.MinAmountOfCardsInASuit;

            int highestMajorCardCount = UserHand.MajorSuits.Max(e => e.AmountOfCards);
            int highestMinorCardCount = UserHand.MinorSuits.Max(e => e.AmountOfCards);

            int hcp = UserHand.HighCardPoints;

            if (!opponentOvercall.IsPass)
            {
                if (lastPartnerBid.IsPass)
                {
                    // Partner passed after overcall
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("\nPartner passed after an overcall. This could mean they have a slightly better hand than we think.");
                    Console.ResetColor();
                }
                else
                {
                    // Modify the partner bid level to stop us from misinterpreting their bid

                    int levelDiff = Bid.LevelDifference(lastPartnerBid, openingBid) - Bid.LevelDifference(opponentOvercall, openingBid);

                    if (levelDiff == 0)
                    {
                        // Overcall did not affect partner's response
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("\nWe were overcalled, but this didn't necessarily affect our partner's response. We are interpreting it normally.");
                        Console.ResetColor();
                    }
                    else
                    {
                        partnerBidLevel -= levelDiff;

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\nWe were overcalled, and this affected our partner's response. We are interpreting it as if they bid {levelDiff} level(s) lower.");
                        Console.ResetColor();
                    }
                }
            }

            string partnerInfo;

            if (openingBidLevel == 1 && openingBidSuit == Suit.NoTrump)
            {
                // We opened 1 NT

                if (lastPartnerBid.IsPass)
                {
                    partnerInfo = "0-7 HCP and a balanced hand";
                }
                else if (partnerBidLevel == 2 && partnerBidSuit == Suit.NoTrump)
                {
                    partnerInfo = "8-9 HCP and a balanced hand";
                }
                else if (partnerBidLevel == 3 && partnerBidSuit == Suit.NoTrump)
                {
                    partnerInfo = "10-15 HCP and a balanced hand";
                }
                else if (partnerBidLevel == 4 && partnerBidSuit == Suit.NoTrump)
                {
                    partnerInfo = "16-17 HCP and a balanced hand";
                }
                else
                {
                    // Partner responded 2 of a suit

                    if (partnerBidSuit == Suit.Clubs)
                    {
                        partnerInfo = "8+ HCP and at least one 4-card major suit. Partner bid according to Stayman convention";
                    }
                    else if (partnerBidSuit == Suit.Diamonds)
                    {
                        partnerInfo = "0+ HCP and 5+ Hearts. Transferring";
                    }
                    else if (partnerBidSuit == Suit.Hearts)
                    {
                        partnerInfo = "0+ HCP and 5+ Spades. Transferring";
                    }
                    else // Spades
                    {
                        partnerInfo = "0+ HCP and a 6+ card minor. Transferring";
                    }
                }
            }
            else if (openingBidLevel == 2 && openingBidSuit == Suit.NoTrump)
            {
                // We opened 2 NT

                if (lastPartnerBid.IsPass)
                {
                    partnerInfo = "0-3 HCP";
                }
                else if (partnerBidLevel == 3)
                {
                    if (partnerBidSuit == Suit.NoTrump)
                    {
                        partnerInfo = "4-11 HCP and a balanced hand or long minor";
                    }
                    else if (partnerBidSuit == Suit.Clubs)
                    {
                        partnerInfo = "4-11 HCP and at least one 4-card major suit. Partner bid according to Stayman convention";
                    }
                    else if (partnerBidSuit == Suit.Diamonds)
                    {
                        partnerInfo = "4-11 HCP and 5+ Hearts. Transferring";
                    }
                    else // Hearts
                    {
                        partnerInfo = "4-11 HCP and 5+ Spades. Transferring";
                    }
                }
                // Partner jumped to the 4 level
                else if (partnerBidSuit == Suit.NoTrump)
                {
                    partnerInfo = "12+ HCP and a balanced hand";
                }
                else
                {
                    partnerInfo = $"12+ HCP and a long suit of {partnerBidSuit.ToString()}";
                }
            }
            else if (openingBidLevel == 1)
            {
                // We opened one of a suit

                if (lastPartnerBid.IsPass)
                {
                    partnerInfo = "0-5 points";
                }
                else if (openingBidSuit.IsMajor() && partnerBidSuit == openingBidSuit)
                {
                    if (partnerBidLevel == 2)
                    {
                        partnerInfo = $"6-9 points and 3+ {partnerBidSuit.ToString()}";
                    }
                    else
                    {
                        partnerInfo = $"10-12 points and 3+ {partnerBidSuit.ToString()}";
                    }
                }
                else if (partnerBidSuit == openingBidSuit)
                {
                    int numberToFit;

                    if (partnerBidSuit == Suit.Clubs)
                    {
                        numberToFit = 5;
                    }
                    else // Diamonds
                    {
                        numberToFit = 4;
                    }

                    if (partnerBidLevel == 2)
                    {
                        partnerInfo = $"6-9 points and {numberToFit}+ {partnerBidSuit.ToString()}";
                    }
                    else
                    {
                        partnerInfo = $"10+ points and {numberToFit}+ {partnerBidSuit.ToString()}";
                    }
                }
                else if (partnerBidSuit != Suit.NoTrump)
                {
                    // Partner bid a new non-NT suit

                    if (Bid.LevelDifference(lastPartnerBid, openingBid) == 1)
                    {
                        if (partnerBidLevel == 1)
                        {
                            partnerInfo = $"6+ points and 4+ {partnerBidSuit.ToString()}";
                        }
                        else
                        {
                            partnerInfo = $"11+ points and 4+ {partnerBidSuit.ToString()}";
                        }
                    }
                    else
                    {
                        // Partner jumped
                        partnerInfo = $"19+ points and 5+ {partnerBidSuit.ToString()}";
                    }
                }
                else // Partner bid NT
                {
                    if (partnerBidLevel == 1)
                    {
                        partnerInfo = "6-11 HCP";
                    }
                    else if (partnerBidLevel == 2)
                    {
                        partnerInfo = "12-15 HCP";
                    }
                    else
                    {
                        partnerInfo = "16+ HCP";
                    }
                }
            }
            else if (openingBidLevel == 2)
            {
                if (openingBidSuit == Suit.Clubs)
                {
                    if (partnerBidSuit == Suit.Diamonds && partnerBidLevel == 2)
                    {
                        partnerInfo = "0-6 HCP";
                    }
                    else if (partnerBidSuit == Suit.NoTrump && partnerBidLevel == 2)
                    {
                        partnerInfo = "7+ HCP and a balanced hand";
                    }
                    else if (partnerBidSuit == Suit.Diamonds && partnerBidLevel == 3)
                    {
                        partnerInfo = "7+ HCP and 5+ Diamonds";
                    }
                    else
                    {
                        partnerInfo = $"7+ HCP and 5+ {partnerBidSuit.ToString()}";
                    }
                }
                else // Weak twos
                {
                    if (lastPartnerBid.IsPass)
                    {
                        partnerInfo = $"0-14 HCP and not enough {openingBidSuit.ToString()} support";
                    }
                    else if (partnerBidSuit == openingBidSuit)
                    {
                        if (partnerBidLevel == 3)
                        {
                            partnerInfo = $"0-14 HCP and 3 {partnerBidSuit.ToString()}";
                        }
                        else // 4 level
                        {
                            partnerInfo = $"0-14 HCP and 4 {partnerBidSuit.ToString()}";
                        }
                    }
                    else if (partnerBidSuit == Suit.NoTrump)
                    {
                        partnerInfo = "15 HCP and a balanced hand";
                    }
                    else // Partner bid a new non-NT suit
                    {
                        partnerInfo = $"15 HCP and 5+ {partnerBidSuit.ToString()}";
                    }
                }
            }
            else if (openingBidLevel == 3)
            {
                if (lastPartnerBid.IsPass)
                {
                    partnerInfo = $"0-15 HCP and not enough {openingBidSuit.ToString()} support";
                }
                else if (partnerBidSuit == openingBidSuit)
                {
                    if (partnerBidLevel == 4 && !openingBidSuit.IsMajor())
                    {
                        partnerInfo = $"0-15 HCP and 3+ {openingBidSuit.ToString()}";
                    }
                    else if (partnerBidLevel == 4 && openingBidSuit.IsMajor())
                    {
                        partnerInfo = $"3+ {openingBidSuit.ToString()}";
                    }
                    else // Partner bid game in minor
                    {
                        partnerInfo = $"16+ HCP and 3+ {openingBidSuit.ToString()}";
                    }
                }
                else if (partnerBidSuit != Suit.NoTrump)
                {
                    partnerInfo = $"16+ HCP and 6+ {partnerBidSuit.ToString()}";
                }
                else // Partner bid 3 NT
                {
                    partnerInfo = "16+ HCP and thinks game is certain";
                }
            }
            else if (openingBidLevel == 4)
            {
                if (partnerBidSuit == openingBidSuit)
                {
                    partnerInfo = $"3+ {openingBidSuit.ToString()}";
                }
                else
                {
                    partnerInfo = $"Less than 3 {openingBidSuit.ToString()}";
                }
            }
            else // We made an insane opening bid
            {
                partnerInfo = "??? Opening bid was out of bounds. No info on partner";
            }

            return new BiddingSystemFeedback
            {
                Bid = new Bid().SetPass(),
                PartnerInfo = partnerInfo,
                Reason = "Opener's rebids not yet implemented. See information about partner's hand above and look at cheat sheet."
            };
        }
    }
}
