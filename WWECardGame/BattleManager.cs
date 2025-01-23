using WWECardGame.Entities;
using WWECardGame.Enum;

namespace WWECardGame;

internal class BattleManager(List<Card> player1Deck, List<Card> player2Deck)
{
    public void StartBattle()
    {
        int player1Wins = 0;
        int player2Wins = 0;

        for (int round = 1; round <= 3; round++)
        {
            int player1Score, player2Score;
            Console.WriteLine($"\nРаунд {round}:");
            BattleType battleType = GetRandomBattleType();
            Console.WriteLine($"Тип боя: {battleType}");

            var attribute1 = GetRandomAttribute();
            var attribute2 = GetRandomAttribute();

            Console.WriteLine(attribute1 == attribute2
                ? $"Выбранные атрибуты: {attribute1}"
                : $"Выбранные атрибуты: {attribute1}, {attribute2}");

            var player1Selection = SelectCardsForBattle(player1Deck, battleType, "Игрок 1");
            var player2Selection = SelectCardsForBattle(player2Deck, battleType, "Игрок 2");
            
            if (attribute1 == attribute2)
            {
                player1Score = CalculateScore(player1Selection, attribute1);
                player2Score = CalculateScore(player2Selection, attribute1);
            }
            else
            {
                player1Score = CalculateScore(player1Selection, attribute1, attribute2);
                player2Score = CalculateScore(player2Selection, attribute1, attribute2);
            }
            
            Console.WriteLine($"Очки Игрока 1: {player1Score} | Очки Игрока 2: {player2Score}");

            DetermineRoundWinner(ref player1Wins, ref player2Wins, player1Score, player2Score);
        }

        DetermineBattleWinner(player1Wins, player2Wins);
        
    }

    private void DetermineRoundWinner(ref int player1Wins, ref int player2Wins, int player1Score, int player2Score)
    {
        if (player1Score > player2Score)
        {
            Console.WriteLine("Игрок 1 выигрывает раунд!");
            player1Wins++;
        }
        else if (player2Score > player1Score)
        {
            Console.WriteLine("Игрок 2 выигрывает раунд!");
            player2Wins++;
        }
        else
        {
            Console.WriteLine("Ничья!");
        }
    }

    private static void DetermineBattleWinner(int player1Wins, int player2Wins)
    {
        Console.WriteLine("\nИтог игры:");
        if (player1Wins > player2Wins)
            Console.WriteLine("Игрок 1 - Победитель!");
        else if (player2Wins > player1Wins)
            Console.WriteLine("Игрок 2 - Победитель!");
        else
            Console.WriteLine("Игра окончилась вничью!");
    }

    private static BattleType GetRandomBattleType()
    {
        Random rand = new Random();
        BattleType[] values = (BattleType[])System.Enum.GetValues(typeof(BattleType));
        return values[rand.Next(values.Length)];
    }

    private static string GetRandomAttribute()
    {
        string[] attributes = ["Strength", "Toughness", "Endurance", "Charisma"];
        var rand = new Random();
        return attributes[rand.Next(attributes.Length)];
    }

    private List<Card> SelectCardsForBattle(List<Card> deck, BattleType battleType, string playerName)
    {
        List<Card> selected = new List<Card>();
        List<Card> validCards = deck.FindAll(c => IsValidCardForBattle(c, battleType));

        if (validCards.Count == 0)
        {
            Console.WriteLine($"{playerName}, у вас нет подходящих карт для этого боя.");
            return selected;
        }

        Console.WriteLine($"{playerName}, выберите карту для боя:");
        for (int i = 0; i < validCards.Count; i++)
        {
            Console.WriteLine($"{i + 1}: {validCards[i]}");
        }
        int choice = int.Parse(Console.ReadLine()) - 1;
        selected.Add(validCards[choice]);

        if (battleType == BattleType.MaleTagTeam || battleType == BattleType.FemaleTagTeam)
        {
            Console.WriteLine($"{playerName}, выберите вторую карту для боя:");
            choice = int.Parse(Console.ReadLine()) - 1;
            selected.Add(validCards[choice]);
        }
        return selected;
    }

    private static bool IsValidCardForBattle(Card card, BattleType battleType)
    {
        return (battleType == BattleType.MaleSingle || battleType == BattleType.MaleTagTeam) && card.Gender == "Male" ||
               (battleType == BattleType.FemaleSingle || battleType == BattleType.FemaleTagTeam) && card.Gender == "Female";
    }

    private int CalculateScore(List<Card> cards, string attr1, string attr2 = null)
    {
        int score = 0;
        foreach (var card in cards)
        {
            score += card.GetAttribute(attr1);
            if (attr2 != null)
            {
                score += card.GetAttribute(attr2);
            }
        }
        return score;
    }
}