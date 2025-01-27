using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WWECardsGame.Entities;
using WWECardsGame.Enum;

namespace WWECardsGame;

public class BattleManager(List<Card> player1Deck, List<Card> player2Deck, Socket player1Socket, Socket player2Socket)
{
    public async Task StartBattleAsync()
    {
        // Отправка списка карт игрокам
        await SendToPlayerAsync(player1Socket, $"Ваши карты:\n{ConvertCardsToString(player1Deck)}\n");
        await SendToPlayerAsync(player2Socket, $"Ваши карты:\n{ConvertCardsToString(player2Deck)}\n");
        
        int player1Wins = 0;
        int player2Wins = 0;

        for (var round = 1; round <= 3; round++)
        {
            Console.WriteLine($"\nРаунд {round}:");

            BattleType battleType = GetRandomBattleType();
            Console.WriteLine($"\nРаунд {round}:\nТип боя: {battleType}\n");

            string attribute1 = GetRandomAttribute();
            string? attribute2 = GetRandomAttribute();
            if (attribute1 == attribute2) attribute2 = null;

            Console.WriteLine(attribute2 == null
                ? $"Выбранный атрибут: {attribute1}"
                : $"Выбранные атрибуты: {attribute1}, {attribute2}");

            await NotifyPlayersAsync(battleType, attribute1, attribute2);

            // Получение выбора карт от игроков
            var results = await Task.WhenAll(
                GetPlayerSelectionAsync(player1Deck, battleType, player1Socket),
                GetPlayerSelectionAsync(player2Deck, battleType, player2Socket)
            );

            var player1Selection = results[0];
            var player2Selection = results[1];


            // Подсчёт очков
            int player1Score = CalculateScore(player1Selection, attribute1, attribute2);
            int player2Score = CalculateScore(player2Selection, attribute1, attribute2);

            Console.WriteLine($"Очки Игрока 1: {player1Score} | Очки Игрока 2: {player2Score}");
            await NotifyRoundResultsAsync(player1Score, player2Score);

            DetermineRoundWinner(ref player1Wins, ref player2Wins, player1Score, player2Score);
        }

        DetermineBattleWinner(player1Wins, player2Wins);
    }

    private async Task NotifyPlayersAsync(BattleType battleType, string attribute1, string? attribute2)
    {
        string message = attribute2 == null
            ? $"Тип боя: {battleType}, Атрибут: {attribute1}"
            : $"Тип боя: {battleType}, Атрибуты: {attribute1}, {attribute2}";

        await Task.WhenAll(
            SendToPlayerAsync(player1Socket, message),
            SendToPlayerAsync(player2Socket, message)
        );
    }

    private async Task<List<Card>> GetPlayerSelectionAsync(List<Card> deck, BattleType battleType, Socket playerSocket)
    {
        List<Card> validCards = deck.FindAll(c => IsValidCardForBattle(c, battleType));
        if (validCards.Count == 0)
        {
            await SendToPlayerAsync(playerSocket, "У вас нет подходящих карт для этого боя.");
            return new List<Card>();
        }

        await SendToPlayerAsync(playerSocket, "Выберите карту для боя:");
        for (int i = 0; i < validCards.Count; i++)
        {
            await SendToPlayerAsync(playerSocket, $"{i + 1}: {validCards[i]}\n");
        }

        int choice = int.Parse(await ReceiveFromPlayerAsync(playerSocket)) - 1;
        List<Card> selected = new() { validCards[choice] };

        if (battleType == BattleType.MaleTagTeam || battleType == BattleType.FemaleTagTeam)
        {
            await SendToPlayerAsync(playerSocket, "Выберите вторую карту для боя:");
            choice = int.Parse(await ReceiveFromPlayerAsync(playerSocket)) - 1;
            selected.Add(validCards[choice]);
        }

        return selected;
    }

    private static async Task SendToPlayerAsync(Socket playerSocket, string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await playerSocket.SendAsync(messageBytes, SocketFlags.None);
    }

    private static async Task<string> ReceiveFromPlayerAsync(Socket playerSocket)
    {
        byte[] buffer = new byte[256];
        int bytesReceived = await playerSocket.ReceiveAsync(buffer, SocketFlags.None);
        return Encoding.UTF8.GetString(buffer, 0, bytesReceived);
    }

    private async Task NotifyRoundResultsAsync(int player1Score, int player2Score)
    {
        string message = $"Очки Игрока 1: {player1Score} | Очки Игрока 2: {player2Score}";
        await Task.WhenAll(
            SendToPlayerAsync(player1Socket, message),
            SendToPlayerAsync(player2Socket, message)
        );
    }

    private static BattleType GetRandomBattleType()
    {
        Random rand = new Random();
        //BattleType[] values = { BattleType.MaleSingle, BattleType.FemaleSingle, BattleType.FemaleTagTeam, BattleType.MaleTagTeam };
        BattleType[] values = { BattleType.MaleSingle, BattleType.FemaleSingle, };
        return values[rand.Next(values.Length)];
    }

    private static string GetRandomAttribute()
    {
        string[] attributes = { "Strength", "Toughness", "Endurance", "Charisma" };
        Random rand = new Random();
        return attributes[rand.Next(attributes.Length)];
    }

    private static bool IsValidCardForBattle(Card card, BattleType battleType)
    {
        return (battleType == BattleType.MaleSingle || battleType == BattleType.MaleTagTeam) && card.Gender == "Male" ||
               (battleType == BattleType.FemaleSingle || battleType == BattleType.FemaleTagTeam) && card.Gender == "Female";
    }

    private int CalculateScore(List<Card> cards, string attr1, string? attr2 = null)
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

    private static void DetermineRoundWinner(ref int player1Wins, ref int player2Wins, int player1Score, int player2Score)
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

    private static string ConvertCardsToString(List<Card> cards)
    {
        return string.Join("\n", cards.Select((card, index) =>
            $"{index + 1}. {card.Name.Trim()} - Strength: {card.Strength}, Toughness: {card.Toughness}, Endurance: {card.Endurance}, Charisma: {card.Charisma}"));
    }

}
