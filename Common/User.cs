using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Common;

namespace Common
{
	public class User
	{
		public Socket Socket;
		public string UserName;

		public Color CurrentColor;

		public User(Socket socket)
		{
			Socket = socket;
		}

		public void Reset(Color currentColor)
		{
			CurrentColor = currentColor;
		}
	}
}
