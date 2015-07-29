using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Common;
using Extensions;
using Newtonsoft.Json;
using Alchemy.Classes;

namespace Server
{
	public static class PacketProcessor
	{
		public static void ProcessPacket(Socket socket, PacketType packetType, byte[] bytes)
		{
			var user = socket.GetUser();
			if (user == null) return;
			user.ProcessPacket(packetType, bytes.GetStringUTF8());
		}

		public static void ProcessPacket(this User user, string json)
		{
			try
			{
				var obj = json.JsonDeserialize();
				PacketType packetType;
				if (Enum.TryParse<PacketType>((string)obj.packetType, out packetType))
				{
					user.ProcessPacket(packetType, json);
				}
			}
			catch
			{

			}
		}

		public static void ProcessPacket(this User user, PacketType packetType, string json = "")
		{
			switch (packetType)
			{
				#region TryEnterChannel
				case PacketType.TryEnterChannel:
					{
						try
						{
							var obj = json.JsonDeserialize();
							user.UserId = (string)obj.userId;
							user.UserName = (string)obj.userName;
							user.UserImage = (string)obj.userImage;
							var result = ColorConquerCenter.EnterChannel(user);
							user.ResultEnterChannel(result);
						}
						catch
						{
							user.ResultEnterChannel(false);
						}
						break;
					}
				#endregion
				#region RequestRoomList
				case PacketType.RequestRoomList:
					{
						try
						{
							user.ResultRoomList();
						}
						catch { }
						break;
					}
				#endregion
				#region TryCreateRoom
				case PacketType.TryCreateRoom:
					{
						try
						{
							dynamic obj = json.JsonDeserialize();
							var roomName = (string)obj.roomName;
							var result = ColorConquerCenter.CreateRoom(user, roomName);
							user.ResultEnterRoom(result, roomName);
						}
						catch
						{
							user.ResultEnterRoom(false);
						}
						break;
					}
				#endregion
				#region TryEnterRoom
				case PacketType.TryEnterRoom:
					{
						try
						{
							dynamic obj = json.JsonDeserialize();
							var roomName = (string)obj.roomName;
							var result = ColorConquerCenter.EnterRoom(user, roomName);
							user.ResultEnterRoom(result, roomName);
						}
						catch
						{
							user.ResultEnterRoom(false);
						}
						break;
					}
				#endregion
				#region TryEnterRoomMonitor
				case PacketType.TryEnterRoomMonitor:
					{
						try
						{
							dynamic obj = json.JsonDeserialize();
							var roomName = (string)obj.roomName;
							var result = ColorConquerCenter.EnterRoomMonitor(user, roomName);
							user.ResultEnterRoomMonitor(result, roomName);
						}
						catch (Exception ex)
						{
							Logger.Log(ex);
							user.ResultEnterRoomMonitor(false);
						}
						break;
					}
				#endregion
				#region TryLeaveRoom
				case PacketType.TryLeaveRoom:
					{
						try
						{
							var result = ColorConquerCenter.LeaveRoom(user);
							user.ResultLeaveRoom(result);
						}
						catch
						{
							user.ResultLeaveRoom(false);
						}
						break;
					}
				#endregion
				#region ChatRoom
				case PacketType.ChatRoom:
					{
						if (!ColorConquerCenter.UserRoomDic.ContainsKey(user)) break;
						var room = ColorConquerCenter.UserRoomDic[user];
						try
						{
							dynamic obj = json.JsonDeserialize();
							var message = (string)obj.message;
							room.Chat(user, message);
						}
						catch { }
						break;
					}
				#endregion
				#region TryStartGame
				case PacketType.TryStartGame:
					{
						if (!ColorConquerCenter.UserRoomDic.ContainsKey(user)) break;
						var room = ColorConquerCenter.UserRoomDic[user];
						var result = true;
						string failMessage = null;
						try
						{
							dynamic obj = json.JsonDeserialize();
							int size = ((string)obj.size).ToInt();
							//int countColor = ((string)obj.countColor).ToInt();
							int countColor = 6; // 그냥 6개로 하자!
							room.StartGame(user, size, countColor);
						}
						catch (GameStartException ex)
						{
							Logger.Log(ex);
							result = false;
							failMessage = ex.Message;
						}
						catch (Exception ex)
						{
							Logger.Log(ex);
							result = false;
							failMessage = "알 수 없는 에러입니다.";
						}
						room.ResultStartGame(result, failMessage);
						break;
					}
				#endregion
				#region GiveupGame
				case PacketType.GiveupGame:
					{
						if (!ColorConquerCenter.UserRoomDic.ContainsKey(user)) break;
						var room = ColorConquerCenter.UserRoomDic[user];
						if (!room.IsGameRunning) break;
						room.Giveup(user);
						break;
					}
				#endregion
				#region ClickCell
				case PacketType.ClickCell:
					{
						if (!ColorConquerCenter.UserRoomDic.ContainsKey(user)) break;
						var room = ColorConquerCenter.UserRoomDic[user];
						try
						{
							dynamic obj = json.JsonDeserialize();
							var color = (Color)Enum.Parse(typeof(Color), ((string)obj.color).ToUpper());
							room.Game.SetColor(user, color);
							room.ResultClickCell(user, true, "");
						}
						catch (SetColorException ex)
						{
							Logger.Log(ex);
							room.ResultClickCell(user, false, ex.Message);
						}
						catch (Exception ex)
						{
							Logger.Log(ex);
							room.ResultClickCell(user, false, "알 수 없는 에러입니다.");
						}
						if (room.Game.IsFinished)
						{
							room.OnFinish(room.Game.Winner, room.Game.Loser);
						}
						break;
					}
				#endregion
				#region Shutdown
				case PacketType.Shutdown:
					{
						ColorConquerCenter.LeaveChannel(user);
						break;
					}
				#endregion
			}

		}

		#region Broadcasting
		static void BroadcastMessage(this Room room, PacketType packetType, string json, bool includeMonitor = true)
		{
			foreach (var user in room.GetUsers(includeMonitor))
			{
				lock (user)
				{
					if (user == null) continue;
					user.SendAsync(packetType, json);
				}
			}
		}
		#endregion

		#region ResultEnterChannel
		public static void ResultEnterChannel(this User user, bool result)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			obj.userName = user.UserName;
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultEnterChannel, json);
		}
		#endregion
		#region ResultRoomList
		public static void ResultRoomList(this User user)
		{
			user.SendAsync(PacketType.ResultRoomList, ColorConquerCenter.RoomList.JsonString);
		}
		#endregion
		#region ResultEnterRoom
		public static void ResultEnterRoom(this User user, bool result, string roomName = null)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			if (result)
			{
				obj.roomName = roomName;
			}
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultEnterRoom, json);
		}
		#endregion
		#region ResultEnterRoomMonitor
		public static void ResultEnterRoomMonitor(this User user, bool result, string roomName = null)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			if (result)
			{
				obj.roomName = roomName;
			}
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultEnterRoomMonitor, json);
		}
		#endregion
		#region ResultLeaveRoom
		public static void ResultLeaveRoom(this User user, bool result)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			obj.userName = user.UserName;
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultLeaveRoom, json);
		}
		#endregion
		#region SendUserList
		public static void SendUserList(this Room room)
		{
			dynamic obj = new ExpandoObject();
			obj = room.GetUsers().Select(e => new { userName = e.UserName, userImage = e.UserImage, userType = (room.IsAlice(e) ? "Alice" : (room.IsBob(e) ? "Bob" : "Monitor")) }).ToArray();
			string json = JsonConvert.SerializeObject(obj);
			room.BroadcastMessage(PacketType.UserList, json);
		}
		#endregion
		#region ChatRoom
		public static void ChatRoom(this User user, string speakerName, string message)
		{
			dynamic obj = new ExpandoObject();
			obj.speakerName = speakerName;
			obj.message = message;
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ChatRoom, json);
		}
		#endregion
		#region ResultStartGame
		public static void ResultStartGame(this Room room, bool result, string failureMessage)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			if (result)
			{
				var game = room.Game;
				obj.size = game.Size;
				obj.countColor = game.CountColor;
				obj.currentTurnName = game.CurrentTurn.UserName;
				obj.aliceName = game.Alice.UserName;
				obj.aliceColor = game.Alice.CurrentColor.ToString();
				obj.aliceScore = game.GetUserScore(game.Alice);
				obj.bobName = game.Bob.UserName;
				obj.bobColor = game.Bob.CurrentColor.ToString();
				obj.bobScore = game.GetUserScore(game.Bob);
				obj.cellsColor = game.CellsColor;
			}
			else
			{
				obj.failureMessage = failureMessage;
			}
			string json = JsonConvert.SerializeObject(obj);
			foreach (var user in room.GetUsers())
			{
				user.SendAsync(PacketType.ResultStartGame, json);
			}
		}
		#endregion
		#region ResultClickCell
		public static void ResultClickCell(this Room room, User clickUser, bool result, string failureMessage)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			if (result)
			{
				var game = room.Game;
				obj.size = game.Size;
				obj.countColor = game.CountColor;
				obj.currentTurnName = game.CurrentTurn.UserName;
				obj.aliceName = game.Alice.UserName;
				obj.aliceColor = game.Alice.CurrentColor.ToString();
				obj.aliceScore = game.GetUserScore(game.Alice);
				obj.bobName = game.Bob.UserName;
				obj.bobColor = game.Bob.CurrentColor.ToString();
				obj.bobScore = game.GetUserScore(game.Bob);
				obj.cellsColor = game.CellsColor;
			}
			else
			{
				obj.failureMessage = failureMessage;
			}
			string json = JsonConvert.SerializeObject(obj);
			if (result)
			{
				room.BroadcastMessage(PacketType.ResultClickCell, json);
			}
			else
			{
				clickUser.SendAsync(PacketType.ResultClickCell, json);
			}
		}
		#endregion
		#region SendGameStatus
		public static void SendGameStatus(this Room room, User targetUser)
		{
			dynamic obj = new ExpandoObject();
			var game = room.Game;
			obj.size = game.Size;
			obj.countColor = game.CountColor;
			obj.currentTurnName = game.CurrentTurn.UserName;
			obj.aliceName = game.Alice.UserName;
			obj.aliceColor = game.Alice.CurrentColor.ToString();
			obj.aliceScore = game.GetUserScore(game.Alice);
			obj.bobName = game.Bob.UserName;
			obj.bobColor = game.Bob.CurrentColor.ToString();
			obj.bobScore = game.GetUserScore(game.Bob);
			obj.cellsColor = game.CellsColor;
			string json = JsonConvert.SerializeObject(obj);
			targetUser.SendAsync(PacketType.GameStatus, json);
		}
		#endregion
		#region ResultGameFinish
		public static void ResultGameFinish(this Room room, User winner, User loser)
		{
			dynamic obj = new ExpandoObject();
			obj.winnerName = winner.UserName;
			obj.loserName = loser.UserName;
			obj.winnerScore = room.Game.GetUserScore(winner);
			obj.loserScore = room.Game.GetUserScore(loser);
			string json = JsonConvert.SerializeObject(obj);
			room.BroadcastMessage(PacketType.GameFinished, json);
		}
		#endregion
	}
}
