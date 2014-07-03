using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using System.Xml.Serialization;

namespace SshConnect
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
		

		public SettingsBase()
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
			try
			{
				using (TextWriter textWriter = new StreamWriter(SettingsFile))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(SettingsBase), GetSerializedTypes());
					serializer.Serialize(textWriter, this);
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Error writing settings", ex);
			}
		}

		public static SettingsBase Load()
		{
			if (!File.Exists(SettingsFile))
				return null;
			try
			{
				using (TextReader textReader = new StreamReader(SettingsFile))
				{
					XmlSerializer deserializer = new XmlSerializer(typeof(SettingsBase), GetSerializedTypes());
					return (SettingsBase)deserializer.Deserialize(textReader);
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Error reading settings", ex);
			}
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
