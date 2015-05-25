using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Server
{
	public static class PacketProcessor
	{
		public static void ProcessPacket(this Socket socket, PacketType packetType, string json = "")
		{
			var user = socket.GetUser();
			switch (packetType)
			{
				case PacketType.EnterChannel:
					ColorConquerCenter.EnterUser(user);
					break;
				case PacketType.LeaveChannel:
					ColorConquerCenter.LeaveUser(user);
					break;
				#region CreateRoom
				case PacketType.CreateRoom:
					ColorConquerCenter.CreateRoom(user, json.Deserialize<Room>());
					break;
				#endregion
				case PacketType.EnterRoom:
					ColorConquerCenter.EnterRoom(user, json);
					break;
				case PacketType.LeaveRoom:
					break;
				case PacketType.StartGame:
					break;
				case PacketType.SetColor:
					break;
			}
		}
	}
}
