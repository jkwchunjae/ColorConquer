using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Common;
using Alchemy.Classes;

namespace Server
{
	public class User
	{
		public Socket Socket { get; private set; }
		public UserContext Context { get; private set; }
		public string UserName;
		public string UserImage;

		public Color CurrentColor;

		public User(User user)
		{
			Socket = user.Socket;
			Context = user.Context;
			UserName = user.UserName;
			UserImage = user.UserImage;
		}

		public User(Socket socket)
		{
			Socket = socket;
			Context = null;
		}

		public User(UserContext context)
		{
			Context = context;
			Socket = null;
		}

		public void Reset(Color currentColor)
		{
			CurrentColor = currentColor;
		}
	}
}
