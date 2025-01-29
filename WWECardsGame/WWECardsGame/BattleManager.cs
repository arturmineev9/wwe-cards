using System.Net.Sockets;
using System.Text;
using WWECardsGame.Entities;
using WWECardsGame.Enum;

namespace WWECardsGame;

public class BattleManager
{
    // Колоды карт для каждого игрока
    private readonly List<Card> player1Deck;
    private readonly List<Card> player2Deck;

    // Сокеты для обмена данными с игроками
    private readonly Socket player1Socket;
    private readonly Socket player2Socket;

    // Конструктор для инициализации колод и сокетов
    public BattleManager(List<Card> player1Deck, List<Card> player2Deck, Socket player1Socket, Socket player2Socket)
    {
        this.player1Deck = player1Deck;
        this.player2Deck = player2Deck;
        this.player1Socket = player1Socket;
        this.player2Socket = player2Socket;
    }

    // Основной метод для начала битвы
    public async Task StartBattleAsync()
    {
        // Отправляем каждому игроку его колоду карт
        await SendToPlayerAsync(player1Socket, Protocol.CARDS, ConvertCardsToString(player1Deck));
        await SendToPlayerAsync(player2Socket, Protocol.CARDS, ConvertCardsToString(player2Deck));

        // Ждем подтверждения готовности от обоих игроков
        await WaitForConfirmation(player1Socket, Protocol.READY);
        await WaitForConfirmation(player2Socket, Protocol.READY);

        // Счетчики побед для каждого игрока
        int player1Wins = 0;
        int player2Wins = 0;

        // Основной цикл для проведения 3 раундов
        for (var round = 1; round <= 3; round++)
        {
            Console.WriteLine($"\nРаунд {round}:");

            // Определяем тип битвы и атрибуты для сравнения
            BattleType battleType = GetRandomBattleType();
            string attribute1 = GetRandomAttribute();
            string? attribute2 = GetRandomAttribute();
            if (attribute1 == attribute2) attribute2 = null;

            // Уведомляем игроков о начале раунда и атрибутах
            await NotifyPlayersAsync(battleType, attribute1, attribute2);

            // Получаем выбор карт от каждого игрока
            var results = await Task.WhenAll(
                GetPlayerSelectionAsync(player1Deck, battleType, player1Socket, player2Socket),
                GetPlayerSelectionAsync(player2Deck, battleType, player2Socket, player1Socket)
            );

            var player1Selection = results[0];
            var player2Selection = results[1];

            // Вычисляем очки для каждого игрока на основе выбранных карт и атрибутов
            int player1Score = CalculateScore(player1Selection, attribute1, attribute2);
            int player2Score = CalculateScore(player2Selection, attribute1, attribute2);

            Console.WriteLine($"Очки Игрока 1: {player1Score} | Очки Игрока 2: {player2Score}");
            await NotifyRoundResultsAsync(player1Score, player2Score);

            // Определяем победителя раунда и обновляем счетчики побед
            DetermineRoundWinner(ref player1Wins, ref player2Wins, player1Score, player2Score);
        }

        // Определяем победителя всей битвы
        DetermineBattleWinner(player1Wins, player2Wins);
    }

    // Метод для уведомления игроков о начале раунда и атрибутах
    private async Task NotifyPlayersAsync(BattleType battleType, string attribute1, string? attribute2)
    {
        string message = attribute2 == null
            ? $"{Protocol.ROUND_START} {battleType} {attribute1}"
            : $"{Protocol.ROUND_START} {battleType} {attribute1} {attribute2}";

        await Task.WhenAll(
            SendToPlayerAsync(player1Socket, message),
            SendToPlayerAsync(player2Socket, message)
        );

        // Ждем подтверждения готовности от обоих игроков
        await WaitForConfirmation(player1Socket, Protocol.ROUND_READY);
        await WaitForConfirmation(player2Socket, Protocol.ROUND_READY);
    }

    // Метод для получения выбора карты от игрока
    private async Task<List<Card>> GetPlayerSelectionAsync(List<Card> deck, BattleType battleType, Socket playerSocket, Socket opponentSocket)
    {
        // Фильтруем карты, которые подходят для текущего типа битвы
        List<Card> validCards = deck.FindAll(c => IsValidCardForBattle(c, battleType));
        if (validCards.Count == 0)
        {
            // Если подходящих карт нет, уведомляем игрока
            await SendToPlayerAsync(playerSocket, Protocol.NO_CARDS);
            return new List<Card>();
        }

        // Отправляем игроку список подходящих карт
        await SendToPlayerAsync(playerSocket, Protocol.SELECT_CARD, ConvertCardsToString(validCards));
        string response = await ReceiveFromPlayerWithTimeoutAsync(playerSocket);

        // Обрабатываем выбор игрока
        if (response.StartsWith(Protocol.SELECT))
        {
            int choice = int.Parse(response.Split(" ")[1]) - 1;
            return new List<Card> { validCards[choice] };
        }

        return new List<Card>();
    }

    // Метод для отправки сообщения игроку
    private static async Task SendToPlayerAsync(Socket playerSocket, string command, string message = "")
    {
        string fullMessage = $"{command} {message}".Trim();
        byte[] messageBytes = Encoding.UTF8.GetBytes(fullMessage);
        await playerSocket.SendAsync(messageBytes, SocketFlags.None);
    }

    // Метод для получения ответа от игрока с таймаутом
    private static async Task<string> ReceiveFromPlayerWithTimeoutAsync(Socket playerSocket, int timeoutSeconds = 30)
    {
        byte[] buffer = new byte[256];
        var task = playerSocket.ReceiveAsync(buffer, SocketFlags.None);
        if (await Task.WhenAny(task, Task.Delay(timeoutSeconds * 1000)) == task)
        {
            return Encoding.UTF8.GetString(buffer, 0, task.Result);
        }
        return Protocol.TIMEOUT;
    }

    // Метод для уведомления игроков о результатах раунда
    private async Task NotifyRoundResultsAsync(int player1Score, int player2Score)
    {
        string message = $"{Protocol.ROUND_RESULT} {player1Score} {player2Score}";
        await Task.WhenAll(
            SendToPlayerAsync(player1Socket, message),
            SendToPlayerAsync(player2Socket, message)
        );
    }

    // Метод для случайного выбора типа битвы
    private static BattleType GetRandomBattleType()
    {
        Random rand = new Random();
        BattleType[] values = { BattleType.MaleSingle, BattleType.FemaleSingle };
        return values[rand.Next(values.Length)];
    }

    // Метод для случайного выбора атрибута
    private static string GetRandomAttribute()
    {
        string[] attributes = { "Strength", "Toughness", "Endurance", "Charisma" };
        Random rand = new Random();
        return attributes[rand.Next(attributes.Length)];
    }

    // Метод для проверки, подходит ли карта для текущего типа битвы
    private static bool IsValidCardForBattle(Card card, BattleType battleType)
    {
        return (battleType == BattleType.MaleSingle || battleType == BattleType.MaleTagTeam) && card.Gender == "Male" ||
               (battleType == BattleType.FemaleSingle || battleType == BattleType.FemaleTagTeam) && card.Gender == "Female";
    }

    // Метод для вычисления очков на основе выбранных карт и атрибутов
    private int CalculateScore(List<Card> cards, string attr1, string? attr2 = null)
    {
        int score = 0;
        foreach (var card in cards)
        {
            score += card.GetAttribute(attr1);
            if (attr2 != null) score += card.GetAttribute(attr2);
        }
        return score;
    }

    // Метод для определения победителя раунда
    private static void DetermineRoundWinner(ref int player1Wins, ref int player2Wins, int player1Score, int player2Score)
    {
        if (player1Score > player2Score) player1Wins++;
        else if (player2Score > player1Score) player2Wins++;
    }

    // Метод для определения победителя всей битвы
    private static void DetermineBattleWinner(int player1Wins, int player2Wins)
    {
        Console.WriteLine("\nИтог игры:");
        Console.WriteLine(player1Wins > player2Wins ? "Игрок 1 - Победитель!" : player2Wins > player1Wins ? "Игрок 2 - Победитель!" : "Ничья!");
    }

    // Метод для преобразования списка карт в строку
    private static string ConvertCardsToString(List<Card> cards)
    {
        return string.Join("\n", cards.Select((card, index) => $"{index + 1}. {card.Name.Trim()} - Strength: {card.Strength}, Toughness: {card.Toughness}, Endurance: {card.Endurance}, Charisma: {card.Charisma}"));
    }

    // Метод для ожидания подтверждения от игрока
    private async Task WaitForConfirmation(Socket playerSocket, string expectedCommand)
    {
        string response = await ReceiveFromPlayerWithTimeoutAsync(playerSocket);
        if (response != expectedCommand)
        {
            await SendToPlayerAsync(playerSocket, Protocol.ERROR, "Неверный ответ");
        }
    }
}
