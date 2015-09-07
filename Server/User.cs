using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR.Hubs;

namespace ColorConquerServer
{
	public class User
	{
		public Socket Socket { get; private set; }
		public HubCallerContext Context { get; private set; }
		public String ConnectionId { get; private set; }
		public string UserId;
		public string UserName;
		public string UserImage;

		public Color CurrentColor;

		public User() { }

		public User(Socket socket)
		{
			Socket = socket;
			Context = null;
		}

		public User(HubCallerContext context)
		{
			Context = context;
			ConnectionId = context.ConnectionId;
			Socket = null;
		}

		public void Reset(Color currentColor)
		{
			CurrentColor = currentColor;
		}
	}

	public class Ai : User
	{
	}
}
