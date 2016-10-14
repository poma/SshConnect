using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.Configuration;
using SshConnect.Plugins.Amazon;
using System.Threading;
using System.Reflection;

namespace SshConnect
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static MainWindow instance;

		private const int BigStep = 20;
		private MainWindowViewModel _model;


		public MainWindow()
		{
			instance = this;
			
			_model = new MainWindowViewModel();
			DataContext = _model;

			InitializeComponent();

			// Provent losing focus when invoked using hotkey
			Loaded += (s, e) => Activate();

			// Return focus for mouse clicks etc.
			//new DispatcherTimer { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(200) }
			//	.Tick += (obj, args) => { if (!textBox.IsFocused) textBox.Focus(); };
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			switch (e.Key)
			{
				case Key.Escape:
					Close();
					break;
				case Key.Enter:
					_model.ExecuteCurrentCommand.Execute(null);
					break;
				case Key.F5:
					_model.RefreshListCommand.Execute(null);
					break;
				case Key.Down:
					if (listBox.SelectedIndex < listBox.Items.Count)
					{
						listBox.SelectedIndex++;
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.Up:
					if (listBox.SelectedIndex > 0)
					{
						listBox.SelectedIndex--;
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.PageUp:
					if (listBox.Items.Count > 0)
					{
						listBox.SelectedIndex = Math.Max(0, listBox.SelectedIndex - BigStep);
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.PageDown:
					if (listBox.Items.Count > 0)
					{
						listBox.SelectedIndex = Math.Min(listBox.Items.Count - 1, listBox.SelectedIndex + BigStep);
						listBox.ScrollIntoView(listBox.SelectedItem);
					}
					break;
				case Key.C:
				case Key.Insert:
					if ((e.KeyboardDevice.Modifiers == ModifierKeys.Control) // Copy
						&& string.IsNullOrWhiteSpace(textBox.SelectedText))
					{
						_model.CopyCurrentCommand.Execute(null);
					}
					else goto default;
					break;

				default:
					// This will just send keystroke to textbox
					e.Handled = false;
					textBox.Focus();
					break;
			}			
		}

		private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			// to be able to press buttons we need to do it in dispatcher
			Dispatcher.BeginInvoke(new Action(() => textBox.Focus()));
		}
		private void FocusTextBox(object sender, RoutedEventArgs e)
		{
			textBox.Focus();
		}

		private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			_model.ExecuteCurrentCommand.Execute(null);
		}
	}
}