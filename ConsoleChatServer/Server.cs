using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Server 
{
    private IPAddress   ipAddress = null;
    private int         port      = 0;
    private TcpListener listener  = null;

    private List<Client> connectedClients = new List<Client>();

    private bool KeepRunning = false;

    public async Task StartServer(IPAddress _ipAddress = null, int _port = 0) {
        if (_ipAddress == null) {
            _ipAddress = IPAddress.Any;
        }

        if (_port <= 0 || _port >= 65535) {
            _port = 23000;
        }

        ipAddress = _ipAddress;
        port = _port;

        if (listener == null) {
            listener = new TcpListener(ipAddress, port);
        }

        try
        {
            listener.Start();
            Console.WriteLine($"Server started ({ipAddress.ToString()}:{port}). Type <CLOSE> to close!");

            KeepRunning = true;

            while(KeepRunning) {

                TcpClient connectedTcpClient = await listener.AcceptTcpClientAsync();
                Client client = new Client(connectedTcpClient, connectedClients.Count.ToString());
                connectedClients.Add(client);

                Console.WriteLine("Client connected: " + client.tcpClient.Client.LocalEndPoint);

                HandleClient(client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private async Task HandleClient(Client client) {
        NetworkStream stream    = null;
        StreamReader  reader    = null;
        TcpClient     tcpClient = client.tcpClient;

        try
        {
            stream = tcpClient.GetStream();
            reader = new StreamReader(stream);

            char[] buffer = new char[64];

            while (KeepRunning) {
                int sentBytesCount = await reader.ReadAsync(buffer, 0, buffer.Length);

                Console.WriteLine("Client sent " + sentBytesCount);

                if (sentBytesCount <= 0) {
                    RemoveClient(client);
                    break;
                }

                string msg = new string(buffer);

                SendMessageForAllClients(msg, client);

                Array.Clear(buffer, 0, buffer.Length);
            }
        }
        catch (Exception e) {
            RemoveClient(client);
            Console.WriteLine(e.ToString());
        }
    }

    private void RemoveClient(Client client) {
        if (connectedClients.Contains(client)) {
            connectedClients.Remove(client);
            Console.WriteLine($"Client {client.tcpClient.Client.RemoteEndPoint} disconnected! Count: {connectedClients.Count}");
        }
    }

    private void ForAllClients(Action<Client> action, Client clientToIgnore) {
        foreach(Client client in connectedClients) {
            if (client != clientToIgnore)
                action(client);
        }
    }

    private async Task SendMessageForAllClients(string message, Client owner) {
        if (String.IsNullOrEmpty(message)) {
            return;
        }

        try
        {
            ForAllClients((_client) => {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);

                _client.tcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }, owner);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}

public class Client 
{
    public TcpClient tcpClient = null;
    public string name         = "";

    public Client(TcpClient _tcpClient, string _name) {
        tcpClient = _tcpClient;
        name      = _name;
    } 
}