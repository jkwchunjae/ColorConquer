using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ColorConquer;
using Extensions;
using System.Net;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{

		}
	}

	class ColorConquerServer
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
					clientSocket.ReceiveLoop();
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
