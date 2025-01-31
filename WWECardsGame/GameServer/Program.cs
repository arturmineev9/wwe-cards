using System.Net;
using System.Net.Sockets;
using WWECardsGame;

var socketServer = new Socket(
    AddressFamily.InterNetwork, 
    SocketType.Stream,         
    ProtocolType.Tcp);          

socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 5000));

socketServer.Listen();

Console.WriteLine("Сервер запущен, ожидание подключения...");


while (true)
{
    var player1Socket = await socketServer.AcceptAsync();
    Console.WriteLine("Игрок 1 подключился.");

    var player2Socket = await socketServer.AcceptAsync();
    Console.WriteLine("Игрок 2 подключился.");

    _ = Task.Run(async () =>
    {
        try
        {
            var (player1Deck, player2Deck) = CardRepository.GetRandomDecks();
            var battleManager = new BattleManager(player1Deck, player2Deck, player1Socket, player2Socket);
            await battleManager.StartBattleAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в сессии: {ex.Message}");
        }
        finally
        {
            player1Socket.Shutdown(SocketShutdown.Both); 
            player1Socket.Close();                       


            player2Socket.Shutdown(SocketShutdown.Both); 
            player2Socket.Close(); 
        }
    });
}


