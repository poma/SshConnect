using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using AwsSsh.Properties;

namespace AwsSsh
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (Settings.Default.FirstRun)
			{
				ShutdownMode = ShutdownMode.OnExplicitShutdown;
				var dialog = new SettingsDialog();
				if (dialog.ShowDialog() == true)
				{
					Settings.Default.FirstRun = false;
					ShutdownMode = ShutdownMode.OnMainWindowClose;
				}
				else
				{
					Shutdown();
					return;
				}
				
			}
			new MainWindow().Show();
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show(e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			Settings.Default.Save();
		}

		public static bool CheckConfig()
		{
			if (!File.Exists(Settings.Default.PuttyPath))
			{
				MessageBox.Show("Putty not found. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			if (!File.Exists(Settings.Default.KeyPath))
			{
				MessageBox.Show("Key file not found. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			if (string.IsNullOrWhiteSpace(Settings.Default.AWSAccessKey) || string.IsNullOrWhiteSpace(Settings.Default.AWSAccessKey))
			{
				MessageBox.Show("Amazon security credentials are empty. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			return true;
		}
	}
}
