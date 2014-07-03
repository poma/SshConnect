using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net.Sockets;
using System.Collections;
using System.Xml;

namespace SshConnect
{
	public static class XmlUtils
	{
		public static void MergeWith<T>(this T primary, T secondary)
		{
			foreach (var pi in typeof(T).GetProperties())
			{
				var priValue = pi.GetGetMethod().Invoke(primary, null);
				var secValue = pi.GetGetMethod().Invoke(secondary, null);
				if (priValue == null || (pi.PropertyType.IsValueType && priValue == Activator.CreateInstance(pi.PropertyType)))
				{
					pi.GetSetMethod().Invoke(primary, new object[] { secValue });
				}
			}
		}


		
		/// <summary>
		/// Concatenates all inner exception messages separated by colon
		/// </summary>
		public static string InnerMessages(this Exception exception)
		{
			string m = "";
			if (exception.Message != null) m += exception.Message;
			if (exception.InnerException != null) m += ": " + exception.InnerException.InnerMessages();
			return m;
		}

		/// <summary>
		/// [Deprecated] Removes all invalid(for xml) symbols from string
		/// </summary>
		public static string ValidateForXml(this string s)
		{
			char[] c = s.ToCharArray();
			for (int i = 0; i < c.Length; i++)
				if (!(c[i] == 0x9 || c[i] == 0xA || c[i] == 0xD ||
					(c[i] >= 0x20 && c[i] <= 0xD7FF) || (c[i] >= 0xE000 && c[i] <= 0xFFFD)))
					c[i] = '_';
			return new string(c);
		}

		public static string XmlEncode(this string s)
		{
			//return System.Security.SecurityElement.Escape(s);
			XmlDocument doc = new XmlDocument();
			var node = doc.CreateElement("root");
			node.InnerText = s;
			return node.InnerXml;
		}

		/// <summary>
		/// Get XML representation of exception data including all inner exceptions.
		/// Includes Message, StackTrace, Data, SocketException's error code.
		/// </summary>
		public static XElement GetXml(this Exception exception)
		{
			if (exception == null) return new XElement("Exception_is_null!");

			Func<string, string> enc = XmlEncode;
			Func<string, IEnumerable<XElement>> ProcessStackTrace = s => from frame in s.Split('\n')
																		 let prettierFrame = enc(frame.Trim())
																		 select new XElement("Frame", prettierFrame);

			XElement root = new XElement(enc(exception.GetType().ToString().Replace(' ', '_')));

			if (exception.Message != null) root.Add(new XElement("Message", enc(exception.Message)));

			if (exception.StackTrace != null)
			{
				root.Add(new XElement("StackTrace", ProcessStackTrace(exception.StackTrace)));
			}

			if (exception.Data.Contains("ThrowPath"))
			{
				root.Add(new XElement("ThrowPath", ProcessStackTrace(exception.Data["ThrowPath"].ToString())));
			}

			if (exception.Data.Count > 0)
			{
				root.Add
				(
					new XElement("Data",
						from entry in exception.Data.Cast<DictionaryEntry>()
						where entry.Key.ToString() != "ThrowPath"
						let key = enc(entry.Key.ToString().Replace(' ', '_'))
						let value = (entry.Value == null) ? "null" : enc(entry.Value.ToString())
						select new XElement(key, value))
				);
			}

			if (exception is SocketException)
				root.Add(new XElement("ErrorCode", (exception as SocketException).ErrorCode));

			if (exception.InnerException != null)
				root.Add(GetXml(exception.InnerException));

			return root;
		}
	}
}
