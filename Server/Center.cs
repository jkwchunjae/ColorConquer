using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Server
{
	static public class ColorConquerCenter
	{
		static public ConcurrentDictionary<Room, int /* 의미 없음 */> RoomList = new ConcurrentDictionary<Room, int>();
		static public ConcurrentDictionary<User, Room> UserRoomDic = new ConcurrentDictionary<User, Room>();

		static ColorConquerCenter() { }

		static public bool EnterUser(User user)
		{
			return UserRoomDic.TryAdd(user, null);
		}

		static public void LeaveUser(User user)
		{
			LeaveRoom(user);
			Room room;
			UserRoomDic.TryRemove(user, out room); // dic 에서도 지운다.
		}

		static public IEnumerable<Room> GetRoomList()
		{
			return RoomList.AsEnumerable().Select(e => e.Key);
		}

		static public bool CreateRoom(User user)
		{
			if (user == null) return false;
			var room = new Room();
			RoomList.TryAdd(room, 0);
			return EnterRoom(user, room);
		}

		static public bool EnterRoom(User user, Room room)
		{
			if (user == null || room == null) return false;

			lock (room)
			{
				if (room.EnterUser(user))
				{
					UserRoomDic[user] = room;
					return true;
				}
			}
			return false;
		}

		static public bool LeaveRoom(User user)
		{
			if (user == null) return false;
			if (!UserRoomDic.ContainsKey(user)) return false; // dic에 없으면 종료
			var room = UserRoomDic[user];
			if (room == null) return false;

			lock (room)
			{
				room.LeaveUser(user);
				int tmp;
				if (room.IsEmpty) RoomList.TryRemove(room, out tmp);
				UserRoomDic.TryUpdate(user, null, room);
			}
			return true;
		}
	}
}
