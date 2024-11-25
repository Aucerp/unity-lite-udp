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
        private Thread _receiveThread; // 用于存储线程对象

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
            StartMessageLoop();
        }

        private void StartMessageLoop()
        {
            _receiveThread = new Thread(() =>
            {
                while (_isRunning)
                {
                    try
                    {
                        string message = _receiver.ReceiveMessage();
                        if (!string.IsNullOrEmpty(message))
                        {
                            _receiver.ProcessIncomingData(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(string.Format("Receive thread error: {0}", ex.Message));
                    }

                    Thread.Sleep(10); // 防止高频占用 CPU
                }
            });

            _receiveThread.IsBackground = true; // 设置为后台线程
            _receiveThread.Start();
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
            _isRunning = false; // 停止接收线程
            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Join(); // 等待线程安全退出
            }

            if (_receiver != null)
            {
                _receiver.Close();
            }

            if (_sender != null)
            {
                _sender.Close();
            }
        }
    }
}
