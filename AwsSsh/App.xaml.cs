using System;	
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.IO.Compression;
using AwsSsh.ApplicationSettings;
using System.Reflection;

namespace AwsSsh
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static bool DontSaveSettings { get; set; }

		private static Settings _settings;
		public static Settings Settings
		{
			get 
			{ 
				if (_settings == null)
					_settings = new Settings();
				return _settings; 
			}
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			Settings.Load();

			if (Settings.IsFirstTimeConfiguration)
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
			if (!DontSaveSettings)
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


		private static Assembly LoadEmbeddedDll(string name)
        {
			using (var stream = Application.GetResourceStream(new Uri(name, UriKind.Relative)).Stream)
				using (var mem = new MemoryStream())
				{
					var gzip = new GZipStream(stream, CompressionMode.Decompress);
					gzip.CopyTo(mem);
					var buf = mem.ToArray();
					return Assembly.Load(buf);
				}
        }

		Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (args.Name.Contains("AWSSDK")) 
				return LoadEmbeddedDll("AWSSDK.dll.gz");
			
			return null;
		}

	}
}
