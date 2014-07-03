using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using System.Xml.Serialization;

namespace AwsSsh
{
	[Serializable]
	public class SettingsBase
	{
		private const string SettingsFile = "Settings.xml";
		private List<PropertyDescriptionInfo> _properties;

		private Guid _guid;
		public Guid Guid
		{
			get 
			{
				if (_guid == Guid.Empty)
					_guid = Guid.NewGuid();
				return _guid; 
			}
			set { _guid = value; }
		}
		

		protected SettingsBase()
		{
			_properties = GetUserProperties();
			_properties.ForEach(p => p.Property.SetValue(this, p.Attribute.DefaultValue, null));
		}

		private static Type[] GetSerializedTypes()
		{
			return Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(SettingsBase).IsAssignableFrom(t) || typeof(IInstanceSource).IsAssignableFrom(t) && !t.IsInterface).ToArray();
		}

		private List<PropertyDescriptionInfo> GetUserProperties()
		{
			return this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
									  .Select(p => new PropertyDescriptionInfo(p, p.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute))
									  .Where(p => p.Attribute != null)
									  .ToList();
		}

		public void Save()
		{
			//try
			//{
				using (TextWriter textWriter = new StreamWriter(SettingsFile))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(SettingsBase), GetSerializedTypes());
					serializer.Serialize(textWriter, this);
				}
			//}
			//catch (Exception ex)
			//{
			//	File.Delete(SettingsFile);
			//	try
			//	{
			//		do
			//		{
			//			File.WriteAllText(SettingsFile + ".error.txt", String.Format("\r\n\r\n{0}\r\n{1}", ex.Message, ex.StackTrace));
			//			ex = ex.InnerException;
			//		} while (ex != null);
			//	}
			//	catch { } // I know that this is bad
			//}
		}

		public static SettingsBase Load()
		{
			if (!File.Exists(SettingsFile))
				return null;
			//try
			//{
				using (TextReader textReader = new StreamReader(SettingsFile))
				{
					XmlSerializer deserializer = new XmlSerializer(typeof(SettingsBase), GetSerializedTypes());
					return (SettingsBase)deserializer.Deserialize(textReader);
				}
			//}
			//catch
			//{
			//	// I know that this is bad
			//	// Most probably this means that cache is corrupted
			//	return null;
			//}
		}

		public void Clear()
		{
			File.Delete(SettingsFile);
		}

		private class PropertyDescriptionInfo
		{
			public PropertyInfo Property { get; set; }
			public DefaultValueAttribute Attribute { get; set; }

			public PropertyDescriptionInfo(PropertyInfo property, DefaultValueAttribute attribute)
			{
				Property = property;
				Attribute = attribute;
			}
		}
	}
}
