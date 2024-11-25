using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Sample_01
{
	//Client
	public class UDPBroadcastSender
	{
		void Start()
		{
			Socket socketBroadcaster = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socketBroadcaster.EnableBroadcast = true;

			IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Parse("255,255,255,255"), 23000);
			byte[] buffer = new byte[] { 0x0D, 0x0A };

			try
			{
				socketBroadcaster.SendTo(buffer, broadcastEP);
				socketBroadcaster.Shutdown(SocketShutdown.Both);
				socketBroadcaster.Close();
			}
			catch (Exception excp)
			{
				Debug.Log(excp.ToString());
			}
		}
	}
}
