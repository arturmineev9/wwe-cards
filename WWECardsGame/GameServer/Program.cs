using System.Net;
using System.Net.Sockets;
using WWECardsGame;
using WWECardsGame.Entities;

var socketServer = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);

socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 5000));
socketServer.Listen();

Console.WriteLine("Сервер запущен, ожидание подключения...");

var player1Socket = await socketServer.AcceptAsync();
Console.WriteLine("Игрок 1 подключился.");

var player2Socket = await socketServer.AcceptAsync();
Console.WriteLine("Игрок 2 подключился.");

// Инициализация колод
var player1Deck = new List<Card>
{
    new Card("John Cena", "Male", 100, 100, 100, 100),
    new Card("The Rock", "Male", 95, 90, 85, 100),
    new Card("Rey Mysterio", "Male", 88, 87, 92, 85),
    new Card("Roman Reigns", "Male", 92, 89, 85, 90),
    new Card("Charlotte Flair", "Female", 85, 80, 88, 92),
    new Card("Sasha Banks", "Female", 80, 78, 82, 90)
};

var player2Deck = new List<Card>
{
    new Card("Kurt Angle", "Male", 88, 90, 92, 85),
    new Card("Triple H", "Male", 90, 93, 85, 88),
    new Card("Brock Lesnar", "Male", 98, 95, 90, 75),
    new Card("AJ Styles", "Male", 85, 82, 88, 91),
    new Card("Becky Lynch", "Female", 88, 85, 80, 90),
    new Card("Ronda Rousey", "Female", 92, 89, 87, 80)
};

// Запуск игры
var battleManager = new BattleManager(player1Deck, player2Deck, player1Socket, player2Socket);
await battleManager.StartBattleAsync(); 

player1Socket.Shutdown(SocketShutdown.Both);
player1Socket.Close();

player2Socket.Shutdown(SocketShutdown.Both);
player2Socket.Close();

socketServer.Dispose();