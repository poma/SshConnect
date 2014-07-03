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
using AwsSsh.Plugins.Chef;
using AwsSsh.Plugins.Putty;

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
			foreach (var source in App.InstanceCollection.InstanceSources)
			{
				if (!(source is PuttyInstanceSource))
					tabControl.Items.Add(new TabItem { Header = source.Name, Content = source.SettingsControl, Tag = source, DataContext = source.Settings });
			}
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Cursor = Cursors.Wait;
				//if (!CheckConfig()) return;
				//if (!AmazonInstanceSource.CheckConnection()) return;
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
			tabControl.Items.Add(new TabItem { Header = source.Name, Content = source.SettingsControl, Tag = source, DataContext = source.Settings });
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
			}
		}

		public static bool CheckConfig()
		{
			if (!File.Exists(App.Settings.PuttyPath))
			{
				ExceptionDialog.Show("Putty not found. Please check your configuration");
				return false;
			}
			return true;
		}
	}
}
