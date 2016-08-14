using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorConquerServer
{
	public class Room
	{
		User Manager;
		User Alice, Bob;
		HashSet<User> Monitor = new HashSet<User>();
		public ColorConquerGame Game;
		public string RoomName;
		public bool IsShowScore = false;

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
				new JProperty("roomName", RoomName),
				new JProperty("isGameRunning", IsGameRunning.ToString().ToLower())
				);
		}

		public string ToJsonString()
		{
			return this.ToJson().ToString();
		}

		public bool IsEmpty { get { return (Alice == null && Bob == null); } }
		public bool IsFull { get { return (Alice != null && Bob != null); } }
		public bool IsGameRunning { get { return Game != null && Game.IsRunning; } }
		public bool IsAliceAi { get { return Alice is Ai; } }
		public bool IsBobAi { get { return Bob is Ai; } }

		public IEnumerable<User> GetUsers(bool includeMonitor = true)
		{
			if (Alice != null) yield return Alice;
			if (Bob != null) yield return Bob;
			if (includeMonitor)
			{
				lock (Monitor)
				{
					foreach (var user in Monitor)
					{
						yield return user;
					}
				}
			}
		}

		public bool IsAlice(User user)
		{
			return Alice == user;
		}

		public bool IsBob(User user)
		{
			return Bob == user;
		}

		public bool IsManager(User user)
		{
			return Manager == user;
		}
		
		public bool IsAi(User user)
		{
			return user is Ai;
		}

		#region Enter/Leave User, Monitor
		public bool EnterUser(User user)
		{
			lock(RoomName)
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
				if (Manager == null)
					Manager = user;
			}
			this.SendUserList();
			ColorConquerCenter.RoomList.UpdateJsonString();
			return true;
		}

		public void LeaveUser(User user)
		{
			if (IsGameRunning && (user == Alice || user == Bob))
			{
				// 게임 중간 종료에 대한 처리 필요
				var winner = user == Bob ? Alice : Bob;
				var loser = user == Alice ? Alice : Bob;
				OnFinish(winner, loser);
			}

			if (user == Alice)
			{
				if (Bob is Ai)
					Bob = null;
				if (Manager == Alice)
					Manager = Bob;
				Alice = null;
			}
			else if (user == Bob)
			{
				if (Alice is Ai)
					Alice = null;
				if (Manager == Bob)
					Manager = Alice;
				Bob = null;
			}
			this.SendUserList();
			ColorConquerCenter.RoomList.UpdateJsonString();
		}

		public void EnterAi()
		{
			if (Alice == null && Bob == null) throw new Exception("방이 꽉 차있으면 AI를 추가할 수 없습니다.");
			if (Alice is Ai || Bob is Ai) throw new Exception("이미 AI가 설정되어 있습니다.");

			if (Alice == null)
			{
				Alice = new Ai();
			}
			else if (Bob == null)
			{
				Bob = new Ai();
			}
			else
			{
				throw new Exception("알수 없는 오류입니다.");
			}
			this.SendUserList();
		}

		public void LeaveAi()
		{
			if (Alice is Ai)
			{
				Alice = null;
			}
			else if (Bob is Ai)
			{
				Bob = null;
			}
			else
			{
				throw new Exception("AI가 없습니다.");
			}
			this.SendUserList();
		}

		public bool EnterMonitor(User user)
		{
			lock (Monitor)
			{
				Monitor.Add(user);
			}
			this.SendUserList();
			if (IsGameRunning)
			{
				this.SendGameStatus(user);
			}
			return true;
		}

		public void LeaveMonitor(User user)
		{
			lock (Monitor)
			{
				Monitor.Remove(user);
			}
			this.SendUserList();
		}
		#endregion

		public void Giveup(User giveupUser)
		{
			var winner = Alice == giveupUser ? Bob : Alice;
			var loser = Alice != giveupUser ? Bob : Alice;
			OnFinish(winner, loser);
		}

		public void OnFinish(User winner, User loser)
		{
			// winner, loser에 대한 승패 기록
			// 알림
			// 게임 종료

			// 여기서 게임 끝났는지 검사하면 안된다.
			// 중간에 유저가 나가서 끝나는 경우도 있으니까..
			// if (!Game.IsFinished) return;

			if (!(loser is Ai))
				Manager = loser;

			#region DB
			#endregion

			#region Broadcast message
			this.ResultGameFinish(winner, loser);
			#endregion

			Game = null;
			this.SendUserList();
		}

		#region Chatting
		public void Chat(User speaker, string message)
		{
			foreach (var user in this.GetUsers())
			{
				user.ChatRoom(speaker.UserName, message);
			}
		}

		public void PlayerChat(User speaker, string message)
		{
			if (!this.GetUsers(false).Contains(speaker))
				return;
			foreach (var user in this.GetUsers(false))
			{
				//user.PlayerChat(speaker.UserName, message);
			}
		}

		public void MonitorChat(User speaker, string message)
		{
			if (!Monitor.Contains(speaker))
				return;
			foreach (var user in Monitor)
			{
				//user.MonitorChat(speaker.UserName, message);
			}
		}
		#endregion

		public void StartGame(User user, int size, int countColor)
		{
			if (!IsFull) throw new GameStartException("방이 꽉차지 않았습니다.");
			if (!IsManager(user)) throw new GameStartException("게임 시작 권한이 없습니다.");
			if (IsGameRunning) throw new GameStartException("현재 게임이 진행중입니다.");
			if (!(size >= 7 && size <= 15)) throw new GameStartException("크기가 적절하지 않습니다. 7~15");
			if (size % 2 == 0) throw new GameStartException("크기는 홀수여야 합니다.");
			if (!(countColor >= 5 && countColor <= 6)) throw new GameStartException("색상수가 적절하지 않습니다. (5~6)");

			Game = new ColorConquerGame(Alice, Bob, size, countColor);
			Game.StartGame();
			ColorConquerCenter.RoomList.UpdateJsonString();
		}
	}

	public class GameStartException : Exception
	{
		public GameStartException(string message)
			: base(message)
		{
		}
	}

	public class RoomList : List<Room>
	{
		Dictionary<string, Room> _roomNameDic = new Dictionary<string, Room>();
		string _jsonString;
		public string JsonString { get { return _jsonString; } }

		public RoomList()
		{
			this.UpdateJsonString(false);
		}

		public string UpdateJsonString(bool isBroadcast = true)
		{
			_jsonString = JsonConvert.SerializeObject(this.Select(e => e.ToJson()));
			if (isBroadcast) ColorConquerCenter.BroadcastRoomList();
			return _jsonString;
		}

		public bool CreateRoom(string roomName)
		{
			if (roomName == null || roomName == string.Empty || roomName == "")
				return false;

			lock (_roomNameDic)
			{
				if (_roomNameDic.ContainsKey(roomName)) return false;
				var room = new Room(roomName);
				_roomNameDic.Add(roomName, room);
				this.Add(room);
			}
			this.UpdateJsonString();
			return true;
		}

		public void DeleteRoom(string roomName)
		{
			lock (_roomNameDic)
			{
				if (_roomNameDic.ContainsKey(roomName))
				{
					var room = _roomNameDic[roomName];
					_roomNameDic.Remove(roomName);
					this.Remove(room);
				}
			}
			this.UpdateJsonString();
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
}
