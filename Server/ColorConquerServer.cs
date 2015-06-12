using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Net;
using Common;
using Alchemy;
using Alchemy.Classes;
using System.Collections.Concurrent;

namespace Server
{
	public class ColorConquerServer
	{
		private WebSocketServer _server;

		private ConcurrentDictionary<UserContext, User> _userDic = new ConcurrentDictionary<UserContext, User>();

		public void StartWebSocket(int port)
		{
			_server = new WebSocketServer(port, IPAddress.Any)
			{
				OnReceive = OnReceive,
				OnSend = OnSend,
				OnConnect = OnConnect,
				OnConnected = OnConnected,
				OnDisconnect = OnDisconnect,
				TimeOut = new TimeSpan(0, 5, 0)
			};
			_server.Start();
			Logger.Log("Server started.");
		}

		public void StopWebSocket()
		{
			_server.Stop();
			Logger.Log("Server stoped.");
		}

		void OnConnect(UserContext context)
		{
			"Connect: {0}".With(context.ClientAddress.ToString()).Dump();
		}

		void OnConnected(UserContext context)
		{
			"Connected: {0}".With(context.ClientAddress.ToString()).Dump();
			_userDic.TryAdd(context, new User(context));
			//context.Send("Connected");
		}

		void OnDisconnect(UserContext context)
		{
			"DisConnect: {0}".With(context.ClientAddress.ToString()).Dump();
			if (!_userDic.ContainsKey(context))
				return;
			User user = _userDic[context];
			user.ProcessPacket(PacketType.Shutdown);
			_userDic.TryRemove(context, out user);
		}

		void OnSend(UserContext context)
		{
			"Send: {0}".With(context.ClientAddress.ToString()).Dump();
		}

		void OnReceive(UserContext context)
		{
			if (!_userDic.ContainsKey(context))
				return;
			User user = _userDic[context];
			user.ProcessPacket(context.DataFrame.ToString());
			//"Receive: {0}, {1}".With(context.ClientAddress.ToString(), context.DataFrame.ToString()).Dump();
		}

		public void Start(int port)
		{
			var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				listener.Bind(new IPEndPoint(IPAddress.Any, port));
				listener.Listen(100);

				Logger.Log("ColorConquerServer Started (port: {0})".With(port));

				Utils.RsaGeneratePrivateKey(1024);

				while (true)
				{
					"Before Accept".Dump();
					var clientSocket = listener.Accept();
					clientSocket.SendAsync(PacketType.RsaPublicKey, Utils.RsaGetPublicKey(Utils.RsaPrivateKeyXmlString), false);
					clientSocket.ReceiveLoop(PacketProcessor.ProcessPacket);
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
			finally
			{
				listener.Close();
			}
		}
	}
}
