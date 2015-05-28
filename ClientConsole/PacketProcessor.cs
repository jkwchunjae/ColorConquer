using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Extensions;
using Newtonsoft.Json;
using Common;
using Newtonsoft.Json.Linq;

namespace ClientConsole
{
	public static class PacketProcessor
	{
		/// <summary>
		/// Process packet that received from server
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="packetType"></param>
		/// <param name="json"></param>
		public static void ProcessPacket(Socket socket, PacketType packetType, string json = "")
		{
			switch (packetType)
			{
				#region ResultEnterChannel
				case PacketType.ResultEnterChannel:
					{
						try
						{
							dynamic obj = JsonConvert.DeserializeObject(json);
							string result = (string)obj.Result;
							if (User.Status != UserStatus.TryEnterChannel) break;
							User.Status = UserStatus.None;
							if (result == "true")
							{
								User.Place = UserPlace.Channel;
								socket.RequestRoomList();
							}
							else
							{
							}
						}
						catch
						{
							User.Status = UserStatus.None;
							User.Place = UserPlace.None;
						}
						break;
					}
				#endregion

				#region ResultRoomList
				case PacketType.ResultRoomList:
					{
						try
						{
							List<dynamic> roomList = json.JsonDeserialize<List<dynamic>>();
							"RoomList Count: {0}".With(roomList.Count).Dump();
							foreach (var room in roomList)
							{
								"Room: {0}".With((string)room.RoomName).Dump();
							}
						}
						catch { }
						break;
					}
				#endregion

				#region ResultEnterRoom
				case PacketType.ResultEnterRoom:
					{
						try
						{
							if (!User.IsValid(UserStatus.TryEnterRoom)) break;
							dynamic obj = JsonConvert.DeserializeObject(json);
							string result = (string)obj.Result;
							string roomName = (string)obj.RoomName;
							User.Status = UserStatus.None;
							if (result == "false")
							{
								"EnterRoom Fail.. {0}".With(roomName).Dump();
								return;
							}
							"EnterRoom Success!! {0}".With(roomName).Dump();
							User.Place = UserPlace.Room;
						}
						catch
						{
							User.Status = UserStatus.None;
						}
						break;
					}
				#endregion

				#region ChatRoom
				case PacketType.ChatRoom:
					{
						try
						{
							if (!User.IsValid(UserPlace.Room)) break;
							dynamic obj = JsonConvert.DeserializeObject(json);
							string speakerName = (string)obj.SpeakerName;
							string message = (string)obj.Message;
							"{0}: {1}".With(speakerName, message).Dump();
						}
						catch { }
						break;
					}
				#endregion

				#region ResultStartGame
				case PacketType.ResultStartGame:
					{
						try
						{
							if (!User.IsValid(UserPlace.Room)) break;
							dynamic obj = json.JsonDeserialize();
							string result = (string)obj.Result;
							if (result == "false")
							{
								User.Status = UserStatus.None;
								break;
							}
							User.Status = UserStatus.GameStarted;
							int size = ((string)obj.Size).ToInt();
							int countColor = ((string)obj.CountColor).ToInt();
							string currentTurnName = (string)obj.CurrentTurnName;
							string aliceName = (string)obj.AliceName;
							string bobName = (string)obj.BobName;
							string aliceColor = (string)obj.AliceColor;
							string bobColor = (string)obj.BobColor;
							List<string> cellsColor = ((JArray)obj.CellsColor).Select(e => e.ToString()).ToList();

							User.CurrentColor = User.UserName == aliceName ? aliceColor : bobColor;

							"Size: {0}".With(size).Dump();
							"Countcolor: {0}".With(countColor).Dump();
							"CurrentTurnName: {0}".With(currentTurnName).Dump();
							"Cells".Dump();
							foreach (var colors in cellsColor)
								colors.Dump();
						}
						catch
						{
							User.Status = UserStatus.None;
						}
						break;
					}
				#endregion

				#region Shutdown
				case PacketType.Shutdown:
					{
						"Disconnected Server".Dump();
						User.Place = UserPlace.None;
						User.Status = UserStatus.None;
						return;
					}
				#endregion
			}
		}

		public static void TryEnterChannel(this Socket socket, string userName)
		{
			if (!User.IsValid(UserStatus.None, UserPlace.None)) return;
			dynamic obj = new ExpandoObject();
			obj.UserName = userName;
			string json = JsonConvert.SerializeObject(obj);
			User.Status = UserStatus.TryEnterChannel;
			socket.SendAsync(PacketType.TryEnterChannel, json);
		}

		public static void RequestRoomList(this Socket socket)
		{
			if (!User.IsValid(UserStatus.None, UserPlace.Channel)) return;
			socket.SendAsync(PacketType.RequestRoomList);
		}

		public static void TryCreateRoom(this Socket socket, string roomName)
		{
			if (!User.IsValid(UserStatus.None, UserPlace.Channel)) return;
			dynamic obj = new ExpandoObject();
			obj.RoomName = roomName;
			string json = JsonConvert.SerializeObject(obj);
			User.Status = UserStatus.TryEnterRoom;
			socket.SendAsync(PacketType.TryCreateRoom, json);
		}

		public static void TryEnterRoom(this Socket socket, string roomName)
		{
			if (!User.IsValid(UserStatus.None, UserPlace.Channel)) return;
			dynamic obj = new ExpandoObject();
			obj.RoomName = roomName;
			string json = JsonConvert.SerializeObject(obj);
			User.Status = UserStatus.TryEnterRoom;
			socket.SendAsync(PacketType.TryEnterRoom, json);
		}

		public static void ChatRoom(this Socket socket, string message)
		{
			if (!User.IsValid(UserPlace.Room)) return;
			dynamic obj = new ExpandoObject();
			obj.Message = message;
			string json = JsonConvert.SerializeObject(obj);
			socket.SendAsync(PacketType.ChatRoom, json);
		}

		public static void TryStartGame(this Socket socket, int size, int countColor)
		{
			if (!User.IsValid(UserStatus.None, UserPlace.Room)) return;
			User.Status = UserStatus.TryStartGame;
			dynamic obj = new ExpandoObject();
			obj.Size = size;
			obj.CountColor = countColor;
			string json = JsonConvert.SerializeObject(obj);
			socket.SendAsync(PacketType.TryStartGame, json);
		}
	}
}
