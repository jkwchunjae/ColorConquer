using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
	public class RoomList: List<Room>
	{
		Dictionary<string, Room> _roomNameDic = new Dictionary<string, Room>();
		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(this.Select(e => e.ToJson()));
		}

		public bool CreateRoom(string roomName)
		{
			lock (_roomNameDic)
			{
				if (_roomNameDic.ContainsKey(roomName)) return false;
				var room = new Room(roomName);
				_roomNameDic.Add(roomName, room);
				this.Add(room);
			}
			return true;
		}

		public void DeleteRoom(string roomName)
		{
			lock(_roomNameDic)
			{
				if (_roomNameDic.ContainsKey(roomName))
				{
					var room  = _roomNameDic[roomName];
					_roomNameDic.Remove(roomName);
					this.Remove(room);
				}
			}
		}

		public Room Find(string roomName)
		{
			lock (_roomNameDic)
			{
				if (_roomNameDic.ContainsKey(roomName))
					return _roomNameDic[roomName];
				return null;
			}
		}
	}

	public class Room
	{
		User Alice, Bob;
		public ColorConquerGame Game;
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

		public IEnumerable<User> GetUsers()
		{
			if (Alice != null) yield return Alice;
			if (Bob != null) yield return Bob;
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

		public void Chat(User speaker, string message)
		{
			foreach (var user in this.GetUsers())
			{
				user.ChatRoom(speaker.UserName, message);
			}
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
