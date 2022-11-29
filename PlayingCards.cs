namespace PlayingCardsApp
{
    public class PlayingCards
    {
        public enum Suit { Clubs, Diamonds, Hearts, Spades }
        public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

        private static readonly Random random = new();
        private static IEnumerable<Suit> Suits { get; } = Enum.GetValues(typeof(Suit)).Cast<Suit>();
        private static IEnumerable<Rank> Ranks { get; } = Enum.GetValues(typeof(Rank)).Cast<Rank>();

        public class PlayingCard : IComparable<PlayingCard>
        {
            public class CardValue
            {
                private readonly int baseValue;
                public int Blackjack { get => (baseValue > 10 ? 11 : baseValue); }
                public CardValue(int baseValue)
                {
                    this.baseValue = baseValue;
                }
            }

            public Suit Suit { get; init; }
            public Rank Rank { get; init; }
            public CardValue Value { get; init; }

            public PlayingCard(Suit suit, Rank rank)
            {
                Suit = suit;
                Rank = rank;
                Value = new((int)rank);
            }

            public override string ToString()
            {
                return $"{Rank} of {Suit}";
            }

            /// <summary>
            /// Compares by suit (ascending), and then internally by rank (ascending)
            /// </summary>
            public int CompareTo(PlayingCard? other)
            {
                ArgumentNullException.ThrowIfNull(other);

                if (this.Suit != other.Suit)
                    return Suit.CompareTo(other.Suit);

                return Rank.CompareTo(other.Rank);
            }

            public static class Comparers
            {
                public static class BySuitAscending
                {
                    public static IComparer<PlayingCard> RankAscending => new CompareBySuitAscendingAndRankAscending();
                    public static IComparer<PlayingCard> RankDescending => new CompareBySuitAscendingAndRankDescending();
                }
                public static class BySuitDescending
                {
                    public static IComparer<PlayingCard> RankAscending => new CompareBySuitDescendingAndRankAscending();
                    public static IComparer<PlayingCard> RankDescending => new CompareBySuitDescendingAndRankDescending();

                }
                public static class ByRankAscending
                {
                    public static IComparer<PlayingCard> SuitAscending => new CompareByRankAscendingAndSuitAscending();
                    public static IComparer<PlayingCard> SuitDescending => new CompareByRankAscendingAndSuitDescending();
                }
                public static class ByRankDescending
                {
                    public static IComparer<PlayingCard> SuitAscending => new CompareByRankDescendingAndSuitAscending();
                    public static IComparer<PlayingCard> SuitDescending => new CompareByRankDescendingAndSuitDescending();
                }
            }

            #region Comparer Classes
            #region Compare By Suit First
            private class CompareBySuitAscendingAndRankAscending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Suit != y.Suit)
                        return x.Suit.CompareTo(y.Suit);

                    return x.Rank.CompareTo(y.Rank);
                }
            }

            private class CompareBySuitDescendingAndRankDescending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Suit != y.Suit)
                        return y.Suit.CompareTo(x.Suit);

                    return y.Rank.CompareTo(x.Rank);
                }
            }

            private class CompareBySuitAscendingAndRankDescending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Suit != y.Suit)
                        return x.Suit.CompareTo(y.Suit);

                    return y.Rank.CompareTo(x.Rank);
                }
            }

            private class CompareBySuitDescendingAndRankAscending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Suit != y.Suit)
                        return y.Suit.CompareTo(x.Suit);

                    return x.Rank.CompareTo(y.Rank);
                }
            }
            #endregion

            #region Compare By Rank First
            private class CompareByRankAscendingAndSuitAscending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Rank != y.Rank)
                        return x.Rank.CompareTo(y.Rank);

                    return x.Suit.CompareTo(y.Suit);
                }
            }
            private class CompareByRankDescendingAndSuitDescending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Rank != y.Rank)
                        return y.Rank.CompareTo(x.Rank);

                    return y.Suit.CompareTo(x.Suit);
                }
            }
            private class CompareByRankAscendingAndSuitDescending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Rank != y.Rank)
                        return x.Rank.CompareTo(y.Rank);

                    return y.Suit.CompareTo(x.Suit);
                }
            }
            private class CompareByRankDescendingAndSuitAscending : IComparer<PlayingCard>
            {
                public int Compare(PlayingCard? x, PlayingCard? y)
                {
                    ArgumentNullException.ThrowIfNull(x);
                    ArgumentNullException.ThrowIfNull(y);

                    if (x.Rank != y.Rank)
                        return y.Rank.CompareTo(x.Rank);

                    return x.Suit.CompareTo(y.Suit);
                }
            }
            #endregion
            #endregion

            public static class Factory
            {
                public static PlayingCard RandomCard()
                {
                    Suit randomSuit = (Suit)random.Next((int)Suits.Min(), Suits.Count());
                    Rank randomRank = (Rank)random.Next((int)Ranks.Min(), Ranks.Count());

                    return new PlayingCard(randomSuit, randomRank);
                }
            }
        }

        public class CardDeck
        {
            private PlayingCard[] playingCards;

            public PlayingCard this[int index] => playingCards[index];
            public int Count { get; init; }

            private CardDeck(int deckSize)
            {
                playingCards = new PlayingCard[deckSize];
                Count = deckSize;
            }

            public void Shuffle()
            {
                int index1 = playingCards.Length, index2;

                while (index1 > 1)
                {
                    index1--;
                    index2 = random.Next(index1 + 1);
                    (playingCards[index1], playingCards[index2]) = (playingCards[index2], playingCards[index1]);
                }
            }

            public void Sort()
            {
                if (playingCards.Length < 2)
                    return;

                for (int iteration = 0; iteration < playingCards.Length; iteration++)
                {
                    for (int card = 0; card < playingCards.Length - 1 - iteration; card++)
                    {
                        if (playingCards[card].CompareTo(playingCards[card + 1]) > 0 )
                            (playingCards[card], playingCards[card + 1]) = (playingCards[card + 1], playingCards[card]);

                    }
                }
            }

            public void Sort(IComparer<PlayingCard>? comparer)
            {
                ArgumentNullException.ThrowIfNull(comparer);

                if (playingCards.Length < 2)
                    return;

                for (int iteration = 0; iteration < playingCards.Length; iteration++)
                {
                    for (int card = 0; card < playingCards.Length - 1 - iteration; card++)
                    {
                        if (comparer.Compare(playingCards[card], playingCards[card + 1]) > 0)
                            (playingCards[card], playingCards[card + 1]) = (playingCards[card + 1], playingCards[card]);

                    }
                }
            }

            public override string ToString()
            {
                if (playingCards is null)
                    throw new InvalidOperationException("Deck does not exist.");

                string output = string.Empty;

                for (int i = 0; i < playingCards.Length; i++)
                {
                    if (playingCards[i] == null)
                        throw new InvalidOperationException($"Card in deck is missing at index {i}.");

                    output += playingCards[i].ToString() + "\n";
                }

                return output.TrimEnd('\n');
            }

            public static class Factory
            {
                public static CardDeck StandardDeck()
                {
                    CardDeck deck = new(52);
                    int index = 0;

                    for (int suit = (int)Suits.Min(); suit <= (int)Suits.Max(); suit++)
                    {
                        for (int rank = (int)Ranks.Min(); rank <= (int)Ranks.Max(); rank++)
                        {
                            deck.playingCards[index] = new PlayingCard((Suit)suit, (Rank)rank);
                            index++;
                        }
                    }

                    return deck;
                }
            }
        }
    }
}
