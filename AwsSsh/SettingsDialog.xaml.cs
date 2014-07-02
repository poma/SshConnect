using System;
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
using AwsSsh.Plugins.Amazon;
using System.Windows.Controls.Primitives;

namespace AwsSsh
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
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Cursor = Cursors.Wait;
				if (!App.CheckConfig()) return;
				if (!AmazonInstanceSource.CheckConnection()) return;
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

		private void ClearSettings_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("All your settings and cache will be cleared and application will shutdown", "Clear data", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
			{
				File.Delete(InstanceCache.CacheFile);
				App.Settings.Clear();
				App.DontSaveSettings = true;
				App.Current.Shutdown();
			}
		}

		private void ClearList_Click(object sender, RoutedEventArgs e)
		{
			MainWindowViewModel.instance.InstanceCollection.Instances.Clear();
		}

		private void showShareMenu(object sender, MouseButtonEventArgs e)
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

	}
}
