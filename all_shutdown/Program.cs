using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace all_shutdown
{
    class Program
    {
        static IPAddress remoteAddress; // хост для отправки данных
        const int remotePort = 6801; // порт для отправки данных
        const int localPort = 6801; // локальный порт для прослушивания входящих подключений
        static string username;
        static void Main(string[] args)
        {
            try
            {
                //Console.Write("Введите свое имя:");
                //username = Console.ReadLine();
                remoteAddress = IPAddress.Parse("224.0.0.2");
                /* Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                 receiveThread.Start();
                 SendMessage(); // отправляем сообщение*/
                if (args.Length != 0)
                    SendMessage();
                else
                    ReceiveMessage();  
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void SendMessage()
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки
            IPEndPoint endPoint = new IPEndPoint(remoteAddress, remotePort);
            try
            {
                for(int i = 0; i < 10; i++)
                {
                    //string message = Console.ReadLine(); // сообщение для отправки
                    //message = String.Format("{0}: {1}", username, message);
                    string message = "shutdown";
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    sender.Send(data, data.Length, endPoint); // отправка
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }



        private static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(localPort); // UdpClient для получения данных
            receiver.EnableBroadcast = true;
            receiver.JoinMulticastGroup(remoteAddress, 20);
            IPEndPoint remoteIp = null;
            string localAddress = LocalIPAddress();
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    if (remoteIp.Address.ToString().Equals(localAddress))
                        continue;
                    string message = Encoding.Unicode.GetString(data);
                    Console.WriteLine(message);

                    if (message.Equals("shutdown") )
                    {
                        Process.Start("shutdown", "/s /t 0");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private static string LocalIPAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
