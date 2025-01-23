using System.Net;
using System.Net.Sockets;
using System.Text;

var clientSocket = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);

await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 5000));
Console.WriteLine("Подключено к серверу.");

// Отправка сообщения серверу
string message = "Привет, сервер!";
var messageBytes = Encoding.UTF8.GetBytes(message);
await clientSocket.SendAsync(messageBytes, SocketFlags.None);
Console.WriteLine("Сообщение отправлено серверу.");

// Получение ответа от сервера
var buffer = new byte[256];
var result = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
string serverResponse = Encoding.UTF8.GetString(buffer, 0, result);
Console.WriteLine($"Ответ от сервера: {serverResponse}");

clientSocket.Shutdown(SocketShutdown.Both);
clientSocket.Close();