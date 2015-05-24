using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public enum PacketType
	{
		EnterChannel,
		CreateRoom,
		EnterRoom,
		LeaveRoom,
		LeaveChannel,
		StartGame,
		SetColor,
	}

	public enum Color
	{
		A, B, C, D, E, F
	}
}
