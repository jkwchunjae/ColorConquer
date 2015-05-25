using System;
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
				Console.Write("Mode (Server(s), ClientConsole(cc)) : ");
				var mode = Console.ReadLine().ToLower();
				if (mode == "server" || mode == "s")
				{
					(new ColorConquerServer()).Start(5519);
				}
				else if (mode == "clientConsole".ToLower() || mode == "cc")
				{
					(new ClientConsole.ClientConsole()).StartClient("localhost", 5519);
				}
			}
		}
	}
}
