﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Extensions;
using Newtonsoft.Json;

namespace ColorConquerServer
{
	public static class PacketProcessor
	{
		public static void ProcessPacket(Socket socket, PacketType packetType, byte[] bytes)
		{
			//var user = socket.GetUser();
			//if (user == null) return;
			//user.ProcessPacket(packetType, bytes.GetStringUTF8());
		}

		public static void ProcessPacket(this User user, string json)
		{
			try
			{
				dynamic obj = JsonConvert.DeserializeObject(json);
				PacketType packetType;
				if (Enum.TryParse<PacketType>((string)obj.packetType, out packetType))
				{
					user.ProcessPacket(packetType, json);
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
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
							dynamic obj = JsonConvert.DeserializeObject(json);
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
							dynamic obj = JsonConvert.DeserializeObject(json);
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
							dynamic obj = JsonConvert.DeserializeObject(json);
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
				#region TryInsertAi
				case PacketType.TryInsertAi:
					{
						try
						{
							if (!ColorConquerCenter.UserRoomDic.ContainsKey(user)) break;
							var room = ColorConquerCenter.UserRoomDic[user];

							room.EnterAi();
						}
						catch
						{
						}
						break;
					}
				#endregion
				#region TryRemoveAi
				case PacketType.TryRemoveAi:
					{
						try
						{

						}
						catch
						{
						}
						break;
					}
				#endregion
				#region TryEnterRoomMonitor
				case PacketType.TryEnterRoomMonitor:
					{
						try
						{
							dynamic obj = JsonConvert.DeserializeObject(json);
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
							dynamic obj = JsonConvert.DeserializeObject(json);
							var message = (string)obj.message;
							if (message == null || message.Length == 0)
								break;
							room.Chat(user, message);
						}
						catch { }
						break;
					}
				#endregion
				#region ChatChannel
				case PacketType.ChatChannel:
					{
						if (!ColorConquerCenter.UserRoomDic.ContainsKey(user)) break;
						try
						{
							dynamic obj = JsonConvert.DeserializeObject(json);
							var message = (string)obj.message;
							if (message == null || message.Length == 0)
								break;
							ChatChannel(user, message);
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
							dynamic obj = JsonConvert.DeserializeObject(json);
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

						var result = false;
						try
						{
							dynamic obj = JsonConvert.DeserializeObject(json);
							var color = (Color)Enum.Parse(typeof(Color), ((string)obj.color).ToUpper());
							room.Game.SetColor(user, color);
							room.ResultClickCell(user, result: true);
							result = true;
						}
						catch (SetColorException ex)
						{
							Logger.Log(ex);
							result = false;
							room.ResultClickCell(user, failureMessage: ex.Message);
						}
						catch (Exception ex)
						{
							Logger.Log(ex);
							result = false;
							room.ResultClickCell(user, failureMessage: "알 수 없는 에러입니다.");
						}

						if (room.Game.IsFinished)
						{
							room.OnFinish(room.Game.Winner, room.Game.Loser);
						}
						else if (room.IsAlice(user) && result && room.IsBobAi)
						{
							// Alice의 셀 클릭이 성공적이고, Bob이 Ai 일 경우
							// Ai 로직은 여기로 들어가면 된다.
							// 잘 계산해서 room.Game.SetColor(bob, color) 하면 된다.

							/*
							 * var color = GetAiColor(...);
							 * room.Game.SetColorAi(color); // SetColorAi 함수 만듬.
							*/
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
		public static void BroadcastMessage(PacketType packetType, string json, bool includeRoom = false)
		{
			foreach (var user in ColorConquerCenter.UserRoomDic.Where(e => (includeRoom ? true : e.Value == null)).Select(e => e.Key))
			{
				lock (user)
				{
					if (user == null) continue;
					user.SendAsync(packetType, json);
				}
			}
		}
		static void BroadcastMessage(this Room room, PacketType packetType, string json, bool includeMonitor = true)
		{
			foreach (var user in room.GetUsers(includeMonitor))
			{
				lock (user)
				{
					if (user == null) continue;
					if (user is Ai) continue;
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
		#region ResultInsertAi
		public static void ResultInsertAi(this User user, bool result)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultInsertAi, json);
		}
		#endregion
		#region ResultRemoveAi
		public static void ResultRemoveAi(this User user, bool result)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultRemoveAi, json);
		}
		#endregion
		#region SendUserList
		public static void SendChannelUserList()
		{
			dynamic obj = new ExpandoObject();

			lock (ColorConquerCenter.UserRoomDic)
			{
				obj = ColorConquerCenter.UserRoomDic
					.Select(e => new
					{
						userName = e.Key.UserName,
						userImage = e.Key.UserImage,
						roomName = e.Value == null ? "" : e.Value.RoomName
					}).ToArray();
			}
			string json = JsonConvert.SerializeObject(obj);
			BroadcastMessage(PacketType.UserList, json, includeRoom: false);
		}
		public static void SendUserList(this Room room)
		{
			dynamic obj = new ExpandoObject();
			obj = room.GetUsers().Select(user => new
			{
				userName = user.UserName,
				userImage = user.UserImage,
				userType = (room.IsAlice(user) ? "Alice" : (room.IsBob(user) ? "Bob" : "Monitor")),
				isManager = room.IsManager(user),
				isAi = room.IsAi(user),
			}).ToArray();
			string json = JsonConvert.SerializeObject(obj);
			room.BroadcastMessage(PacketType.UserList, json);
		}
		#endregion
		#region ChatRoom
		public static void ChatChannel(this User speaker, string message)
		{
			dynamic obj = new ExpandoObject();
			obj.speakerName = speaker.UserName;
			obj.message = message;
			string json = JsonConvert.SerializeObject(obj);
			BroadcastMessage(PacketType.ChatChannel, json, includeRoom: false);
		}
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
				obj.aliceScore = room.IsShowScore ? game.GetUserScore(game.Alice).ToString() : "";
				obj.bobName = game.Bob.UserName;
				obj.bobColor = game.Bob.CurrentColor.ToString();
				obj.bobScore = room.IsShowScore ? game.GetUserScore(game.Bob).ToString() : "";
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
		public static void ResultClickCell(this Room room, User clickUser, bool result = false, string failureMessage = "")
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
				obj.aliceScore = room.IsShowScore ? game.GetUserScore(game.Alice).ToString() : "";
				obj.bobName = game.Bob.UserName;
				obj.bobColor = game.Bob.CurrentColor.ToString();
				obj.bobScore = room.IsShowScore ? game.GetUserScore(game.Bob).ToString() : "";
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
			obj.aliceScore = room.Game.GetAliceScore();
			obj.bobScore = room.Game.GetBobScore();
			string json = JsonConvert.SerializeObject(obj);
			room.BroadcastMessage(PacketType.GameFinished, json);
		}
		#endregion
	}
}
