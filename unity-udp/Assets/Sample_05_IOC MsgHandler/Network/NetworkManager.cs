using System;
using System.Threading;
using UnityEngine;

namespace Sample_05
{
    public abstract class NetworkManager : IDisposable
    {
        protected readonly Receiver _receiver;
        protected readonly Sender _sender;
        protected readonly IMessageHandler _msgHandler;
        protected bool _isRunning;
        protected readonly CancellationTokenSource _cancellationTokenSource;

        protected NetworkManager(int listenPort, string targetAddress, int targetPort, IMessageHandler msgHandler)
        {
            if (!NetworkUtils.IsValidIPv4(targetAddress))
            {
                throw new System.ArgumentException("Invalid IPv4 address format: " + targetAddress);
            }

            _receiver = new Receiver(listenPort);
            _sender = new Sender(targetAddress, targetPort);
            _msgHandler = msgHandler;
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            StartMessageLoop();
        }

        private void StartMessageLoop()
        {
            Thread receiveThread = new Thread(() =>
            {
                while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string message = _receiver.ReceiveMessage();
                    if (!string.IsNullOrEmpty(message))
                    {
                        _receiver.ProcessIncomingData(message);
                    }
                    Thread.Sleep(10);
                }
            });
            receiveThread.Start();
        }

        public void SendMessage(string messageType, string data)
        {
            NetworkMessage message = new NetworkMessage
            {
                Type = messageType,
                Data = data
            };
            string jsonMessage = JsonUtility.ToJson(message);
            _sender.SendMessage(jsonMessage);
        }

        public virtual void Dispose()
        {
            _isRunning = false;
            _cancellationTokenSource.Cancel();
            if (_receiver != null)
            {
                _receiver.Close();
            }
            if (_sender != null)
            {
                _sender.Close();
            }
            _cancellationTokenSource.Dispose();
        }
    }
}