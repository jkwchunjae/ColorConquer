using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Server;
using Common;

namespace ColorConquer
{
	class Program
	{
		static void Main(string[] args)
		{
			User Alice = new User();
			User Bob = new User();

			var game = new ColorConquerGame(Alice, Bob);
			game.StartGame();

			while (true)
			{
				var color = Console.ReadLine();
				"input: {0}".With(color);
				game.SetColor(game.CurrentTurn, (Color)Enum.Parse(typeof(Color), color));
				game.Print();
			}
		}
	}
}
