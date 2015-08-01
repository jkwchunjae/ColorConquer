using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Extensions;

namespace ColorConquerServer
{
	public static class UserStatic
	{
		public static ColorConquerHub colorConquerHub = null;

		public static void SendAsync(this User user, PacketType packetType, string json)
		{
			if (colorConquerHub == null)
				return;

			json = @"{ ""packetType"":""{packetType}"", ""data"":{json} }".WithVar(new { packetType, json });
			json.Dump();

			colorConquerHub.Send(user.ConnectionId, json);
		}
	}
}
