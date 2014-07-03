using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace AwsSsh
{
	public static class InstanceCache
	{
		public static readonly string CacheFile = "Cache.xml";

		private static Type[] GetSerializedTypes()
		{
			return Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Instance).IsAssignableFrom(t)).ToArray();
		}
		public static void Save(IEnumerable<Instance> list)
		{
			try
			{
				File.Delete(CacheFile + ".error.txt");
				using (TextWriter textWriter = new StreamWriter(CacheFile))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Instance>), GetSerializedTypes());
					serializer.Serialize(textWriter, list.ToList());
				}
			}
			catch (Exception ex)
			{
				File.Delete(CacheFile);
				try
				{
					do
					{
						File.WriteAllText(CacheFile + ".error.txt", String.Format("\r\n\r\n{0}\r\n{1}", ex.Message, ex.StackTrace));
						ex = ex.InnerException;
					} while (ex != null);
				}
				catch { } // I know that this is bad
			}
		}
		public static List<Instance> Load()
		{
			if (!File.Exists(CacheFile)) 
				return null;
			try
			{
				using (TextReader textReader = new StreamReader(CacheFile))
				{
					XmlSerializer deserializer = new XmlSerializer(typeof(List<Instance>), GetSerializedTypes());
					var list = (List<Instance>)deserializer.Deserialize(textReader);
					list = list.Where(a => a.Source != null).ToList();
					return list;
				}
			}
			catch (Exception e)
			{
				// I know that this is bad
				// Most probably this means that cache is corrupted
				return null;
			} 
		}
		public static void Clear()
		{
			File.Delete(InstanceCache.CacheFile);
		}
	}
}
