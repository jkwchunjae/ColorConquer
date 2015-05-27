using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Extensions;
using System.Dynamic;

namespace Common
{
	public static class JsonExtensions
	{
		public static T JsonDeserialize<T>(this string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static dynamic JsonDeserialize(this string json)
		{
			return JsonConvert.DeserializeObject(json);
		}

		public static string JsonSerialize(this object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}
	}
}
