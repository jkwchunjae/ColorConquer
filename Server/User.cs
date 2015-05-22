using ColorConquer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class User
	{
		Socket socket;
		public string UserName;

		public Color CurrentColor;

		public void Reset(Color currentColor)
		{
			CurrentColor = currentColor;
		}
	}
}
