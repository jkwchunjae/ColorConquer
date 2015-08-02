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

		#region Enter, Leave Channel
		public static bool EnterChannel(User user)
		{
			"EnterUser".Dump();
			if (user.UserName == null || user.UserName == string.Empty || user.UserName == "")
				return false;
			UserRoomDic.TryAdd(user, null);
			PacketProcessor.SendChannelUserList();
			return true;
		}

		public static void LeaveChannel(User user)
		{
			"LeaveUser".Dump();
			LeaveRoom(user);
			Room room;
			UserRoomDic.TryRemove(user, out room); // dic 에서도 지운다.
			PacketProcessor.SendChannelUserList();
		}
		#endregion

		#region Create, Enter, Leave Room
		public static bool CreateRoom(User user, string roomName)
		{
			"CreateRoom: {0}".With(roomName).Dump();
			if (user == null) return false;
			var room = new Room(roomName);
			if (UserRoomDic[user] == null && RoomList.CreateRoom(room.RoomName))
			{
				return EnterRoom(user, room.RoomName);
			}
			else
			{
				return false;
			}
		}

		public static bool EnterRoom(User user, string roomName)
		{
			"EnterRoom".Dump();
			if (user == null || roomName == null || roomName.Length == 0) return false;

			if (UserRoomDic[user] != null) return false;

			var room = RoomList.Find(roomName);
			if (room == null) return false;

			lock (room)
			{
				if (room.EnterUser(user))
				{
					UserRoomDic[user] = room;
					"EnterRoom Success".Dump();
					PacketProcessor.SendChannelUserList();
					return true;
				}
			}
			return false;
		}

		public static bool EnterRoomMonitor(User user, string roomName)
		{
			"EnterRoomMonitor".Dump();
			if (user == null || roomName == null || roomName.Length == 0) return false;

			if (UserRoomDic[user] != null) return false;

			var room = RoomList.Find(roomName);
			if (room == null) return false;

			lock (room)
			{
				if (room.EnterMonitor(user))
				{
					UserRoomDic[user] = room;
					"EnterRoomMonitor Success".Dump();
					PacketProcessor.SendChannelUserList();
					return true;
				}
			}
			return false;
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
					RoomList.DeleteRoom(room.RoomName);
				}
				UserRoomDic.TryUpdate(user, null, room);
				"LeaveRoom Success".Dump();
			}
			PacketProcessor.SendChannelUserList();
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
