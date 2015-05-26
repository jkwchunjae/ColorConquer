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
		#endregion

		#region Server -> Client
		#endregion
	}

	public enum UserStatus
	{
		None,
		TryEnterChannel,
		TryCreateRoom,
		TryEnterRoom,
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
