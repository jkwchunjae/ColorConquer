using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace ClientConsole
{
	public static class User
	{
		public static string UserName;
		public static string Password;

		public static string CurrentColor;

		public static UserStatus Status = UserStatus.None;
		public static UserPlace Place = UserPlace.None;

		public static bool TryLogin()
		{
			UserName = Utils.Input("UserName");
			return true;
		}

		public static bool IsValid(UserStatus status, UserPlace place)
		{
			return Status == status && Place == place;
		}

		public static bool IsValid(UserPlace place, UserStatus status)
		{
			return Status == status && Place == place;
		}

		public static bool IsValid(UserStatus status)
		{
			return Status == status;
		}

		public static bool IsValid(UserPlace place)
		{
			return Place == place;
		}
	}
}
