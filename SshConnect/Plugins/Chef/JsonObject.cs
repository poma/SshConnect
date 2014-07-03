using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SshConnect.Plugins.Chef
{
	public class JsonObject
	{
		private object Element { get; set; }

		public JsonObject(object element)
		{
			this.Element = element;
		}

		public JsonObject this[string key]
		{
			get
			{
				var data = (Hashtable)Element;
				return data.ContainsKey(key) ? new JsonObject(data[key]) : null;
			}
		}

		public JsonObject this[int key]
		{
			get
			{
				var data = (ArrayList)Element;
				return new JsonObject(data[key]);
			}
		}

		public static explicit operator string(JsonObject obj)
		{
			return (string)obj.Element;
		}

		public static explicit operator double(JsonObject obj)
		{
			return (double)obj.Element;
		}

		public static explicit operator int(JsonObject obj)
		{
			return (int)(double)obj.Element;
		}

		public JsonObject[] ToArray()
		{
			var data = (ArrayList)Element;
			return data.Cast<object>().Select(a => new JsonObject(a)).ToArray();
		}
		public Dictionary<string, JsonObject> ToDictionary()
		{
			var data = (Hashtable)Element;
			return data.Cast<DictionaryEntry>().ToDictionary(a => a.Key.ToString(), a => new JsonObject(a.Value));
		}
		public override string ToString()
		{
			return Element.ToString();
		}
	}
}
