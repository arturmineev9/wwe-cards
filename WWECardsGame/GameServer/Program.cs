using System.Net;
using System.Net.Sockets;
using System.Text;

var socketServer = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);

socketServer.Bind(
    new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 5000));
socketServer.Listen();

Console.WriteLine("Сервер запущен, ожидание подключения...");

var connectionSocket = await socketServer.AcceptAsync();
Console.WriteLine("Клиент подключился.");

var buffer = new byte[256];
var result = 0;
var resultMessage = new StringBuilder();

do
{
    result = await connectionSocket.ReceiveAsync(buffer, SocketFlags.None);
    resultMessage.Append(Encoding.UTF8.GetString(buffer, 0, result));
} while (result > 0 && result == buffer.Length);

Console.WriteLine($"Получено от клиента: {resultMessage}");

var responseMessage = "Привет, клиент!";
var bytesSent = await connectionSocket.SendAsync(
    new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseMessage)),
    SocketFlags.None);

Console.WriteLine($"Отправлено {bytesSent} байт");

connectionSocket.Shutdown(SocketShutdown.Both);
connectionSocket.Close();

socketServer.Dispose();