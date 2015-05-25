using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Extensions;

namespace Common
{
	public static class JsonExtensions
	{
		public static T Deserialize<T>(this string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static RoomList DeseializeRoomList(this string json)
		{
			var jsonObject = JObject.Parse(json);
			var list = jsonObject["RoomList"].Children()
				.Select(e => e.ToString().Deserialize<Room>())
				.ToList();
			var roomList = new RoomList();
			roomList.AddRange(list);
			return roomList;
		}
	}
	//public class RoomInfo
	//{
	//	public string RoomName;

	//	public JObject ToJson()
	//	{
	//		return new JObject(
	//			new JProperty("RoomName", RoomName)
	//			);
	//	}
	//}

	//public class RoomList : List<RoomInfo>
	//{
	//	public string ToJsonString()
	//	{
	//		var roomListJson = new JObject(new JProperty("RoomList", this.Select(e => e.ToJson())));
	//		return roomListJson.ToString();
	//	}
	//}
}
