﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Common;
using Server;
using ClientConsole;

namespace ColorConquer
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				int port = 55591;
				Console.Write("Mode (Server(s), ClientConsole(cc)) : ");
				var mode = Console.ReadLine().ToLower();
				if (mode == "server" || mode == "s")
				{
					//(new ColorConquerServer()).Start(port);
					var server = new ColorConquerServer();
					server.StartWebSocket(port);
					Console.ReadLine();
					server.StopWebSocket();
					return;
				}
				else if (mode == "clientConsole".ToLower() || mode == "cc")
				{
					(new ClientConsole.ClientConsole()).StartClient("localhost", port);
				}
			}
		}
	}
}
