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
using System.Threading;
using System.Reflection;
using MvvmFoundation.Wpf;
using AwsSsh.Plugins.Putty;

namespace AwsSsh
{
	public class InstanceCollection: ObservableObject
	{
		public static readonly string CacheFile = Path.Combine(Path.GetTempPath(), "AwsSsh.cache.xml");

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

		public InstanceCollection()
		{
			InstanceSources = new List<InstanceSource>
			{
				new AmazonInstanceSource(),
				new PuttyInstanceSource()
			};

			// todo: wrap exceptions
			LoadInstanceCache();
			RefreshList();
		}


		public void RefreshList()
		{
			foreach (var src in InstanceSources)
			{
				
				BackgroundWorker w = new BackgroundWorker();
				//IsLoading = true;
				w.DoWork += (obj, args) => args.Result = new Tuple<InstanceSource, List<Instance>>(args.Argument as InstanceSource, (args.Argument as InstanceSource).GetInstanceList()) ;
				w.RunWorkerCompleted += (obj, args) =>
				{
					//IsLoading = false;
					if (args.Error != null)
					{
						//_updateTimer.IsEnabled = false;
						throw new Exception("Error downloading server list: " + args.Error.Message, args.Error);
					}
					var previousSelection = MainWindow.instance.listBox.SelectedItem as Instance;

					var res = args.Result as Tuple<InstanceSource, List<Instance>>;
					var newInstances = res.Item2;
					using (MainWindowViewModel.instance.InstanceCollectionView.DeferRefresh())
						res.Item1.MergeInstanceList(Instances, newInstances);

					if (MainWindow.instance.listBox.Items.Count > 0)
					{
						if (previousSelection != null)
						{
							int ind = MainWindow.instance.listBox.Items.OfType<Instance>().Select(i => i.Name).ToList().IndexOf(previousSelection.Name);
							if (ind < 0) ind = 0;
							MainWindow.instance.listBox.SelectedIndex = ind;
						}
						else
							MainWindow.instance.listBox.SelectedIndex = 0;
					}

					//var previousSelection = listBox.SelectedItem as AmazonInstance;
					//using (InstanceCollectionView.DeferRefresh())
					//	AmazonClient.MergeInstanceList(Instances, newInstances);
					//if (listBox.Items.Count > 0)
					//	if (previousSelection != null && listBox.Items.Contains(previousSelection))
					//		listBox.SelectedItem = previousSelection;
					//	else
					//		listBox.SelectedIndex = 0;
				};
				w.RunWorkerAsync(src);
			}
		}

		public static Type[] GetSerializedTypes()
		{
			return Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Instance).IsAssignableFrom(t)).ToArray();			
		}

		public void LoadInstanceCache()
		{
			if (!File.Exists(CacheFile)) return;
			try
			{
				using (TextReader textReader = new StreamReader(CacheFile))
				{
					XmlSerializer deserializer = new XmlSerializer(typeof(List<Instance>), GetSerializedTypes());
					var instances = (List<Instance>)deserializer.Deserialize(textReader);
					textReader.Close();
					instances.ForEach(a => Instances.Add(a));
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
					XmlSerializer serializer = new XmlSerializer(typeof(List<Instance>), GetSerializedTypes());
					serializer.Serialize(textWriter, Instances.ToList());
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
	}
}
