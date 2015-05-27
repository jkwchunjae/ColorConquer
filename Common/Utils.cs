using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace Common
{
	public static class Utils
	{
		public static string Input(string question = "")
		{
			Console.Write("{0}: ".With(question));
			return Console.ReadLine();
		}
	}
}
