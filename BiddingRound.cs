using System.Collections.Generic;
using System.Linq;

namespace BridgeBidding
{
    class BiddingRound
    {
        public BiddingRound(PlayerPosition userPosition, PlayerPosition firstToBid)
        {
            Players = new List<Player>();
            BiddingHistory = new List<Bid>();

            Players.AddRange(new List<Player>
            {
                new Player(PlayerPosition.North),
                new Player(PlayerPosition.East),
                new Player(PlayerPosition.South),
                new Player(PlayerPosition.West)
            });

            UserPosition = userPosition;
            BiddingMode = BiddingMode.Opening;

            GetPlayerByPosition(firstToBid).IsToBid = true;
        }

        public PlayerPosition UserPosition { get; set; }
        public BiddingMode BiddingMode { get; set; }
        public List<Player> Players { get; private set; }
        public List<Bid> BiddingHistory { get; private set; }

        public Player CurrentBiddingPlayer => Players.Single(e => e.IsToBid);
        public Player User => GetPlayerByPosition(UserPosition);
        public Player Partner => GetPlayerByPosition(GetOppositePlayerPosition(UserPosition));
        public Player Opener => GetPlayerByPosition(BiddingHistory.First(e => e.IsRaise).PlayerPosition);

        public Bid OpeningBid => BiddingHistory.FirstOrDefault(e => e.IsRaise);
        public Bid LatestNonPassBid => BiddingHistory.LastOrDefault(e => !e.IsPass);
        public Bid LatestRaise => BiddingHistory.LastOrDefault(e => e.IsRaise);
        public List<Bid> AllRaises => BiddingHistory.Where(e => e.IsRaise).ToList();
        public Bid LatestPartnerBid => Partner.Bids.Last();

        public Bid LatestOpponentBid()
        {
            PlayerPosition partnerPosition = GetOppositePlayerPosition(UserPosition);
            return BiddingHistory.LastOrDefault(e => e.PlayerPosition != UserPosition && e.PlayerPosition != partnerPosition);
        }

        public Bid LastRaiseBeforePartnerRaise()
        {
            int partnerBidIndex = BiddingHistory.IndexOf(LatestPartnerBid);
            return BiddingHistory.Take(partnerBidIndex).Last(e => e.IsRaise);
        }

        public bool BiddingEnded => BiddingHistory.Count > 3 && BiddingHistory.TakeLast(3).All(e => e.IsPass);

        public void AddBid(Bid bid)
        {
            CurrentBiddingPlayer.MakeBid(bid);

            BiddingHistory.Add(bid);

            if (bid.IsRaise)
            {
                SetNewBiddingMode();
            }
        }

        public void NextTurn()
        {
            PlayerPosition nextTurnPosition = GetNextPlayerPosition(CurrentBiddingPlayer.Position);

            CurrentBiddingPlayer.IsToBid = false;

            GetPlayerByPosition(nextTurnPosition).IsToBid = true;
        }

        private void SetNewBiddingMode()
        {
            Player raiser = CurrentBiddingPlayer;

            if (raiser == User)
            {
                if (BiddingMode == BiddingMode.Opening)
                {
                    // We opened
                    BiddingMode = BiddingMode.OpenersRebid;
                }
                else
                {
                    BiddingMode = BiddingMode.Improvise;
                }
            }
            else if (raiser == Partner)
            {
                if (BiddingMode == BiddingMode.Opening)
                {
                    // Partner opened
                    BiddingMode = BiddingMode.Responding;
                }
                else if (BiddingMode == BiddingMode.OpenersRebid)
                {
                    // Partner responded. No change
                    return;
                }
                else if (BiddingMode == BiddingMode.Overcall)
                {
                    // Partner overcalled
                    BiddingMode = BiddingMode.OvercallResponse;
                }
                else
                {
                    BiddingMode = BiddingMode.Improvise;
                }
            }
            else 
            {
                // Last raise was by an opponent

                if (BiddingMode == BiddingMode.Opening)
                {
                    // An opponent opened
                    BiddingMode = BiddingMode.Improvise;
                }
                else if (BiddingMode == BiddingMode.OpenersRebid)
                {
                    // Opponent overcalled. No change
                    return;
                }
                else if (BiddingMode == BiddingMode.Responding)
                {
                    // Opponent overcalled. No change
                    return;
                }
                else
                {
                    BiddingMode = BiddingMode.Improvise;
                }
            }
        }

        private static PlayerPosition GetOppositePlayerPosition(PlayerPosition position) => GetNextPlayerPosition(GetNextPlayerPosition(position));
        private static PlayerPosition GetPreviousPlayerPosition(PlayerPosition position) => GetNextPlayerPosition(GetNextPlayerPosition(GetNextPlayerPosition(position)));

        public static PlayerPosition GetNextPlayerPosition(PlayerPosition position)
        {
            return position switch
            {
                PlayerPosition.North => PlayerPosition.East,
                PlayerPosition.East => PlayerPosition.South,
                PlayerPosition.South => PlayerPosition.West,
                PlayerPosition.West => PlayerPosition.North,
            };
        }

        private Player GetPlayerByPosition(PlayerPosition position) => Players.Single(e => e.Position == position);
    }
}
