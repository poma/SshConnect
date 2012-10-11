using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace AwsSsh.ApplicationSettings
{
	public class Settings : SettingsBase, INotifyPropertyChanged
	{
		[DefaultValue("Put your access key here")]
		public string AWSAccessKey { get; set; }

		[DefaultValue("Put your secret key here")]
		public string AWSSecretKey { get; set; }
		
		[DefaultValue("C:\\certificate.ppk")]
		public string KeyPath { get; set; }

		[DefaultValue("C:\\putty.exe")]
		public string PuttyPath { get; set; }

		[DefaultValue(@"V:\Development\chef.pub")]
		public string ChefPublicKey { get; set; }

		[DefaultValue(@"V:\Development\chef.pem")]
		public string ChefPrivateKey { get; set; }

		[DefaultValue("")]
		public string PuttySession { get; set; }

		[DefaultValue("")]
		public string CommandLineArgs { get; set; }

		[DefaultValue("https://us-east-1.ec2.amazonaws.com")]
		public string ServiceUrl { get; set; }

		[DefaultValue(true)]
		public bool CloseOnConnect { get; set; }

		[DefaultValue("ubuntu")]
		public string DefaultUser { get; set; }

		[DefaultValue(10)]
		public int UpdateInterval { get; set; }

		[DefaultValue(true)]
		public bool IsFirstTimeConfiguration { get; set; }

		[DefaultValue(false)]
		public bool ShowPuttyButton
		{
			get
			{
				return showPuttyButton;
			}
			set
			{
				if (showPuttyButton == value) return;
				showPuttyButton = value;
				OnPropertyChanged("ShowPuttyButton");
			}
		}

		private bool showPuttyButton;
        private bool includePuttySessionsInList;
		[DefaultValue(true)]
		public bool IncludePuttySessionsInList
		{
			get
			{
				return includePuttySessionsInList;
			}
			set
			{
				if (includePuttySessionsInList == value) return;
				includePuttySessionsInList = value;
				OnPropertyChanged("IncludePuttySessionsInList");
			}
		}


		public Settings() : base(typeof(Settings)) { }

		public event PropertyChangedEventHandler PropertyChanged;
		public virtual void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
