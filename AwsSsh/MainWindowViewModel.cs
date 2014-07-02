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

namespace AwsSsh
{
	public class MainWindowViewModel : ObservableObject
	{
		public static MainWindowViewModel instance;

		#region Properties

		private InstanceCollection _instanceCollection;
		public InstanceCollection InstanceCollection
		{
			get { return _instanceCollection; }
		}

		private DispatcherTimer _updateTimer;

		public Settings Settings
		{
			get { return App.Settings; }
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
				if (!InstanceCollectionView.View.IsEmpty && SelectedItem == null)
					SelectedIndex = 0;
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
					_instanceCollectionView.Source = _instanceCollection.Instances;
					_instanceCollectionView.Filter += CollectionViewSource_Filter;
				}
				return _instanceCollectionView;
			}
		}

		private Instance _selectedItem;
		public Instance SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				if (_selectedItem == value) return;
				_selectedItem = value;
				OnPropertyChanged("SelectedItem");
			}
		}

		private int _selectedIndex;
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				if (_selectedIndex == value) return;
				_selectedIndex = value;
				OnPropertyChanged("SelectedIndex");
			}
		}	
		
		#endregion

		public MainWindowViewModel()
		{
			instance = this;
			_instanceCollection = new InstanceCollection();

			if (!InstanceCollectionView.View.IsEmpty)
				SelectedIndex = 0;

			_updateTimer = new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromSeconds(App.Settings.UpdateInterval) };
			_updateTimer.Tick += (obj, args) => { _instanceCollection.RefreshList(); };
		}

		private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			string name = (e.Item as Instance).Name.ToLower();
			foreach (var s in SearchText.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
				if (!name.Contains(s))
				{
					e.Accepted = false;
					return;
				}
			e.Accepted = true;
		}


		public ICommand PreferencesCommand { get { return new RelayCommand(ShowPreferences); } }
		public ICommand ExecuteCurrentCommand { get { return new RelayCommand(ConnectToCurrentServer); } }
		public ICommand StartPuttyCommand { get { return new RelayCommand(StartPutty); } }
		public ICommand RefreshListCommand { get { return new RelayCommand(DoRefreshList); } }
		public ICommand CopyCurrentCommand { get { return new RelayCommand(CopyCurrent); } }

		


		public void ShowPreferences()
		{
			new SettingsDialog().ShowDialog();
		}
		private void ConnectToCurrentServer()
		{
			if (SelectedItem != null)
				RunInstance(SelectedItem);
		}
		public void StartPutty()
		{
			Process.Start(App.Settings.PuttyPath);
		}

		public void DoRefreshList()
		{
			_updateTimer.IsEnabled = true;
			_instanceCollection.RefreshList();
		}

		public void Close()
		{
			if (!App.DontSaveSettings)
				InstanceCache.Save(InstanceCollection.Instances);
		}
		public void RunInstance(Instance instance)
		{
			var success = instance.Run();
			if (success && Settings.CloseOnConnect)
				Application.Current.Shutdown();
		}
		public void CopyCurrent()
		{
			if (SelectedItem != null && !string.IsNullOrWhiteSpace(SelectedItem.ClipboardText))
				Clipboard.SetText(SelectedItem.ClipboardText);
		}
	}
}
