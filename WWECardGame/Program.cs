using WWECardGame.Entities;

namespace WWECardGame;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Добро пожаловать в игру о битве рестлеров!");
        Thread.Sleep(2000);
        // Создаем тестовые карты
        var player1Deck = new List<Card>
        {
            new Card("John Cena", "Male", 100, 100, 100, 100),
            new Card("The Rock", "Male", 95, 90, 85, 100),
            new Card("Rey Mysterio", "Male", 88, 87, 92, 85),
            new Card("Roman Reigns", "Male", 92, 89, 85, 90),
            new Card("Charlotte Flair", "Female", 85, 80, 88, 92),
            new Card("Sasha Banks", "Female", 80, 78, 82, 90)
        };
        
        List<Card> player2Deck =
        [
            new("Kurt Angle", "Male", 88, 90, 92, 85),
            new("Triple H", "Male", 90, 93, 85, 88),
            new("Brock Lesnar", "Male", 98, 95, 90, 75),
            new("AJ Styles", "Male", 85, 82, 88, 91),
            new("Becky Lynch", "Female", 88, 85, 80, 90),
            new("Ronda Rousey", "Female", 92, 89, 87, 80)
        ];

        // Выводим карты игрока
        Console.WriteLine("\nТвоя колода:");
        foreach (var card in player1Deck)
        {
            Console.WriteLine(card);
        }

        Thread.Sleep(2000);
        Console.WriteLine("\nГотовьтесь к битве!");
        // Можно добавить игровой цикл здесь
        Thread.Sleep(2000);
        
        BattleManager battleManager = new BattleManager(player1Deck, player2Deck);
        battleManager.StartBattle();
    }
}