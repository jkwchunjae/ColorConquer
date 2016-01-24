using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorConquerServer
{
	public enum PacketType
	{
		#region Client -> Server
		TryEnterLobby,
		RequestRoomList,
		TryInsertAi,
		TryRemoveAi,
		TryCreateRoom,
		TryEnterRoom,
		TryEnterRoomMonitor,
		TryLeaveRoom,
		TryStartGame,
		GiveupGame,
		ClickCell,
		#endregion

		#region Server -> Client
		RsaPublicKey,
		ResultEnterLobby,
		ResultRoomList,
		ResultEnterRoom,
		ResultEnterRoomMonitor,
		UserList,
		ResultLeaveRoom,
		ResultStartGame,
		ResultClickCell,
		GameStatus,
		GameFinished,
		#endregion

		#region Others (Both)
		ChatLobby,
		ChatRoom,
		Shutdown,
		#endregion
	}

	public enum UserStatus
	{
		None,
		TryEnterLobby,
		TryCreateRoom,
		TryEnterRoom,
		TryStartGame,
		GameStarted,
	}

	public enum UserPlace
	{
		None,
		Lobby,
		Room,
	}

	public enum Color
	{
		A, B, C, D, E, F, G, H, I, J
	}
}
