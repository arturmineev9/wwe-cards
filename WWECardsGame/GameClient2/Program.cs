using System.Net;
using System.Net.Sockets;
using System.Text;

var clientSocket = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);

await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 5000));
Console.WriteLine("Подключено к серверу.");

while (true)
{
    // Получение сообщения от сервера
    var buffer = new byte[256];
    int bytesReceived = clientSocket.Receive(buffer);
    string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
    Console.WriteLine(serverMessage);

    // Если сервер запрашивает выбор карты
    if (serverMessage.Contains("выберите карту"))
    {
        Console.Write("Ваш выбор: ");
        string choice = Console.ReadLine();
        clientSocket.Send(Encoding.UTF8.GetBytes(choice));
    }

    // Если игра завершена
    if (serverMessage.Contains("Победитель") || serverMessage.Contains("Ничья"))
    {
        break;
    }
}

clientSocket.Shutdown(SocketShutdown.Both);
clientSocket.Close();