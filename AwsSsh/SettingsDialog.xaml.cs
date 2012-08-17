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
			DataContext = Properties.Settings.Default;
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

	}
}
