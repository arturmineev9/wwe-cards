using System.Net;
using System.Net.Sockets;
using System.Text;
using WWECardsGame;

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
            string message = await ReceiveMessageAsync();
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

    // Если ввод не пустой, отправляем его на сервер.
    if (!string.IsNullOrWhiteSpace(input))
    {
        // Преобразуем строку в массив байтов.
        var messageBytes = Encoding.UTF8.GetBytes(input);

        // Асинхронная отправка данных на сервер.
        await clientSocket.SendAsync(messageBytes, SocketFlags.None);
    }
}

async Task HandleServerMessage(string message)
{
    string[] parts = message.Split(' ');

    switch (parts[0])
    {
        case "CARDS":
            Console.WriteLine("Ваши карты:");
            string[] cards = message.Substring(Protocol.CARDS.Length + 1)
                .Split('|'); // Пропускаем "CARDS " и делим по "|"
            foreach (string card in cards)
            {
                Console.WriteLine(card.Trim());
            }

            break;

        case "SELECT_CARDS":
            string[] validCards = message.Substring(Protocol.SELECT_CARDS.Length + 1).Split('|');
            foreach (string card in validCards)
            {
                Console.WriteLine(card.Trim());
            }

            Console.Write("Выберите карту (номер): ");
            string choice = Console.ReadLine();
            Console.WriteLine($"Вы выбрали карту: {choice}");
            await SendMessageAsync($"{Protocol.PLAYER_SELECTED} {choice}");
            break;

        case "ROUND_START":
            Console.WriteLine($"Тип боя: {parts[1]}, Атрибут: {parts[2]}");

            break;

        case "ROUND_RESULT":
            Console.WriteLine($"Результаты раунда: {string.Join(' ', parts[1..])}");
            break;

        case "GAME_RESULT":
            Console.WriteLine($"Игра окончена: {parts[1]}");
            Environment.Exit(0);
            break;

        case "ERROR":
            Console.WriteLine("Ошибка!");
            break;
        default:
            Console.WriteLine($"Неизвестное сообщение: {message}");
            break;
    }
}

async Task<string> ReceiveMessageAsync()
{
    byte[] buffer = new byte[1024];
    int bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
    return Encoding.UTF8.GetString(buffer, 0, bytesReceived).Trim();
}

async Task SendMessageAsync(string message)
{
    byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\n");
    await clientSocket.SendAsync(messageBytes, SocketFlags.None);
}