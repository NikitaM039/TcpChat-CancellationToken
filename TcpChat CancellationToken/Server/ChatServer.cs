﻿using System.Net.Sockets;
using System.Net;

namespace Client
{
    internal class ChatServer
    {
        private TcpClient client;
        private IPEndPoint endPoint;

        CancellationTokenSource tokenSource;
        CancellationToken token;

        public ChatServer(IPEndPoint endPoint)
        {
            this.client = new TcpClient();
            this.endPoint = endPoint;
            this.tokenSource = new CancellationTokenSource();
            this.token = tokenSource.Token;
        }

        /// <summary>
        /// Запуск клиента
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            await Console.Out.WriteLineAsync($"Клиент {GetHashCode()}\n" + new string('-', 15));
            try
            {
                await client.ConnectAsync(endPoint); // Асинхронное подключение
                Console.WriteLine("Соединение");

                var stream = client.GetStream();
                var writer = new StreamWriter(stream);
                var reader = new StreamReader(stream);

                var receiveTask = ReceiveMessages(reader); // Задача для получения сообщений

                // Отправка сообщений в цикле
                //!token.IsCancellationRequested
                while (!tokenSource.IsCancellationRequested)
                {
                    string? message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message)) continue; // Если ввод пустой, делаем вид, что ничего не произошло

                    if (message.ToLower() == "exit")
                    {
                        tokenSource.Cancel();
                        continue;
                        //token.ThrowIfCancellationRequested();
                    }

                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("До новых встреч!");
            }
            finally
            {
                await Console.Out.WriteLineAsync("Good-bye!");
                client.Close();
            }
        }

        /// <summary>
        /// Прослушивание сообщений от сервера
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private async Task ReceiveMessages(StreamReader reader)
        {
            try
            {
                string? message;
                while (!string.IsNullOrEmpty(message = await reader.ReadLineAsync())) // если сообщение не пустое
                {
                    Console.WriteLine(message);
                }
            }
            catch (Exception)
            {
                //Console.WriteLine($"Ошибка при получении сообщений: {ex.Message}");
            }
        }
    }
}