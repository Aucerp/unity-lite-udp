using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
/*
public class Receiver
{
    private readonly Socket _socketReceiver;
    private readonly ILog _log;
    private readonly int _port;
    public int Port { get { return _port; } }

    public event Action<NetworkMessage> OnMessageReceived;

    public Receiver(int port)
    {
        _port = port;
        _log = new UnityLog();
        _socketReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {
            _socketReceiver.Bind(new IPEndPoint(IPAddress.Any, port));
            _log.Log("Receiver", string.Format("Socket bound to port: {0}", port));
        }
        catch (Exception ex)
        {
            _log.LogError("Receiver", string.Format("Failed to bind to port {0}", port), ex);
            throw;
        }
    }

    public void ProcessIncomingData(string rawMessage)
    {
        if (string.IsNullOrEmpty(rawMessage))
        {
            _log.LogError("Receiver", "Received empty message", null);
            return;
        }

        try
        {
            NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(rawMessage);
            if (OnMessageReceived != null)
            {
                OnMessageReceived.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            _log.LogError("Receiver", "Error processing incoming message", ex);
        }
    }

    public string ReceiveMessage()
    {
        byte[] buffer = new byte[512];
        try
        {
            if (_socketReceiver.Poll(1000, SelectMode.SelectRead))
            {
                int receivedBytes = _socketReceiver.Receive(buffer);
                if (receivedBytes > 0)
                {
                    string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    _log.Log("Receiver", string.Format("Received message: {0}", receivedMessage));
                    return receivedMessage;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _log.LogError("Receiver", "Error receiving message", ex);
            return null;
        }
    }

    public void Close()
    {
        try
        {
            _socketReceiver.Close();
            _log.Log("Receiver", string.Format("Socket on port {0} closed", _port));
        }
        catch (Exception ex)
        {
            _log.LogError("Receiver", "Error closing socket", ex);
        }
    }
}*/