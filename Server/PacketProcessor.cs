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

namespace Server
{
	public static class PacketProcessor
	{
		public static void ProcessPacket(Socket socket, PacketType packetType, string json = "")
		{
			var user = socket.GetUser();
			if (user == null) return;
			switch (packetType)
			{
				#region TryEnterChannel
				case PacketType.TryEnterChannel:
					{
						try
						{
							var result = ColorConquerCenter.EnterChannel(user);
							var obj = json.JsonDeserialize();
							user.UserName = (string)obj.UserName;
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
							var roomName = (string)obj.RoomName;
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
							var roomName = (string)obj.RoomName;
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

				#region ChatRoom
				case PacketType.ChatRoom:
					{
						if (!ColorConquerCenter.UserRoomDic.ContainsKey(user)) break;
						var room = ColorConquerCenter.UserRoomDic[user];
						try
						{
							dynamic obj = json.JsonDeserialize();
							var message = (string)obj.Message;
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
						try
						{
							dynamic obj = json.JsonDeserialize();
							int size = ((string)obj.Size).ToInt();
							int countColor = ((string)obj.CountColor).ToInt();
							var result = room.StartGame(size, countColor);
							room.ResultStartGame(result);
						}
						catch
						{
							room.ResultStartGame(false);
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

		public static void ResultEnterChannel(this User user, bool result)
		{
			dynamic obj = new ExpandoObject();
			obj.Result = result ? "true" : "false";
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultEnterChannel, json);
		}

		public static void ResultRoomList(this User user)
		{
			user.SendAsync(PacketType.ResultRoomList, ColorConquerCenter.RoomList.JsonString);
		}

		public static void ResultEnterRoom(this User user, bool result, string roomName = null)
		{
			dynamic obj = new ExpandoObject();
			obj.Result = result ? "true" : "false";
			if (result)
			{
				obj.RoomName = roomName;
			}
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultEnterRoom, json);
		}

		public static void ChatRoom(this User user, string speakerName, string message)
		{
			dynamic obj = new ExpandoObject();
			obj.SpeakerName = speakerName;
			obj.Message = message;
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ChatRoom, json);
		}

		public static void ResultStartGame(this Room room, bool result)
		{
			dynamic obj = new ExpandoObject();
			obj.Result = result ? "true" : "false";
			if (result)
			{
				var game = room.Game;
				obj.Size = game.Size;
				obj.CountColor = game.CountColor;
				obj.CurrentTurnName = game.CurrentTurn.UserName;
				obj.AliceName = game.Alice.UserName;
				obj.BobName = game.Bob.UserName;
				obj.AliceColor = game.Alice.CurrentColor.ToString();
				obj.BobColor = game.Bob.CurrentColor.ToString();
				obj.CellsColor = game.CellsColor;
			}
			string json = JsonConvert.SerializeObject(obj);
			foreach (var user in room.GetUsers())
			{
				user.SendAsync(PacketType.ResultStartGame, json);
			}
		}
	}
}
