using System;
using System.Collections.Generic;
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

namespace SshConnect.Plugins.Chef
{
	/// <summary>
	/// Interaction logic for ChefSettingsControl.xaml
	/// </summary>
	public partial class ChefSettingsControl : UserControl
	{
		public ChefSettingsControl()
		{
			InitializeComponent();
		}

		private void TestConnection_Click(object sender, RoutedEventArgs e)
		{
			var src = new ChefInstanceSource { Settings = DataContext as ChefSettings };
			testConnectionButton.IsEnabled = false;
			(Parent as Control).Cursor = Cursors.Wait;
			Task.Factory.StartNew(() => src.GetInstanceList())
				.ContinueWith(t => 
				{
					testConnectionButton.IsEnabled = true;
					(Parent as Control).Cursor = Cursors.Arrow;
					if (t.Exception != null)
						ExceptionDialog.Show(t.Exception);
					else
						MessageBox.Show("Connection Successful");
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
