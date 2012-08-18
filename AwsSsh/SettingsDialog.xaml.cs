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
				if (!AmazonClient.CheckConnection()) return;
				DialogResult = true;
			}
			finally
			{
				Cursor = Cursors.Arrow;
			}
		}

		private void LinkClick(object sender, MouseButtonEventArgs e)
		{
			Process.Start((sender as FrameworkElement).Tag.ToString());
		}

		private void Clear_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("All your settings and cache will be cleared and application will shutdown", "Clear data", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
			{
				File.Delete(MainWindow.CacheFile);
				App.Settings.Clear();
				App.DontSaveSettings = true;
				App.Current.Shutdown();
			}
		}

	}
}
