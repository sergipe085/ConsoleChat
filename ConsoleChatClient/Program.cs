using System;

namespace ConsoleChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();

            Console.WriteLine("--- Welcome to Whatsapp 2! ---");
            Console.WriteLine("Provide the server IP...");
            string ip = Console.ReadLine();
            if (!client.SetServerIP(ip)) {
                Console.WriteLine("Invalid IP! Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Provide the server PORT...");
            string port = Console.ReadLine();
            if (!client.SetServerPort(port)) {
                Console.WriteLine("Invalid PORT! Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Provide your name...");
            string name = Console.ReadLine();

            client.ConnectToServer();

            string userInput = "";
            do
            {
                userInput = Console.ReadLine();

                if (userInput.Trim() != "<EXIT>")
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine(userInput + " << You");
                    client.SendDataToServerAsync($"{name} >> {userInput}");
                }
                else if (userInput.Equals("<EXIT>"))
                {
                    client.CloseAndDisconnect();
                }
            }
            while(userInput != "<EXIT>");
        }
    }
}
