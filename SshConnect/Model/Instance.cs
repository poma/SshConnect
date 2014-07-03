using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Windows;
using System.Xml.Serialization;

namespace SshConnect
{
	[Serializable]
	[DebuggerDisplay("Name = {_name}")]
	public class Instance : INotifyPropertyChanged
	{
		private IInstanceSource _source;	
		[XmlIgnore]
		public IInstanceSource Source
		{
			get { return _source; }
			set { _source = value; }
		}

		public Guid SourceGuid
		{
			get
			{
				return Source.Settings.Guid;
			}
			set
			{
				var src = App.InstanceCollection.InstanceSources.Where(s => s.Settings.Guid == value).FirstOrDefault();
				if (src != null)
					Source = src;
				else
					throw new ApplicationException("Corresponding source not found");
			}
		}

		private string _name;
		[CopyProperty]
		public virtual string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name == value) return;
				_name = value;
				OnPropertyChanged("Name");
			}
		}

		public virtual string GetId()
		{
			return Name;
		}

		private string _stateName = "Unknown";
		[CopyProperty]
		public virtual string StateName
		{
			get
			{
				return _stateName;
			}
			set
			{
				if (_stateName == value) return;
				_stateName = value;
				OnPropertyChanged("StateName");
			}
		}

		private StateColor _stateColor = StateColor.Gray;
		[CopyProperty]
		public virtual StateColor StateColor
		{
			get { return _stateColor; }
			set
			{
				if (_stateColor == value) return;
				_stateColor = value;
				OnPropertyChanged("StateColor");
			}
		}

		public virtual string Tooltip
		{
			get { return null; }
		}

		public virtual string ClipboardText
		{
			get { return Name; }
		}

		public virtual bool Run()
		{
			return true;
		}

		protected void RunPutty(string args)
		{
			Process.Start(App.Settings.PuttyPath, args);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
