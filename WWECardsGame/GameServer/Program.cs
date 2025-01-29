using System.Net;
using System.Net.Sockets;
using WWECardsGame;
using WWECardsGame.Entities;

// Создание сокета для сервера с использованием IPv4, потокового типа и протокола TCP.
var socketServer = new Socket(
    AddressFamily.InterNetwork, // Используем IPv4.
    SocketType.Stream,          // Потоковый тип сокета (для TCP).
    ProtocolType.Tcp);          // Используем протокол TCP.

// Привязка сокета к локальному IP-адресу (127.0.0.1) и порту 5000.
socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 5000));

// Начало прослушивания входящих подключений.
socketServer.Listen();

// Вывод сообщения о том, что сервер запущен и ожидает подключения игроков.
Console.WriteLine("Сервер запущен, ожидание подключения...");

// Ожидание подключения первого игрока.
var player1Socket = await socketServer.AcceptAsync();
Console.WriteLine("Игрок 1 подключился.");

// Ожидание подключения второго игрока.
var player2Socket = await socketServer.AcceptAsync();
Console.WriteLine("Игрок 2 подключился.");

// Инициализация колоды карт для первого игрока.
var player1Deck = new List<Card>
{
    new Card("John Cena", "Male", 100, 100, 100, 100),
    new Card("The Rock", "Male", 95, 90, 85, 100),
    new Card("Rey Mysterio", "Male", 88, 87, 92, 85),
    new Card("Roman Reigns", "Male", 92, 89, 85, 90),
    new Card("Charlotte Flair", "Female", 85, 80, 88, 92),
    new Card("Sasha Banks", "Female", 80, 78, 82, 90)
};

// Инициализация колоды карт для второго игрока.
var player2Deck = new List<Card>
{
    new Card("Kurt Angle", "Male", 88, 90, 92, 85),
    new Card("Triple H", "Male", 90, 93, 85, 88),
    new Card("Brock Lesnar", "Male", 98, 95, 90, 75),
    new Card("AJ Styles", "Male", 85, 82, 88, 91),
    new Card("Becky Lynch", "Female", 88, 85, 80, 90),
    new Card("Ronda Rousey", "Female", 92, 89, 87, 80)
};

// Создание экземпляра BattleManager для управления боем между двумя игроками.
var battleManager = new BattleManager(player1Deck, player2Deck, player1Socket, player2Socket);

// Запуск боя. Метод StartBattleAsync управляет всеми раундами и логикой игры.
await battleManager.StartBattleAsync();

// Завершение соединения с первым игроком.
player1Socket.Shutdown(SocketShutdown.Both); // Остановка отправки и получения данных.
player1Socket.Close();                       // Закрытие сокета.

// Завершение соединения со вторым игроком.
player2Socket.Shutdown(SocketShutdown.Both); // Остановка отправки и получения данных.
player2Socket.Close();                       // Закрытие сокета.

// Освобождение ресурсов серверного сокета.
socketServer.Dispose();