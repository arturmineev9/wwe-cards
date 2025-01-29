using WWECardsGame.Entities;

namespace WWECardsGame
{
    public class CardRepository
    {
        private static readonly List<Card> AllCards = new()
        {
            new Card("John Cena", "Male", 100, 100, 100, 100),
            new Card("The Rock", "Male", 95, 90, 85, 100),
            new Card("Rey Mysterio", "Male", 88, 87, 92, 85),
            new Card("Roman Reigns", "Male", 92, 89, 85, 90),
            new Card("Charlotte Flair", "Female", 85, 80, 88, 92),
            new Card("Sasha Banks", "Female", 80, 78, 82, 90),
            new Card("Kurt Angle", "Male", 88, 90, 92, 85),
            new Card("Triple H", "Male", 90, 93, 85, 88),
            new Card("Brock Lesnar", "Male", 98, 95, 90, 75),
            new Card("AJ Styles", "Male", 85, 82, 88, 91),
            new Card("Becky Lynch", "Female", 88, 85, 80, 90),
            new Card("Ronda Rousey", "Female", 92, 89, 87, 80),
            new Card("Edge", "Male", 89, 88, 86, 87),
            new Card("Randy Orton", "Male", 92, 90, 85, 88),
            new Card("Shawn Michaels", "Male", 90, 87, 86, 95),
            new Card("Trish Stratus", "Female", 86, 82, 84, 90),
            new Card("Lita", "Female", 84, 80, 83, 89),
            new Card("CM Punk", "Male", 88, 85, 84, 91),
            new Card("Daniel Bryan", "Male", 87, 86, 89, 88)
        };

        private static readonly Random Random = new();

        public static (List<Card> player1Deck, List<Card> player2Deck) GetRandomDecks()
        {
            var shuffledCards = AllCards.OrderBy(_ => Random.Next()).ToList();
            
            var player1Males = shuffledCards.Where(c => c.Gender == "Male").Take(4).ToList();
            var player1Females = shuffledCards.Where(c => c.Gender == "Female").Take(2).ToList();
            var player1Deck = player1Males.Concat(player1Females).ToList();
            
            var remainingCards = shuffledCards.Except(player1Deck).ToList();
            var player2Males = remainingCards.Where(c => c.Gender == "Male").Take(4).ToList();
            var player2Females = remainingCards.Where(c => c.Gender == "Female").Take(2).ToList();
            var player2Deck = player2Males.Concat(player2Females).ToList();
            
            return (player1Deck, player2Deck);
        }
    }
}
