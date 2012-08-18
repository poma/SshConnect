using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Win32;

namespace AwsSsh.ApplicationSettings
{
	public class SettingsBase
	{
		Type _type;
		List<PropertyDescriptionInfo> _properties;

		private static RegistryKey _registryKey;
		private static RegistryKey RegistryKey
		{
			get
			{
				if (_registryKey == null)
					_registryKey = Registry.CurrentUser.CreateSubKey(@"Software\AwsSsh\");
				return _registryKey;
			}
		}

		protected SettingsBase(Type _Type)
		{
			_type = _Type;
			_properties = GetUserProperties();
		}

		private List<PropertyDescriptionInfo> GetUserProperties()
		{
			return _type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
									  .Select(p => new PropertyDescriptionInfo(p, p.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute))
									  .Where(p => p.Attribute != null)
									  .ToList();
		}

		public void Save()
		{
			foreach (var p in _properties)
			{
				object val = p.Property.GetValue(this, null);
				if (val != null)
					RegistryKey.SetValue(p.Property.Name, val, RegistryValueKind.String);
			}
		}

		public void Load()
		{
			foreach (var p in _properties)
			{
				object val = RegistryKey.GetValue(p.Property.Name, p.Attribute.DefaultValue);
				if (val != null)
				{
					try
					{
						if (p.Property.PropertyType == typeof(string)) val = val.ToString();
						if (p.Property.PropertyType == typeof(int)) val = int.Parse(val.ToString());
						if (p.Property.PropertyType == typeof(bool)) val = bool.Parse(val.ToString());
						if (typeof(Enum).IsAssignableFrom(p.Property.PropertyType)) val = Enum.Parse(p.Property.PropertyType, val.ToString());
					}
					catch { }
					if (val != null)
						p.Property.SetValue(this, val, null);
				}
			}
		}

		public void Clear()
		{
			Registry.CurrentUser.DeleteSubKeyTree(@"Software\AwsSsh", false);
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
