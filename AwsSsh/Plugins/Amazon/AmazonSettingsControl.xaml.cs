using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AwsSsh.Plugins.Amazon
{
	/// <summary>
	/// Interaction logic for AmazonSettingsDialog.xaml
	/// </summary>
	public partial class AmazonSettingsControl : UserControl
	{
		public AmazonSettingsControl()
		{
			InitializeComponent();
		}

		private void LinkClick(object sender, RoutedEventArgs e)
		{
			string link = ((sender as FrameworkElement).Tag ?? "").ToString();
			if (!string.IsNullOrEmpty(link))
				Process.Start(link);
		}

		private void TestConnection_Click(object sender, RoutedEventArgs e)
		{
			var src = new AmazonInstanceSource { Settings = DataContext as AmazonSettings };
			testConnectionButton.IsEnabled = false;
			Task.Factory.StartNew(() => src.GetInstanceList())
				.ContinueWith(t =>
				{
					testConnectionButton.IsEnabled = true;
					if (t.Exception != null)
						ExceptionDialog.Show(t.Exception);
					else
						MessageBox.Show("Connection Successful");
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
