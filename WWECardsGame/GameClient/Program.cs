using System.Net;
using System.Net.Sockets;
using System.Text;

// Создание сокета для клиента с использованием IPv4, потокового типа и протокола TCP.
var clientSocket = new Socket(
    AddressFamily.InterNetwork, // Используем IPv4.
    SocketType.Stream,          // Потоковый тип сокета (для TCP).
    ProtocolType.Tcp);          // Используем протокол TCP.

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
            // Буфер для хранения входящих данных.
            var buffer = new byte[256];

            // Асинхронное получение данных от сервера.
            int bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);

            // Если данные получены, преобразуем их в строку и выводим в консоль.
            if (bytesReceived > 0)
            {
                string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                Console.WriteLine(serverMessage);
            }
        }
        catch
        {
            // В случае ошибки (например, разрыв соединения) выводим сообщение и завершаем цикл.
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
