using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Host.HttpListener;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using Extensions;

namespace ColorConquerServer
{
	public class ColorConquerServer
	{
		static void Main(string[] args)
		{
			int port = 55591;
#if DEBUG
			string url = "http://localhost:{0}".With(port);
			Logger.Log(url);
#else
			string url = "http://hellojkw.com:{0}".With(port);
#endif
			using (WebApp.Start(url))
			{
				Console.WriteLine("Server started.");
				Console.ReadLine();
			}
		}
	}

	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseCors(CorsOptions.AllowAll);
			app.MapSignalR("/colorconquer", new HubConfiguration());
		}
	}

	public class ColorConquerHub : Hub
	{
		private static ConcurrentDictionary<string /* ConnectionId */, User> _userDic = new ConcurrentDictionary<string, User>();

		public ColorConquerHub()
		{
			UserStatic.colorConquerHub = this;
		}
		
		public override Task OnConnected()
		{
			"Connected".Dump();
			Context.User.Identity.Name.Dump("UserName:");
			_userDic.TryAdd(Context.ConnectionId, new User(Context));
			return base.OnConnected();
		}

		public override Task OnReconnected()
		{
			"Reconnected".Dump();
			if (!_userDic.ContainsKey(Context.ConnectionId))
			{
				_userDic.TryAdd(Context.ConnectionId, new User(Context));
			}
			return base.OnReconnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			"Disconnected".Dump();
			if (_userDic.ContainsKey(Context.ConnectionId))
			{
				User user = _userDic[Context.ConnectionId];
				user.ProcessPacket(PacketType.Shutdown);
				_userDic.TryRemove(Context.ConnectionId, out user);
			}
			return base.OnDisconnected(stopCalled);
		}

		public void ClientToServer(string message)
		{
			if (!_userDic.ContainsKey(Context.ConnectionId))
				return;
			User user = _userDic[Context.ConnectionId];
			user.ProcessPacket(message);
		}

		public void Send(string connectionId, string message)
		{
			Clients.Client(connectionId).ServerToClient(message);
			//Clients.All.ServerToClient(connectionId, message);
		}
	}
}
