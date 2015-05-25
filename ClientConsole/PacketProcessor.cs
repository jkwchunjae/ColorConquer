using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Common;

namespace ClientConsole
{
	public static class PacketProcessor
	{
		public static void ProcessPacket(this Socket socket, PacketType packetType, string json = "")
		{
			switch (packetType)
			{
				#region RoomList
				case PacketType.RoomList:
					var roomList = json.DeseializeRoomList();
					"RoomList Count: {0}".With(roomList.Count).Dump();
					foreach (var room in roomList)
					{
						"Room: {0}".With(room.RoomName).Dump();
					}
					break;
				#endregion
			}
		}
	}
}
