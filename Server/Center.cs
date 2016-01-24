using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Extensions;

namespace ColorConquerServer
{
	public static class ColorConquerCenter
	{
		//public static ConcurrentDictionary<Room, int /* 의미 없음 */> RoomList = new ConcurrentDictionary<Room, int>();
		public static RoomList RoomList = null;
		public static ConcurrentDictionary<User, Room> UserRoomDic = null;

		static ColorConquerCenter()
		{
			RoomList = new RoomList();
			UserRoomDic = new ConcurrentDictionary<User, Room>();
		}

		#region Enter, Leave Lobby
		public static bool EnterLobby(User user)
		{
			"EnterUser".Dump();
			if (user.UserName == null || user.UserName == string.Empty || user.UserName == "")
				return false;
			UserRoomDic.TryAdd(user, null);
			PacketProcessor.SendLobbyUserList();
			return true;
		}

		public static void LeaveLobby(User user)
		{
			"LeaveUser".Dump();
			LeaveRoom(user);
			Room room;
			UserRoomDic.TryRemove(user, out room); // dic 에서도 지운다.
			PacketProcessor.SendLobbyUserList();
		}
		#endregion

		#region Create, Enter, Leave Room
		public static bool CreateRoom(User user, string roomName, out Room room)
		{
			room = null;
			"CreateRoom: {0}".With(roomName).Dump();
			if (user == null) return false;
			if (!UserRoomDic.ContainsKey(user)) return false;

			if (UserRoomDic[user] == null && RoomList.CreateRoom(roomName, out room))
			{
				return EnterRoom(user, room.Id);
			}
			else
			{
				return false;
			}
		}

		public static bool EnterRoom(User user, int roomId)
		{
			"EnterRoom".Dump();
			if (roomId == 0) return false;

			if (UserRoomDic[user] != null) return false;

			var room = RoomList.Find(roomId);
			if (room == null) return false;

			if (!room.EnterUser(user))
				return false;

			UserRoomDic[user] = room;
			"EnterRoom Success".Dump();
			PacketProcessor.SendLobbyUserList();
			return true;
		}

		public static bool EnterRoomMonitor(User user, int roomId)
		{
			"EnterRoomMonitor".Dump();
			if (roomId == 0) return false;

			if (UserRoomDic[user] != null) return false;

			var room = RoomList.Find(roomId);
			if (room == null) return false;

			if (!room.EnterMonitor(user))
				return false;

			UserRoomDic[user] = room;
			"EnterRoomMonitor Success".Dump();
			PacketProcessor.SendLobbyUserList();
			return true;
		}

		public static bool LeaveRoom(User user)
		{
			"LeaveRoom".Dump();
			if (user == null) return false;
			if (!UserRoomDic.ContainsKey(user)) return false; // dic에 없으면 종료
			var room = UserRoomDic[user];
			if (room == null) return false;

			lock (room)
			{
				room.LeaveUser(user);
				room.LeaveMonitor(user);
				//int tmp;
				//if (room.IsEmpty) RoomList.TryRemove(room, out tmp);
				if (room.IsEmpty)
				{
					RoomList.DeleteRoom(room.Id);
				}
				UserRoomDic.TryUpdate(user, null, room);
				"LeaveRoom Success".Dump();
			}
			PacketProcessor.SendLobbyUserList();
			return true;
		}
		#endregion

		public static void BroadcastRoomList()
		{
			if (UserRoomDic == null) return;
			foreach (var user in UserRoomDic.Where(e => e.Value == null).Select(e => e.Key))
				user.ResultRoomList();
		}
	}
}
