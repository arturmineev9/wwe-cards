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

// Инициализация колоды карт для игроков.
var (player1Deck, player2Deck) = CardRepository.GetRandomDecks();

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