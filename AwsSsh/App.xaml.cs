using System;	
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace AwsSsh
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static Settings _settings;
		public static Settings Settings
		{
			get 
			{ 
				if (_settings == null)
					_settings = SettingsBase.Load() as Settings ?? new Settings();
				return _settings; 
			}
		}

		private static InstanceCollection _instanceCollection;
		public static InstanceCollection InstanceCollection
		{
			get { return _instanceCollection; }
		}

		private static bool DontSaveSettings;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			DispatcherUnhandledException += ExceptionDialog.Handler;
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			_instanceCollection = new InstanceCollection();

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

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (!DontSaveSettings)
			{
				Settings.Save();
				InstanceCache.Save(App.InstanceCollection.Instances);
			}
		}

		public static void ClearSettingsAndExit()
		{
			InstanceCache.Clear();
			App.Settings.Clear();
			App.DontSaveSettings = true;
			App.Current.Shutdown();
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
