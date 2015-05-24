﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Common;

namespace Server
{
	public static class SocketExtensions
	{
		public static async void ReceiveLoop(this Socket socket)
		{
			while (true)
			{
				var length = (await socket.ReceiveAsync(sizeof(int))).ConvertToInt32();
				length.Dump();
				if (length == 0)
				{
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
					break;
				}
				var packetBytes = await socket.ReceiveAsync(length);

				var user = socket.GetUser();
				var packetType = (PacketType)packetBytes.Take(sizeof(int)).ToArray().ConvertToInt32();
			}
		}

		public static async void SendAsync(this Socket socket, string message)
		{
			byte[] msg = Encoding.UTF8.GetBytes(message);
			byte[] sizemsg = BitConverter.GetBytes(msg.Length);

			await socket.SendAsync(sizemsg.Concat(msg).ToArray());

			//return true;
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
