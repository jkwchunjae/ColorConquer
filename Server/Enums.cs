﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorConquerServer
{
	public enum PacketType
	{
		#region Client -> Server
		TryEnterChannel,
		RequestRoomList,
		TryCreateRoom,
		TryEnterRoom,
		TryEnterRoomMonitor,
		TryLeaveRoom,
		TryInsertAi,
		TryRemoveAi,
		TryStartGame,
		GiveupGame,
		ClickCell,
		#endregion

		#region Server -> Client
		RsaPublicKey,
		ResultEnterChannel,
		ResultRoomList,
		ResultEnterRoom,
		ResultEnterRoomMonitor,
		UserList,
		ResultLeaveRoom,
		ResultInsertAi,
		ResultRemoveAi,
		ResultStartGame,
		ResultClickCell,
		GameStatus,
		GameFinished,
		#endregion

		#region Others (Both)
		ChatChannel,
		ChatRoom,
		Shutdown,
		#endregion
	}

	public enum UserStatus
	{
		None,
		TryEnterChannel,
		TryCreateRoom,
		TryEnterRoom,
		TryStartGame,
		GameStarted,
	}

	public enum UserPlace
	{
		None,
		Channel,
		Room,
	}

	public enum Color
	{
		A, B, C, D, E, F, G, H, I, J
	}
}
