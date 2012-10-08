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
using AwsSsh.ApplicationSettings;
using System.Configuration;
using AwsSsh.Plugins.Amazon;

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

		public Settings Settings
		{
			get { return App.Settings; }
		}

        private bool isLoading;
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

		private List<InstanceSource> _instanceSources;
		public List<InstanceSource> InstanceSources
		{
			get { return _instanceSources; }
			set
			{
				if (_instanceSources == value) return;
				_instanceSources = value;
				OnPropertyChanged("InstanceSources");
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
			InstanceSources = new List<InstanceSource>
			{
				new AmazonInstanceSource()
			};

			// todo: wrap exceptions
			LoadInstanceCache();
			InstanceSources.ForEach(s => s.RefreshList(Instances));
			Closing += (obj, args) => { if (!App.DontSaveSettings) SaveInstanceCache(); };

			// Just in case
			new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(200) }
				.Tick += (obj, args) => { if (!textBox.IsFocused) textBox.Focus(); };

			_updateTimer = new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromSeconds(Settings.UpdateInterval) };
			_updateTimer.Tick += (obj, args) => { RefreshList(); };
		}


		//Methods are sorted by importance
		//public void RunPuttyInstance(AnazonInstance instance)
		//{
		//	if (string.IsNullOrEmpty(instance.PublicIp)) return; // offline instancces
		//	var session = string.IsNullOrWhiteSpace(Settings.PuttySession) ? "" : String.Format("-load \"{0}\"", Settings.PuttySession);
		//	RunPutty(String.Format(@"{0} -ssh {1} -l {2} -i ""{3}"" {4}", session, instance.PublicIp, Settings.DefaultUser, Settings.KeyPath, Settings.CommandLineArgs));
		//}
		//public void RunPuttySession(string sessionName)
		//{
		//	RunPutty(String.Format("-load \"{0}\"", sessionName));
		//}
		//public void RunPutty(string command)
		//{
		//	Process.Start(Settings.PuttyPath, command);
		//	if (Settings.CloseOnConnect)
		//		Close();
		//}

		public void RunInstance(Instance instance)
		{
			var success = instance.Run();
			if (success && Settings.CloseOnConnect)
				Close();
		}

		public void RefreshList()
		{
			//BackgroundWorker w = new BackgroundWorker();
			//IsLoading = true;
			//w.DoWork += (obj, args) => args.Result = AmazonClient.GetInstances();
			//w.RunWorkerCompleted += (obj, args) =>
			//{
			//	IsLoading = false;
			//	if (args.Error != null)
			//	{
			//		_updateTimer.IsEnabled = false;
			//		throw new Exception("Error downloading server list: " + args.Error.Message, args.Error);
			//	}
			//	var newInstances = args.Result as List<AnazonInstance>;
			//	var previousSelection = listBox.SelectedItem as AnazonInstance;
			//	using (InstanceCollectionView.DeferRefresh())
			//		AmazonClient.MergeInstanceList(Instances, newInstances);
			//	if (listBox.Items.Count > 0)
			//		if (previousSelection != null && listBox.Items.Contains(previousSelection))
			//			listBox.SelectedItem = previousSelection;
			//		else
			//			listBox.SelectedIndex = 0;
			//};
			//w.RunWorkerAsync();			
		}

		private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			e.Accepted = (e.Item as Instance).Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
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
					XmlSerializer deserializer = new XmlSerializer(typeof(List<Instance>), InstanceSources.SelectMany(s => s.SerializedTypes).ToArray());
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
					XmlSerializer serializer = new XmlSerializer(typeof(List<Instance>), InstanceSources.SelectMany(s => s.SerializedTypes).ToArray());
					serializer.Serialize(textWriter, Instances);
					textWriter.Close();
				}
			}
			catch (Exception ex)
			{
				try
				{
					do
					{
						File.AppendAllText(CacheFile, String.Format("\r\n\r\n{0}\r\n{1}", ex.Message, ex.StackTrace));
						ex = ex.InnerException;
					} while (ex != null);
				}
				catch { } // I know that this is bad
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
			RunInstance(item);
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