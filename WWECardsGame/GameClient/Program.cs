using System.Net;
using System.Net.Sockets;
using System.Text;
using GameServer;
using WWECardsGame;

class Program
{
    private static bool _isWaitingForCardSelection = false;

    static async Task Main(string[] args)
    {
        // Создание сокета для клиента с использованием IPv4, потокового типа и протокола TCP.
        var clientSocket = new Socket(
            AddressFamily.InterNetwork, // Используем IPv4.
            SocketType.Stream, // Потоковый тип сокета (для TCP).
            ProtocolType.Tcp); // Используем протокол TCP.

        // Подключение клиента к серверу по адресу 127.0.0.1 (локальный хост) и порту 5000.
        await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 5000));
        Console.WriteLine("Подключено к серверу.");

        // Запуск асинхронной задачи для обработки входящих сообщений от сервера.
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    string message = await ReceiveMessageAsync(clientSocket); // Передаем сокет в метод
                    await HandleServerMessage(message);
                }
                catch
                {
                    Console.WriteLine("Соединение с сервером потеряно.");
                    break;
                }
            }
        });

        // Основной цикл для ввода данных от пользователя и отправки их на сервер.
        while (true)
        {
            // Чтение ввода пользователя из консоли.
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (_isWaitingForCardSelection)
                {
                    // Если ожидается выбор карты, отправляем выбор на сервер
                    await SendMessageAsync(clientSocket,
                        $"{Protocol.PLAYER_SELECTED} {input}"); // Передаем сокет в метод
                    _isWaitingForCardSelection = false; // Сбрасываем флаг
                }
                else
                {
                    // Иначе отправляем ввод как есть
                    await SendMessageAsync(clientSocket, input); // Передаем сокет в метод
                }
            }
        }
    }

    static Task HandleServerMessage(string message)
    {
        string[] parts = message.Split(' ');

        switch (parts[0])
        {
            case "PLAYER_NUMBER":
                Console.WriteLine(message.Substring(Protocol.PLAYER_NUMBER.Length + 1) + "\n");
                break;
            case "CARDS":
                var cards = message.Substring(Protocol.CARDS.Length + 1)
                    .Split('|'); // Пропускаем "CARDS " и делим по "|"
                foreach (string card in cards)
                {
                    Console.WriteLine(card.Trim());
                }
                Console.WriteLine();
                break;

            case "SELECT_CARDS":
                var validCards = message.Substring(Protocol.SELECT_CARDS.Length + 1).Split('|');
                
                foreach (string card in validCards)
                {
                    Console.WriteLine(card.Trim());
                }

                Console.Write("\nВыберите карту (номер): ");
                _isWaitingForCardSelection = true; // Устанавливаем флаг ожидания выбора
                break;

            case "ROUND_START":
                Console.WriteLine($"Тип боя: {parts[1]}, Атрибут: {parts[2]}");
                break;

            case "ROUND_RESULT":
                Console.WriteLine($"\nРезультаты раунда: {string.Join(' ', parts[1..])}\n");
                break;

            case "GAME_RESULT":
                Console.WriteLine($"Итоги игры: {message.Substring(Protocol.GAME_RESULT.Length + 1)}");
                Console.WriteLine("Игра окончена.");
                Environment.Exit(0);
                break;

            case "READY":
                Console.WriteLine(_isWaitingForCardSelection
                    ? "Противник выбрал карту. Ваш ход!"
                    : "Ожидайте соперника...");

                break;
            case "ERROR":
                Console.WriteLine("Ошибка!");
                break;

            default:
                Console.WriteLine($"Неизвестное сообщение: {message}");
                break;
        }

        return Task.CompletedTask;
    }

    static async Task<string> ReceiveMessageAsync(Socket clientSocket)
    {
        var buffer = new byte[1024];
        var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
        //Console.WriteLine($"Получено сообщение от сервера: {Encoding.UTF8.GetString(buffer, 0, bytesReceived).Trim()}");
        return Encoding.UTF8.GetString(buffer, 0, bytesReceived).Trim();
    }

    static async Task SendMessageAsync(Socket clientSocket, string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message + "\n");
        await clientSocket.SendAsync(messageBytes, SocketFlags.None);
    }
}