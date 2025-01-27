using System.Net;
using System.Net.Sockets;
using System.Text;

var clientSocket = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);

await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 5000));
Console.WriteLine("Подключено к серверу.");

// Запускаем обработку входящих сообщений от сервера
_ = Task.Run(async () =>
{
    while (true)
    {
        try
        {
            var buffer = new byte[256];
            int bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
            if (bytesReceived > 0)
            {
                string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                Console.WriteLine(serverMessage);
            }
        }
        catch
        {
            Console.WriteLine("Соединение с сервером потеряно.");
            break;
        }
    }
});

// Основной цикл ввода данных
while (true)
{
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        var messageBytes = Encoding.UTF8.GetBytes(input);
        await clientSocket.SendAsync(messageBytes, SocketFlags.None);
    }
}

clientSocket.Shutdown(SocketShutdown.Both);
clientSocket.Close();