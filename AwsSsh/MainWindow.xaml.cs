using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using AwsSsh.Properties;
using System.Configuration;

namespace AwsSsh
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Properties

		public static readonly string CacheFile = Path.Combine(Path.GetTempPath(), "AwsSsh.cache.xml");
		public const int BigStep = 20;

		private DispatcherTimer _updateTimer;

		public List<string> PuttySessions { get; private set; }

		internal Settings Settings
		{
			get { return Settings.Default; }
		}

		/// <summary>
		/// Used for databinding
		/// </summary>
		public ApplicationSettingsBase SettingsBase
		{
			get { return Settings.Default; }
		}

        public bool isLoading;
		public bool IsLoading
		{
			get { return isLoading; }
			set
			{
				if (isLoading == value) return;
				isLoading = value;
				OnPropertyChanged("IsLoading");
			}
		}

		public ObservableCollection<Instance> _instances;
		public ObservableCollection<Instance> Instances
		{
			get
			{
				if (_instances == null)
					_instances = new ObservableCollection<Instance>();
				return _instances;
			}
		}

		private string _searchText;
		public string SearchText
		{
			get { return _searchText ?? ""; }
			set
			{
				if (_searchText == value) return;
				_searchText = value;
				InstanceCollectionView.View.Refresh();
				if (listBox.Items.Count > 0 && listBox.SelectedItem == null)
					listBox.SelectedIndex = 0;
				OnPropertyChanged("SearchText");
			}
		}

		private CollectionViewSource _instanceCollectionView;
		public CollectionViewSource InstanceCollectionView
		{
			get
			{
				if (_instanceCollectionView == null)
				{
					_instanceCollectionView = new CollectionViewSource();
					_instanceCollectionView.Source = Instances;
					_instanceCollectionView.Filter += CollectionViewSource_Filter;
				}
				return _instanceCollectionView;
			}
		}

		#endregion

		public MainWindow()
		{
			InitializeComponent();
			LoadInstanceCache();
			GetPuttySessions();
			RefreshList();
			Closing += (obj, args) => { if (!App.DontSaveSettings) SaveInstanceCache(); };

			// Just in case
			new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(200) }
				.Tick += (obj, args) => { if (!textBox.IsFocused) textBox.Focus(); };

			_updateTimer = new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromSeconds(Settings.UpdateInterval) };
			_updateTimer.Tick += (obj, args) => { RefreshList(); };

			Settings.PropertyChanged += Settings_PropertyChanged;
		}

		void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IncludePuttySessionsInList")
			{
				Instances.Where(a => a.IsPuttyInstance).ToList().ForEach(a => Instances.Remove(a));
				if (Settings.IncludePuttySessionsInList)
					GetPuttySessions();
			}
		}

		//Methods are sorted by importance
		public void RunPuttyInstance(Instance instance)
		{
			if (string.IsNullOrEmpty(instance.PublicDnsName)) return; // offline instancces
			RunPutty(String.Format(@"-ssh {0} -l {1} -i ""{2}"" {3}", instance.PublicDnsName, Settings.DefaultUser, Settings.KeyPath, Settings.CommandLineArgs));
		}
		public void RunPuttySession(string sessionName)
		{
			RunPutty(String.Format("-load \"{0}\"", sessionName));
		}
		public void RunPutty(string command)
		{
			Process.Start(Settings.PuttyPath, command);
			if (Settings.CloseOnConnect)
				Close();
		}

		public void RefreshList()
		{
			BackgroundWorker w = new BackgroundWorker();
			IsLoading = true;
			w.DoWork += (obj, args) => args.Result = AmazonClient.GetInstances();
			w.RunWorkerCompleted += (obj, args) =>
			{
				IsLoading = false;
				if (args.Error != null)
				{
					_updateTimer.IsEnabled = false;
					throw new Exception("Error downloading server list: " + args.Error.Message, args.Error);
				}
				var newInstances = args.Result as List<Instance>;
				var previousSelection = listBox.SelectedItem as Instance;
				using (InstanceCollectionView.DeferRefresh())
					AmazonClient.MergeInstanceList(Instances, newInstances);
				if (listBox.Items.Count > 0)
					if (previousSelection != null && listBox.Items.Contains(previousSelection))
						listBox.SelectedItem = previousSelection;
					else
						listBox.SelectedIndex = 0;
			};
			w.RunWorkerAsync();			
		}

		private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			e.Accepted = (e.Item as Instance).Name.Contains(SearchText);
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			switch (e.Key)
			{
				case Key.Escape:
					Close();
					break;
				case Key.Enter: 
					ConnectToCurrentServer(); 
					break;
				case Key.F5:
					RefreshList();
					break;
				case Key.Down:
					if (listBox.SelectedIndex < listBox.Items.Count)
					{
						listBox.SelectedIndex++;
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.Up:
					if (listBox.SelectedIndex > 0)
					{
						listBox.SelectedIndex--;
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.PageUp:
					if (listBox.Items.Count > 0)
					{
						listBox.SelectedIndex = Math.Max(0, listBox.SelectedIndex - BigStep);
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.PageDown:
					if (listBox.Items.Count > 0)
					{
						listBox.SelectedIndex = Math.Min(listBox.Items.Count - 1, listBox.SelectedIndex + BigStep);
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;

				default:
					// This will just send keystroke to textbox
					e.Handled = false;
					textBox.Focus();
					break;
			}			
		}

		public void LoadInstanceCache()
		{
			if (!File.Exists(CacheFile)) return;
			try
			{
				using (TextReader textReader = new StreamReader(CacheFile))
				{
					XmlSerializer deserializer = new XmlSerializer(typeof(List<Instance>));
					var instances = (List<Instance>)deserializer.Deserialize(textReader);
					textReader.Close();
					instances.ForEach(a => Instances.Add(a));
					if (listBox.Items.Count > 0)
						listBox.SelectedIndex = 0;
				}
			}
			catch { } // I know that this is bad
		}
		public void SaveInstanceCache()
		{
			try
			{
				using (TextWriter textWriter = new StreamWriter(CacheFile))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Instance>));
					serializer.Serialize(textWriter, Instances.Where(a => !a.IsPuttyInstance).ToList());
					textWriter.Close();
				}
			}
			catch { } // I know that this is bad
		}

		public void GetPuttySessions()
		{
			PuttySessions = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions").GetSubKeyNames().ToList();
			if (!Settings.IncludePuttySessionsInList) return;
			foreach (var s in PuttySessions)
			{
				var inst = new Instance
				{
					IsPuttyInstance = true,
					State = InstatnceStates.Unknown,
					StateName = "Unknown",
					Name = s,
					Id = s
				};
				Instances.Add(inst);
			}
		}
        private void Preferences_MouseDown(object sender, MouseButtonEventArgs e)
		{
			new SettingsDialog().ShowDialog();
		}
		private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ConnectToCurrentServer();
		}
		private void ConnectToCurrentServer()
		{
			var item = listBox.SelectedItem as Instance;
			if (item == null) return;
			if (item.IsPuttyInstance)
				RunPuttySession(item.Id);
			else
				RunPuttyInstance(item);
		}
		private void Putty_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Process.Start(Settings.PuttyPath);
		}

		private void RefreshButton_MouseDown(object sender, MouseButtonEventArgs e)
		{
			_updateTimer.IsEnabled = true;
			RefreshList();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public virtual void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}