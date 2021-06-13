using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Client
{
    TcpClient tcpClient  = null;
    IPAddress serverIp   = null;
    int       serverPort = 0;

    public bool SetServerIP(string ipStr) {
        IPAddress ipaddr = null;

        if (!IPAddress.TryParse(ipStr, out ipaddr)){
            Console.WriteLine("Invalid IP!");
            return false;
        }

        serverIp = ipaddr;
        return true;
    }

    public bool SetServerPort(string portStr) {
        int port = 0;

        if (!int.TryParse(portStr, out port)) {
            Console.WriteLine("Invalid port!");
            return false;
        }

        if (port <= 0 || port >= 65535) {
            Console.WriteLine("Please provide a port between 0 and 65535!");
            return false;
        }

        serverPort = port;
        return true;
    }

    public async Task ConnectToServer() {
        if (tcpClient == null) {
            tcpClient = new TcpClient();
        }

        try
        {
            await tcpClient.ConnectAsync(serverIp, serverPort);
            Console.WriteLine($"Connected to server {serverIp.ToString()}:{serverPort}. Type <EXIT> to exit.");

            ReadDataAsync();
        }
        catch(Exception e) 
        {
            Console.WriteLine(e.ToString());
        }
    }

    private async Task ReadDataAsync() {
        try
        {
            StreamReader reader = new StreamReader(tcpClient.GetStream());
            char[] buffer = new char[64];

            while(true) {
                int receivedByteCount = await reader.ReadAsync(buffer, 0, buffer.Length);

                if (receivedByteCount <= 0) {
                    Console.WriteLine("Disconnected from server!");
                    tcpClient.Close();
                    break;
                }

                Console.WriteLine(new string(buffer));
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public async Task SendDataToServerAsync(string userInput) {
        if (String.IsNullOrEmpty(userInput))
        {
            Console.WriteLine("Invalid Input");
            return;
        }

        if (tcpClient != null && tcpClient.Connected)
        {
            StreamWriter streamWriter = new StreamWriter(tcpClient.GetStream());
            streamWriter.AutoFlush = true;

            await streamWriter.WriteAsync(userInput);
        }
    }

    public void CloseAndDisconnect() {
        if (tcpClient != null && tcpClient.Connected) {
            tcpClient.Close();
        }
    }
}