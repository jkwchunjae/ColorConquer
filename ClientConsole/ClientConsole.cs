using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Common;
using Extensions;

namespace ClientConsole
{
	public class ClientConsole
	{
		string _host;
		int _port;
		Socket socket;

		public ClientConsole()
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		}

		public void StartClient(string host, int port)
		{
			_host = host;
			_port = port;
			socket.Connect(host, port);

			socket.ReceiveLoop(PacketProcessor.ProcessPacket);

			if (!User.TryLogin()) return;

			DoCommand(PacketType.TryEnterChannel);

			while (true)
			{
				var command = Utils.Input("Command");

				PacketType packetType;
				if (Enum.TryParse<PacketType>(command, out packetType))
				{
					DoCommand(packetType);
				}
			}
		}

		public void DoCommand(PacketType packetType)
		{
			switch (packetType)
			{
				#region TryEnterChannel
				case PacketType.TryEnterChannel:
					{
						socket.TryEnterChannel(User.UserName);
						break;
					}
				#endregion

				#region TryCreateRoom
				case PacketType.TryCreateRoom:
					{
						var roomName = Utils.Input("RoomName");
						socket.TryCreateRoom(roomName);
						break;
					}
				#endregion

				#region TryEnterRoom
				case PacketType.TryEnterRoom:
					{
						var roomName = Utils.Input("RoomName");
						socket.TryEnterRoom(roomName);
						break;
					}
				#endregion

				#region ChatRoom
				case PacketType.ChatRoom:
					{
						var message = Utils.Input("Message");
						socket.ChatRoom(message);
						break;
					}
				#endregion

				#region TryStartGame
				case PacketType.TryStartGame:
					{
						var size = Utils.Input("Size").ToInt();
						var countColor = Utils.Input("CountColor").ToInt();
						socket.TryStartGame(size, countColor);
						break;
					}
				#endregion
			}
		}
	}
}
