using System.Net.Sockets;
using System.Text;
using GameServer;
using WWECardsGame.Entities;
using WWECardsGame.Enum;

namespace WWECardsGame;

public class BattleManager(List<Card> player1Deck, List<Card> player2Deck, Socket player1Socket, Socket player2Socket)
{
    public async Task StartBattleAsync()
    {
        try
        {
            await InitializePlayersAsync();
            int player1Wins = 0, player2Wins = 0;

            for (var round = 1; round <= 3; round++)
            {
                try
                {
                    BattleType battleType = GetRandomBattleType();
                    string attribute1 = GetRandomAttribute();
                    string? attribute2 = GetRandomAttribute();
                    if (attribute1 == attribute2) attribute2 = null;

                    await NotifyPlayersAsync(battleType, attribute1, attribute2);

                    var results = await Task.WhenAll(
                        GetPlayerSelectionAsync(player1Deck, battleType, player1Socket, player2Socket),
                        GetPlayerSelectionAsync(player2Deck, battleType, player2Socket, player1Socket)
                    );

                    var player1Selection = results[0];
                    var player2Selection = results[1];

                    int player1Score = CalculateScore(player1Selection, attribute1, attribute2);
                    int player2Score = CalculateScore(player2Selection, attribute1, attribute2);

                    await NotifyRoundResultsAsync(player1Score, player2Score);
                    DetermineRoundWinner(ref player1Wins, ref player2Wins, player1Score, player2Score);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Игра завершена: один из игроков отключился.");
                    return;
                }
            }

            await AnnounceBattleWinnerAsync(player1Wins, player2Wins);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в ходе игры: {ex.Message}");
        }
        finally
        {
            player1Socket.Close();
            player2Socket.Close();
        }
    }

    // Метод для подсчёта очков на основе выбранных карт и атрибутов.
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

    private async Task NotifyPlayersAsync(BattleType battleType, string attribute1, string? attribute2)
    {
        var message = attribute2 == null ? $"{battleType} {attribute1}" : $"{battleType} {attribute1},{attribute2}";
        await BroadcastMessageAsync(Protocol.ROUND_START, message);
        await Task.Delay(50);
    }

    private async Task<List<Card>> GetPlayerSelectionAsync(List<Card> deck, BattleType battleType, Socket playerSocket,
        Socket opponentSocket)
    {
        List<Card> validCards = deck.FindAll(c => IsValidCardForBattle(c, battleType));
        if (validCards.Count == 0)
        {
            await SendToPlayerAsync(playerSocket, Protocol.NO_CARDS, "У вас нет подходящих карт.");
            return new List<Card>();
        }

        await SendToPlayerAsync(playerSocket, Protocol.SELECT_CARDS, ConvertCardsToString(validCards));

        await Task.Delay(50); // Даем клиенту время обработать SELECT_CARDS

        var (command, data) = await ReceiveFromPlayerAsync(playerSocket);
        if (command != Protocol.PLAYER_SELECTED || !int.TryParse(data, out int choice) || choice < 1 ||
            choice > validCards.Count)
        {
            await SendToPlayerAsync(playerSocket, Protocol.ERROR, "Некорректный выбор.");
            return await GetPlayerSelectionAsync(deck, battleType, playerSocket, opponentSocket);
        }

        List<Card> selected = new() { validCards[choice - 1] };

        if (battleType is BattleType.MaleTagTeam or BattleType.FemaleTagTeam)
        {
            await SendToPlayerAsync(playerSocket, Protocol.SELECT_CARDS, "Выберите вторую карту:");
            (command, data) = await ReceiveFromPlayerAsync(playerSocket);
            if (command == Protocol.PLAYER_SELECTED && int.TryParse(data, out choice) && choice >= 1 &&
                choice <= validCards.Count)
            {
                selected.Add(validCards[choice - 1]);
            }
        }

        await SendToPlayerAsync(playerSocket, Protocol.READY, "Ожидайте соперника...");
        await SendToPlayerAsync(opponentSocket, Protocol.READY, "Противник выбрал. Ваш ход!");

        return selected;
    }

    private static async Task SendToPlayerAsync(Socket playerSocket, string command, string message = "")
    {
        string fullMessage = $"{command} {message}".Trim() + "\n"; // Добавляем \n
        byte[] messageBytes = Encoding.UTF8.GetBytes(fullMessage);
        await playerSocket.SendAsync(messageBytes, SocketFlags.None);
    }

    private static async Task<(string command, string data)> ReceiveFromPlayerAsync(Socket playerSocket)
    {
        byte[] buffer = new byte[256];
        int bytesReceived = await playerSocket.ReceiveAsync(buffer, SocketFlags.None);
        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived).Trim();
        int spaceIndex = receivedMessage.IndexOf(' ');
        return spaceIndex == -1
            ? (receivedMessage, "")
            : (receivedMessage[..spaceIndex], receivedMessage[(spaceIndex + 1)..]);
    }

    private async Task NotifyRoundResultsAsync(int player1Score, int player2Score)
    {
        var winnerPlayer = player1Score > player2Score ? "Игрок №1" :
            player1Score < player2Score ? "Игрок №2" : "Ничья";


        var message = $"Игрок 1: {player1Score}, Игрок 2: {player2Score}\nПобедитель раунда: {winnerPlayer}";
        await Task.WhenAll(
            SendToPlayerAsync(player1Socket, Protocol.ROUND_RESULT, message),
            SendToPlayerAsync(player2Socket, Protocol.ROUND_RESULT, message)
        );
    }

    private static void DetermineRoundWinner(ref int player1Wins, ref int player2Wins, int player1Score,
        int player2Score)
    {
        if (player1Score > player2Score) player1Wins++;
        else if (player2Score > player1Score) player2Wins++;
    }

    private async Task AnnounceBattleWinnerAsync(int player1Wins, int player2Wins)
    {
        string result = player1Wins > player2Wins ? "Игрок 1 победил!" :
            player2Wins > player1Wins ? "Игрок 2 победил!" : "Ничья!";
        await BroadcastMessageAsync(Protocol.GAME_RESULT, result);
        Console.WriteLine($"Итог игры: {result}");
    }

    private string ConvertCardsToString(List<Card> deck)
    {
        return string.Join("|", deck.Select((card, index) =>
            $"{index + 1}. {card.Name} - " +
            $"Сила: {card.Strength}, " +
            $"Жесткость: {card.Toughness}, " +
            $"Выносливость: {card.Endurance}, " +
            $"Харизма: {card.Charisma}"
        ));
    }


    private static BattleType GetRandomBattleType()
    {
        Random rand = new Random();
        BattleType[] values =
        [
            BattleType.MaleSingle, BattleType.FemaleSingle, BattleType.FemaleTagTeam, BattleType.FemaleTagTeam
        ];
        return values[rand.Next(values.Length)];
    }

    private async Task InitializePlayersAsync()
    {
        await SendToPlayerAsync(player1Socket, Protocol.PLAYER_NUMBER, "Вы - Игрок №1");
        await SendToPlayerAsync(player2Socket, Protocol.PLAYER_NUMBER, "Вы - Игрок №2");

        await Task.Delay(50);
        
        await SendDeckToPlayerAsync(player1Socket, player1Deck);
        await SendDeckToPlayerAsync(player2Socket, player2Deck);
    }

    private async Task SendDeckToPlayerAsync(Socket playerSocket, List<Card> deck)
    {
        await SendToPlayerAsync(playerSocket, Protocol.CARDS, ConvertCardsToString(deck));
    }

    private static string GetRandomAttribute() =>
        new[] { "Сила", "Жесткость", "Выносливость", "Харизма" }[new Random().Next(4)];

    private async Task BroadcastMessageAsync(string command, string message)
    {
        await Task.WhenAll(
            SendToPlayerAsync(player1Socket, command, message),
            SendToPlayerAsync(player2Socket, command, message)
        );
    }

    private static bool IsValidCardForBattle(Card card, BattleType battleType)
    {
        return (battleType == BattleType.MaleSingle || battleType == BattleType.MaleTagTeam) && card.Gender == "Male" ||
               (battleType == BattleType.FemaleSingle || battleType == BattleType.FemaleTagTeam) &&
               card.Gender == "Female";
    }
}