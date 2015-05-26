using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common
{
	public class RoomList: List<Room>
	{
		HashSet<string> _roomNameSet = new HashSet<string>();
		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(this.Select(e => e.ToJson()));
			//var roomListJson = new JObject(new JProperty("RoomList", this.Select(e => e.ToJson())));
			//return roomListJson.ToString();
		}

		public bool CreateRoom(string roomName)
		{
			lock (_roomNameSet)
			{
				if (_roomNameSet.Contains(roomName)) return false;
				_roomNameSet.Add(roomName);
				this.Add(new Room(roomName));
			}
			return true;
		}
	}

	public class Room
	{
		User Alice, Bob;
		ColorConquerGame Game;
		public string RoomName;

		public Room(string roomName)
		{
			Alice = null;
			Bob = null;
			Game = null;
			RoomName = roomName;
		}

		public JObject ToJson()
		{
			return new JObject(
				new JProperty("RoomName", RoomName)
				);
		}

		public string ToJsonString()
		{
			return this.ToJson().ToString();
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
