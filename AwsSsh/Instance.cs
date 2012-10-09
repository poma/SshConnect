using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Windows;

namespace AwsSsh
{
	[Serializable]
	public class Instance : INotifyPropertyChanged
	{
		private string _name;
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

		private string _stateName = "Unknown";
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
