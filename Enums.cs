namespace BridgeBidding
{
    enum Suit
    {
        NoTrump = 0,
        Spades = 1,
        Hearts = 2,
        Diamonds = 3,
        Clubs = 4
    }

    enum PlayerPosition
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    enum BiddingMode
    {
        Opening,
        Responding,
        OpenersRebid,
        Overcall,
        OvercallResponse,
        Improvise
    }
}
