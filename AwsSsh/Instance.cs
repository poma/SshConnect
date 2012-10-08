using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace AwsSsh
{
	[Serializable]
	public class Instance : INotifyPropertyChanged
	{
		private string _name;
		public string Name
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

		private string _stateName;
		public string StateName
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

		private StateColor _stateColor;
		public StateColor StateColor
		{
			get { return _stateColor; }
			set
			{
				if (_stateColor == value) return;
				_stateColor = value;
				OnPropertyChanged("StateColor");
			}
		}
		

		private string _endpoint;
		public string Endpoint
		{
			get { return _endpoint; }
			set
			{
				if (_endpoint == value) return;
				_endpoint = value;
				OnPropertyChanged("Endpoint");
			}
		}
		

		public virtual string Tooltip
		{
			get { return "Endpoint: " + Endpoint; }
		}

		public bool Run()
		{
			return true;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public virtual void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
