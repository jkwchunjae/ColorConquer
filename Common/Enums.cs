using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public enum PacketType
	{
		#region Client -> Server
		EnterChannel,
		CreateRoom,
		EnterRoom,
		LeaveRoom,
		LeaveChannel,
		StartGame,
		SetColor,
		#endregion

		#region Server -> Client
		RoomList,
		#endregion
	}

	public enum UserStatus
	{
		Channel,
		EnterRoomWaiting,
		Room,
		GameRunning,
	}

	public enum Color
	{
		A, B, C, D, E, F
	}
}
