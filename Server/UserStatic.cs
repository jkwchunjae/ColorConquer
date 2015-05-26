﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Extensions;
using Common;

namespace Server
{
	public static class UserStatic
	{
		public static ConcurrentDictionary<Socket, User> UserSocketDic = new ConcurrentDictionary<Socket, User>();

		public static User GetUser(this Socket socket)
		{
			if (!UserSocketDic.ContainsKey(socket))
			{
				UserSocketDic.TryAdd(socket, new User(socket));
			}
			return UserSocketDic[socket];
		}

		public static void SendAsync(this User user, PacketType packetType, string json)
		{
			user.Socket.SendAsync(packetType, json);
		}

		public static void SendRoomList(this User user, RoomList roomList)
		{
			user.SendAsync(PacketType.RoomList, roomList.ToJsonString());
		}
	}
}