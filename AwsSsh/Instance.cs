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
		public string Id { get; private set; }

		#region Properties
		private string _name;
		public string Name
		{
			get { return _name; }
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
			get { return _stateName; }
			set
			{
				if (_stateName == value) return;
				_stateName = value;
				OnPropertyChanged("StateName");
			}
		}

		private InstatnceStates _state;
		public InstatnceStates State
		{
			get { return _state; }
			set
			{
				if (_state == value) return;
				_state = value;
				OnPropertyChanged("State");
			}
		}

		private string _publicIp;
		public string PublicIp
		{
			get { return _publicIp; }
			set
			{
				if (_publicIp == value) return;
				_publicIp = value;
				OnPropertyChanged("PublicIp");
			}
		}

		private string _privateIp;
		public string PrivateIp
		{
			get { return _privateIp; }
			set
			{
				if (_privateIp == value) return;
				_privateIp = value;
				OnPropertyChanged("PrivateIp");
			}
		}
        

		private string _instanceType;
		public string InstanceType
		{
			get { return _instanceType; }
			set
			{
				if (_instanceType == value) return;
				_instanceType = value;
				OnPropertyChanged("InstanceType");
			}
		}

		private string _publicDnsName;
		public string PublicDnsName
		{
			get { return _publicDnsName; }
			set
			{
				if (_publicDnsName == value) return;
				_publicDnsName = value;
				OnPropertyChanged("PublicDnsName");
			}
		}

		private string _privateDnsName;
		public string PrivateDnsName
		{
			get { return _privateDnsName; }
			set
			{
				if (_privateDnsName == value) return;
				_privateDnsName = value;
				OnPropertyChanged("PrivateDnsName");
			}
		}
        

		#endregion

		public string Tooltip 
		{
			get
			{
				return String.Format("Instance Type: {0}\n\nAddresses:\nPublic IP: {1}\nPrivate IP: {2}\nPublic DNS: {3}\nPrivate DNS: {4}", InstanceType, PublicIp, PrivateIp, PublicDnsName, PrivateDnsName);
			}
		}

		/// <summary>
		/// Initializes a new instance of the Instance class.
		/// </summary>
		public Instance(string id)
		{
			Id = id;
		}

		/// <summary>
		/// Used to merge new instance info but retain references
		/// </summary>
		public static void AssignInstance(Instance Src, Instance New)
		{
			Src.Name = New.Name;
			Src.StateName = New.StateName;
			Src.State = New.State;
			Src.PublicIp = New.PublicIp;
			Src.PrivateIp = New.PrivateIp;
			Src.InstanceType = New.InstanceType;
			Src.PublicDnsName = New.PublicDnsName;
			Src.PrivateDnsName = New.PrivateDnsName;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public virtual void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
