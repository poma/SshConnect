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
		private static Settings Settings
		{
			get { return Settings.Default; }
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (Settings.Default.NeedsUpgrade)
			{
				Settings.Upgrade();
				Settings.NeedsUpgrade = false;
				Settings.Save();
			}
			if (Settings.IsFirstTimeConfiguration && false)
			{
				ShutdownMode = ShutdownMode.OnExplicitShutdown;
				var dialog = new SettingsDialog();
				if (dialog.ShowDialog() == true)
				{
					Settings.IsFirstTimeConfiguration = false;
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
			Settings.Save();
		}

		public static bool CheckConfig()
		{
			if (!File.Exists(Settings.PuttyPath))
			{
				MessageBox.Show("Putty not found. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			if (!File.Exists(Settings.KeyPath))
			{
				MessageBox.Show("Key file not found. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			if (string.IsNullOrWhiteSpace(Settings.AWSAccessKey) || string.IsNullOrWhiteSpace(Settings.AWSAccessKey))
			{
				MessageBox.Show("Amazon security credentials are empty. Please check your configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			return true;
		}
	}
}
