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

		public Settings Settings { get { return App.Settings; }	}

		#region Properties

		private DispatcherTimer _updateTimer;

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
					_instanceCollectionView.Source = App.InstanceCollection.Instances;
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

			if (!InstanceCollectionView.View.IsEmpty)
				SelectedIndex = 0;

			_updateTimer = new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromSeconds(App.Settings.UpdateInterval) };
			_updateTimer.Tick += (obj, args) => { DoRefreshList(); };
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
		public ICommand ShowNextErrorCommand { get { return new RelayCommand(ShowNextError); } }

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
			App.InstanceCollection.RefreshList();
		}
		public void RunInstance(Instance instance)
		{
			var success = instance.Run();
			if (success && App.Settings.CloseOnConnect)
				Application.Current.Shutdown();
		}
		public void CopyCurrent()
		{
			if (SelectedItem != null && !string.IsNullOrWhiteSpace(SelectedItem.ClipboardText))
				Clipboard.SetText(SelectedItem.ClipboardText);
		}
		public void ShowNextError()
		{
			while (App.InstanceCollection.ErrorsPresent)
				ExceptionDialog.Show(App.InstanceCollection.GetNextException());			
		}
	}
}
