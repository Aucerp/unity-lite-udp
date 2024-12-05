using System;
using System.Net;
using System.Text;
using UnityEngine;
namespace s2
{
    public class EchoMsgHandler : IMsgHandler
    {
        public void Handle(string channel, byte[] message, string senderRole, EndPoint senderEndpoint)
        {
            string content = Encoding.UTF8.GetString(message);
            Debug.Log(LoggerUtils.FormatLog("Server", senderEndpoint.ToString(), "收到消息", content, senderRole, senderEndpoint.ToString()));
        }
    }
}
