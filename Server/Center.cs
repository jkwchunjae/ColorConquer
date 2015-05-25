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
	static public class ColorConquerCenter
	{
		//static public ConcurrentDictionary<Room, int /* 의미 없음 */> RoomList = new ConcurrentDictionary<Room, int>();
		static public RoomList RoomList = new RoomList();
		static public ConcurrentDictionary<User, Room> UserRoomDic = new ConcurrentDictionary<User, Room>();

		static ColorConquerCenter() { }

		static public bool EnterUser(User user)
		{
			"EnterUser".Dump();
			user.SendRoomList(RoomList);
			return UserRoomDic.TryAdd(user, null);
		}

		static public void LeaveUser(User user)
		{
			"LeaveUser".Dump();
			LeaveRoom(user);
			Room room;
			UserRoomDic.TryRemove(user, out room); // dic 에서도 지운다.
		}

		//static public IEnumerable<Room> GetRoomList()
		//{
		//	return RoomList.AsEnumerable().Select(e => e.Key);
		//}

		static public bool CreateRoom(User user, Room room)
		{
			"CreateRoom: {0}".With(room.RoomName).Dump();
			if (user == null) return false;
			//var room = new Room(roomName);
			//RoomList.TryAdd(room, 0);
			RoomList.CreateRoom(room.RoomName);
			return EnterRoom(user, room.RoomName);
		}

		static public bool EnterRoom(User user, string roomName)
		{
			"EnterRoom".Dump();
			if (user == null || roomName == null || roomName.Length == 0) return false;

			var room = RoomList.Where(e => e.RoomName == roomName).FirstOrDefault();

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

		static public bool LeaveRoom(User user)
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
