﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using SshConnect.Plugins.Amazon;
using System.Windows.Controls.Primitives;
using SshConnect.Plugins.Chef;
using SshConnect.Plugins.Putty;

namespace SshConnect
{
	/// <summary>
	/// Interaction logic for SettingsDialog.xaml
	/// </summary>
	public partial class SettingsDialog : Window
	{
		public SettingsDialog()
		{
			InitializeComponent();
			DataContext = App.Settings;
			foreach (var source in App.InstanceCollection.InstanceSources)
			{
				if (!(source is PuttyInstanceSource))
				{
					source.SettingsControl.Background = new SolidColorBrush(Colors.Transparent);
					tabControl.Items.Add(new TabItem { Header = source.Name, Content = source.SettingsControl, Tag = source, DataContext = source.Settings });
				}
			}
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Cursor = Cursors.Wait;
				if (!CheckConfig()) return;
				DialogResult = true;
			}
			finally
			{
				Cursor = Cursors.Arrow;
			}
		}

		private void LinkClick(object sender, RoutedEventArgs e)
		{
			string link = ((sender as FrameworkElement).Tag ?? "").ToString();
			if (!string.IsNullOrEmpty(link))
				Process.Start(link);
		}

		private void StartPutty_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(App.Settings.PuttyPath);
		}


		private void ClearSettings_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("All your settings and cache will be cleared and application will shutdown", "Clear data", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
			{
				App.ClearSettingsAndExit();
			}
		}

		private void ClearList_Click(object sender, RoutedEventArgs e)
		{
			App.InstanceCollection.Instances.Clear();
		}

		private void ShowShareMenu(object sender, MouseButtonEventArgs e)
		{
			shareMenu.PlacementTarget = shareButton;
			shareMenu.IsOpen = true;
		}

		private void ShowButtonMenu(object sender, RoutedEventArgs e)
		{
			var b = sender as Button;
			if (b == null || b.ContextMenu == null)
				return;
			b.ContextMenu.PlacementTarget = b;
			b.ContextMenu.IsOpen = true;
		}

		private void CreateSource(object sender, RoutedEventArgs e)
		{
			var type = (sender as MenuItem).Tag as string;
			IInstanceSource source;
			switch (type)
			{
				case "Amazon":
					source = new AmazonInstanceSource();
					break;
				case "Chef":
					source = new ChefInstanceSource();
					break;
				default:
					return;
			}

			App.Settings.InstanceSources.Add(source);
			App.InstanceCollection.InstanceSources.Add(source);
			source.SettingsControl.Background = new SolidColorBrush(Colors.Transparent);
			var tab = new TabItem { Header = source.Name, Content = source.SettingsControl, Tag = source, DataContext = source.Settings };
			tabControl.Items.Add(tab);
			tabControl.SelectedItem = tab;
			if (MainWindowViewModel.instance != null)
				MainWindowViewModel.instance.DoRefreshList();
		}

		private void DeleteSourceClick(object sender, RoutedEventArgs e)
		{
			var source = (tabControl.SelectedItem as TabItem).Tag as IInstanceSource;
			if (source == null)
				return;
			if (MessageBox.Show("Are you sure you want to delete this source", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				App.Settings.InstanceSources.Remove(source);
				App.InstanceCollection.InstanceSources.Remove(source);
				tabControl.Items.Remove(tabControl.SelectedItem);
				App.InstanceCollection.Instances.Where(a => a.Source == source).ToList().ForEach(a => App.InstanceCollection.Instances.Remove(a));
			}
		}

		public static bool CheckConfig()
		{
			if (!File.Exists(App.Settings.PuttyPath))
			{
				ExceptionDialog.Show("Putty not found. Please specify currect putty path.");
				return false;
			}
			return true;
		}

		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			deleteSourceButton.IsEnabled = tabControl.SelectedItem as TabItem != null && (tabControl.SelectedItem as TabItem).Tag as IInstanceSource != null;
		}
	}
}
