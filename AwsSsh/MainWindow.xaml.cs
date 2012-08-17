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

namespace AwsSsh
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Properties

		public const string CacheFile = "cache.xml";
		private DispatcherTimer _updateTimer;

		private Settings Settings
		{
			get { return Settings.Default; }
		}

        public bool isLoadComplete;
		public bool IsLoadComplete
		{
			get { return isLoadComplete; }
			set
			{
				if (isLoadComplete == value)
					return;
				isLoadComplete = value;
				PropertyChanged(this, new PropertyChangedEventArgs("IsLoadComplete"));
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
			if (!CheckConfig())
			{
				Application.Current.Shutdown();
				return;
			}
			InitializeComponent();
			LoadInstanceCache();
			RefreshList();
			Closing += (obj, args) => SaveInstanceCache();

			// Just in case
			new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(200) }
				.Tick += (obj, args) => { if (!textBox.IsFocused) textBox.Focus(); };

			_updateTimer = new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromSeconds(Settings.UpdateInterval) };
			_updateTimer.Tick += (obj, args) => { RefreshList(); }; // This stupid hack is used to prevent update timer from spamming errors
		}

		//Methods are sorted by importance
		public void RunPutty(string ip)
		{
			if (string.IsNullOrEmpty(ip)) return; // offline instancces
			string command = String.Format(@"-ssh {0} -l {1} -i ""{2}"" {3}", ip, Settings.DefaultUser, Settings.KeyPath, Settings.CommandLineArgs);
			Process.Start(Settings.PuttyPath, command);
			if (Settings.CloseOnConnect)
				Close();
		}

		public void RefreshList()
		{
			BackgroundWorker w = new BackgroundWorker();
			IsLoadComplete = false;
			w.DoWork += (obj, args) => args.Result = AmazonClient.GetInstances();
			w.RunWorkerCompleted += (obj, args) =>
			{
				IsLoadComplete = true;
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
						listBox.SelectedIndex = 0;
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.PageDown:
					if (listBox.Items.Count > 0)
					{
						listBox.SelectedIndex = listBox.Items.Count - 1;
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
					serializer.Serialize(textWriter, Instances.ToList());
					textWriter.Close();
				}
			}
			catch { } // I know that this is bad
		}
		private bool CheckConfig()
		{
			if (!File.Exists(Settings.PuttyPath))
			{
				MessageBox.Show("Putty not found. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			if (!File.Exists(Settings.KeyPath))
			{
				MessageBox.Show("Key file not found. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			if (string.IsNullOrWhiteSpace(Settings.AWSAccessKey) || string.IsNullOrWhiteSpace(Settings.AWSAccessKey))
			{
				MessageBox.Show("Amazon security credentials are empty. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			return true;
		}
		private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ConnectToCurrentServer();
		}
		private void ConnectToCurrentServer()
		{
			var item = listBox.SelectedItem as Instance;
			if (item == null) return;
			RunPutty(item.PublicDnsName);
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