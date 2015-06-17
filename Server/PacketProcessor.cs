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
							user.UserName = (string)obj.userName;
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
						try
						{
							dynamic obj = json.JsonDeserialize();
							int size = ((string)obj.size).ToInt();
							int countColor = ((string)obj.countColor).ToInt();
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
			obj.result = result.ToString().ToLower();
			obj.userName = user.UserName;
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
			obj.result = result.ToString().ToLower();
			if (result)
			{
				obj.roomName = roomName;
			}
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultEnterRoom, json);
		}

		public static void ResultLeaveRoom(this User user, bool result)
		{
			dynamic obj = new ExpandoObject();
			obj.result = result.ToString().ToLower();
			obj.userName = user.UserName;
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ResultLeaveRoom, json);
		}

		public static void SendUserList(this User user, IEnumerable<User> userList)
		{
			dynamic obj = new ExpandoObject();
			obj = userList.Select(e => new { userName = e.UserName }).ToArray();
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.UserList, json);
		}

		public static void ChatRoom(this User user, string speakerName, string message)
		{
			dynamic obj = new ExpandoObject();
			obj.speakerName = speakerName;
			obj.message = message;
			string json = JsonConvert.SerializeObject(obj);
			user.SendAsync(PacketType.ChatRoom, json);
		}

		public static void ResultStartGame(this Room room, bool result)
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
				obj.bobName = game.Bob.UserName;
				obj.aliceColor = game.Alice.CurrentColor.ToString();
				obj.bobColor = game.Bob.CurrentColor.ToString();
				obj.cellsColor = game.CellsColor;
			}
			string json = JsonConvert.SerializeObject(obj);
			foreach (var user in room.GetUsers())
			{
				user.SendAsync(PacketType.ResultStartGame, json);
			}
		}
	}
}
