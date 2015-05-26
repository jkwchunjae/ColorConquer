using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Common;

namespace Common
{
	public static class SocketExtensions
	{
		public static async void ReceiveLoop(this Socket socket, Action<Socket, PacketType, string /* json */> processPacket)
		{
			while (true)
			{
				try
				{
					var length = (await socket.ReceiveAsync(sizeof(int))).ConvertToInt32();
					"Recv length: {0}".With(length).Dump();
					if (length == 0)
					{
						processPacket(socket, PacketType.LeaveChannel, null);
						socket.Shutdown(SocketShutdown.Both);
						socket.Close();
						break;
					}
					var packetBytes = await socket.ReceiveAsync(length);
					var packetType = (PacketType)packetBytes.Take(sizeof(int)).ToArray().ConvertToInt32();
					"Recv type: {0}".With(packetType.ToString()).Dump();
					//socket.ProcessPacket(packetType, packetBytes.Skip(sizeof(int)).ToArray().GetStringUTF8());
					processPacket(socket, packetType, packetBytes.Skip(sizeof(int)).ToArray().GetStringUTF8());
				}
				catch
				{
					processPacket(socket, PacketType.LeaveChannel, null);
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
					break;
				}
			}
		}

		public static async void SendAsync(this Socket socket, PacketType packetType, string json = "")
		{
			byte[] bytesPacketType = BitConverter.GetBytes((int)packetType);
			byte[] bytesMessage = Encoding.UTF8.GetBytes(json);
			int packetSize = bytesPacketType.Length + bytesMessage.Length;
			byte[] sizemsg = BitConverter.GetBytes(packetSize);
			"Send PacketType: {0}".With(packetType.ToString()).Dump();
			"Send Message: ({0}) {1}".With(packetSize, json).Dump();
			await socket.SendAsync(sizemsg.Concat(bytesPacketType).Concat(bytesMessage).ToArray());
		}

		public static Task<int> SendAsync(this Socket socket, byte[] buffer)
		{
			var source = new TaskCompletionSource<int>();
			socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, ar =>
			{
				try
				{
					source.SetResult(socket.EndSend(ar));
				}
				catch (Exception ex)
				{
					source.SetException(ex);
				}
			}, socket);
			return source.Task;
		}

		public static async Task<byte[]> ReceiveAsync(this Socket socket, int size)
		{
			var buffer = new byte[size];
			var length = 0;
			do
			{
				var num = await socket.ReceiveAsync(buffer, length, size);
				if (num == 0)
					break;
				length += num;
				size -= num;
			} while (size > 0);

			return buffer;
		}

		public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int off, int size)
		{
			var source = new TaskCompletionSource<int>();
			socket.BeginReceive(buffer, off, size, SocketFlags.None, ar =>
			{
				try
				{
					source.SetResult(socket.EndReceive(ar));
				}
				catch (Exception ex)
				{
					source.SetException(ex);
				}
			}, source);
			return source.Task;
		}
	}
}
