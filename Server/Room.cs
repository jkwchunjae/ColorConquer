using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace Server
{
	class Room
	{
		User Alice, Bob;
		ColorConquerGame Game;

		public Room()
		{
			Alice = null;
			Bob = null;
			Game = null;
		}

		public bool IsEmpty { get { return (Alice == null && Bob == null); } }
		public bool IsFull { get { return (Alice != null && Bob != null); } }
		public bool IsGameRunning { get { return Game != null; } }

		public bool EnterUser(User user)
		{
			if (Alice == null)
			{
				Alice = user;
			}
			else if (Bob == null)
			{
				Bob = user;
			}
			else
			{
				return false;
			}
			return true;
		}

		public void LeaveUser(User user)
		{
			if (IsGameRunning)
			{
				// 게임 중간 종료에 대한 처리 필요
			}

			if (user == Alice) Alice = null;
			if (user == Bob) Bob = null;
		}

		public bool StartGame(int size, int countColor)
		{
			if (!IsFull) return false;

			Game = new ColorConquerGame(Alice, Bob, size, countColor);
			Game.StartGame();

			return true;
		}
	}

}
