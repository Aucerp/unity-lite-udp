using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
namespace Sample_01
{
	//Server
	public class UDPBroadcastReceiver
	{
		void Start()
		{
			Socket socketBroadcastReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint ipepLocal = new IPEndPoint(IPAddress.Any, 23000);
			byte[] buffer = new byte[512];
			int nCountReceive = 0;

			try
			{
				socketBroadcastReceiver.Bind(ipepLocal);
				while (true)
				{
					nCountReceive = socketBroadcastReceiver.Receive(buffer);
					Debug.Log("Number of Receive :" + nCountReceive);
					Array.Clear(buffer, 0, buffer.Length);
				}
			}
			catch (Exception excp)
			{
				Debug.Log(excp.ToString());
			}
		}
	}
}
