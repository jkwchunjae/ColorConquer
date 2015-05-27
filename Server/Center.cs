using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Extensions;
using Common;

namespace Server
{
	public static class ColorConquerCenter
	{
		//public static ConcurrentDictionary<Room, int /* 의미 없음 */> RoomList = new ConcurrentDictionary<Room, int>();
		public static RoomList RoomList = new RoomList();
		public static ConcurrentDictionary<User, Room> UserRoomDic = new ConcurrentDictionary<User, Room>();

		static ColorConquerCenter() { }

		public static bool EnterChannel(User user)
		{
			"EnterUser".Dump();
			//user.SendRoomList(RoomList);
			return UserRoomDic.TryAdd(user, null);
		}

		public static void LeaveChannel(User user)
		{
			"LeaveUser".Dump();
			LeaveRoom(user);
			Room room;
			UserRoomDic.TryRemove(user, out room); // dic 에서도 지운다.
		}

		//public static IEnumerable<Room> GetRoomList()
		//{
		//	return RoomList.AsEnumerable().Select(e => e.Key);
		//}

		public static bool CreateRoom(User user, string roomName)
		{
			"CreateRoom: {0}".With(roomName).Dump();
			if (user == null) return false;
			var room = new Room(roomName);
			if (RoomList.CreateRoom(room.RoomName))
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
				//int tmp;
				//if (room.IsEmpty) RoomList.TryRemove(room, out tmp);
				if (room.IsEmpty) RoomList.Remove(room);
				UserRoomDic.TryUpdate(user, null, room);
				"LeaveRoom Success".Dump();
			}
			return true;
		}
	}
}
