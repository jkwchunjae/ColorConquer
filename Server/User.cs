using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Collections.Concurrent;

namespace Server
{
	public class User
	{
		Socket socket;
		public string UserName;

		public Color CurrentColor;

		public void Reset(Color currentColor)
		{
			CurrentColor = currentColor;
		}
	}

	public static class UserStatic
	{
		public static ConcurrentDictionary<Socket, User> UserSocketDic = new ConcurrentDictionary<Socket, User>();

		public static User GetUser(this Socket socket)
		{
			if (!UserSocketDic.ContainsKey(socket))
			{
				UserSocketDic.TryAdd(socket, new User());
			}
			return UserSocketDic[socket];
		}

		public static void ProcessPacket(User user, PacketType packetType, byte[] bytes)
		{
			switch (packetType)
			{
				case PacketType.EnterChannel:
					ColorConquerCenter.EnterUser(user);
					break;
				case PacketType.LeaveChannel:
					ColorConquerCenter.LeaveUser(user);
					break;
				case PacketType.CreateRoom:
					ColorConquerCenter.CreateRoom(user);
					break;
				case PacketType.EnterRoom:
					//ColorConquerCenter.EnterRoom(user
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
