using System.Collections.Generic;
using System.Linq;

namespace BridgeBidding
{
    class Player
    {
        public Player(PlayerPosition position)
        {
            Bids = new List<Bid>();

            Position = position;
        }

        public bool IsToBid { get; set; }
        public PlayerPosition Position { get; set; }
        public List<Bid> Bids { get; private set; }

        public List<Bid> Raises => Bids.Where(e => e.IsRaise).ToList();

        public void MakeBid(Bid bid)
        {
            bid.PlayerPosition = Position;
            Bids.Add(bid);
        }
    }
}
