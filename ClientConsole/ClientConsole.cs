using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Threading;

namespace ClientConsole
{
	public class ClientConsole
	{
		Socket socket;

		public ClientConsole()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		}

		public void StartClient(string host, int port)
		{
			socket.Connect(host, port);

			socket.ReceiveLoop(PacketProcessor.ProcessPacket);

			EnterChannel();

			//(new Thread(() =>
			//{
			//	while (true)
			//	{
			//		Console.Write("Command: ");
			//		var command = Console.ReadLine();

			//		PacketType packetType;
			//		if (Enum.TryParse<PacketType>(command, out packetType))
			//		{
			//			DoCommand(packetType);
			//		}
			//	}
			//})).Start();
			
			while (true)
			{
				Console.Write("Command: ");
				var command = Console.ReadLine();

				PacketType packetType;
				if (Enum.TryParse<PacketType>(command, out packetType))
				{
					DoCommand(packetType);
				}
			}
		}

		public void EnterChannel()
		{
			socket.SendAsync(PacketType.EnterChannel);
		}

		public void DoCommand(PacketType packetType)
		{
			switch (packetType)
			{
				#region CreateRoom
				case PacketType.CreateRoom:
					{
						Console.Write("RoomName: ");
						var roomName = Console.ReadLine();
						socket.SendAsync(packetType, (new Room(roomName)).ToJsonString());
						break;
					}
				#endregion

				#region EnterRoom
				case PacketType.EnterRoom:
					{
						Console.Write("RoomName: ");
						var roomName = Console.ReadLine();
						socket.SendAsync(packetType, roomName);
						break;
					}
				#endregion
			}
		}
	}
}
