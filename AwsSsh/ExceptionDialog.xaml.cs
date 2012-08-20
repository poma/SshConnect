using System;
using System.Windows;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Threading;

namespace AwsSsh
{
	public class ExceptionDialog
	{
		public static bool IgnoreErrors { get; set; }
		private static bool DialogShown { get; set; }

		public static void Show(Exception e)
		{
			if (!IgnoreErrors && !DialogShown)
			{
				if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) DoShowWindow(e);
				else
				{
					DialogShown = true; // Set shown to true becaue there can be delay before actual func execution
					TryDispatch(() => DoShowWindow(e));
				}
			}
		}

		public static void Show(string message)
		{
			Show(new Exception(message));
		}

		private static void DoShowWindow(Exception e)
		{
			DialogShown = true;
			try
			{
				var exception = WrapKnownExceptions(e);
				new ExceptionDialogWindow(exception).ShowDialog();
			}
			catch
			{
				try { MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
				catch { }
			}
			DialogShown = false;
		}

		public static void Handler(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Show(e.Exception);
			e.Handled = true;
		}

		private static void TryDispatch(Action action)
		{
			try { if (Application.Current != null && Application.Current.Dispatcher != null)
				Application.Current.Dispatcher.BeginInvoke(action);
			}
			catch { }
		}

		internal static Exception WrapKnownExceptions(Exception src)
		{
			if (!src.Data.Contains("ThrowPath")) src.Data.Add("ThrowPath", Environment.StackTrace);
			return src;
		}
	}

	public partial class ExceptionDialogWindow
	{
		public string Error { get; set; }
		public string Details { get; set; }
		private readonly Exception exception;

		internal ExceptionDialogWindow(Exception e)
		{
			exception = e;
			Error = e.Message;
			Details = e.GetXml().ToString();
			InitializeComponent();
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}