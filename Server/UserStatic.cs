using System;
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
			if (user.Socket != null)
			{
				user.Socket.SendAsync(packetType, json);
			}
			else if (user.Context != null)
			{
				json = @"{ ""packetType"":""{packetType}"", ""data"":{json} }".WithVar(new { packetType, json });
				json.Dump();
				user.Context.Send(json);
			}
		}
	}
}
