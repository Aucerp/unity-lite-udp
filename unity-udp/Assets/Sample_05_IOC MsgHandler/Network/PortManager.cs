using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public static class PortManager
{
    private static readonly HashSet<int> _usedPorts = new HashSet<int>();
    private static readonly object _lock = new object();
    private const int MIN_PORT = 23001;
    private const int MAX_PORT = 23999;

    public static int AcquirePort()
    {
        lock (_lock)
        {
            // 先嘗試找到第一個可用的端口
            for (int port = MIN_PORT; port <= MAX_PORT; port++)
            {
                if (!_usedPorts.Contains(port) && !IsPortInUse(port))
                {
                    _usedPorts.Add(port);
                    return port;
                }
            }
            throw new Exception("No available ports in range " + MIN_PORT + " - " + MAX_PORT);
        }
    }

    public static void ReleasePort(int port)
    {
        lock (_lock)
        {
            _usedPorts.Remove(port);
        }
    }

    private static bool IsPortInUse(int port)
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                return false;
            }
        }
        catch
        {
            return true;
        }
    }
}