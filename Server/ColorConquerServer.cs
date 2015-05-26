using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Net;
using Common;

namespace Server
{
	public class ColorConquerServer
	{
		public void Start(int port)
		{
			var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				listener.Bind(new IPEndPoint(IPAddress.Any, port));
				listener.Listen(100);

				Logger.Log("ColorConquerServer Started (port: {0})".With(port));

				while (true)
				{
					"Before Accept".Dump();
					var clientSocket = listener.Accept();
					clientSocket.ReceiveLoop(PacketProcessor.ProcessPacket);
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
			finally
			{
				listener.Close();
			}
		}
	}
}
