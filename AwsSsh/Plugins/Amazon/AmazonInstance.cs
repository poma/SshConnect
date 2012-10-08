using System;
using System.Linq;
using System.Collections.Generic;

namespace AwsSsh
{
	[Serializable]
	public class AmazonInstance : Instance
	{
		public string Id { get; set; }
		public bool IsPuttyInstance { get; set; }

		#region Properties

		private InstatnceStates _state;
		public InstatnceStates State
		{
			get { return _state; }
			set
			{
				if (_state == value) return;
				_state = value;
				UpdateStateColor();
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
				Endpoint = value;
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

		public override string Tooltip
		{
			get
			{
				if (IsPuttyInstance)
					return "Putty saved session";
				else
					return String.Format("Instance Type: {0}\n\nAddresses:\nPublic IP: {1}\nPrivate IP: {2}\nPublic DNS: {3}\nPrivate DNS: {4}", InstanceType, PublicIp, PrivateIp, PublicDnsName, PrivateDnsName);
			}
		}

		/// <summary>
		/// Used to merge new instance info but retain references
		/// </summary>
		public static void AssignInstance(AmazonInstance Src, AmazonInstance New)
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

		private void UpdateStateColor()
		{
			switch (State)
			{
				case InstatnceStates.Unknown: StateColor = AwsSsh.StateColor.Gray; break;
				case InstatnceStates.Pending: StateColor = AwsSsh.StateColor.Yellow; break;
				case InstatnceStates.Running: StateColor = AwsSsh.StateColor.Green; break;
				case InstatnceStates.ShuttingDown: StateColor = AwsSsh.StateColor.Yellow; break;
				case InstatnceStates.Terminated: StateColor = AwsSsh.StateColor.Red; break;
				case InstatnceStates.Stopping: StateColor = AwsSsh.StateColor.Red; break;
				case InstatnceStates.Stopped: StateColor = AwsSsh.StateColor.Red; break;
				default: throw new InvalidOperationException("Unknown state");
			}
		}
	}
}
