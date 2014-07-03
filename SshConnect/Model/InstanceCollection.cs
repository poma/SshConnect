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
using MvvmFoundation.Wpf;
using SshConnect.Plugins.Putty;
using SshConnect.Plugins.Chef;

namespace SshConnect
{
	public class InstanceCollection: ObservableObject
	{		
		private int _loadingCount;

        private bool _isLoading;
		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				if (_isLoading == value) return;
				_isLoading = value;
				OnPropertyChanged("IsLoading");
			}
		}

		private ObservableCollection<Instance> _instances;
		public ObservableCollection<Instance> Instances
		{
			get
			{
				if (_instances == null)
					_instances = new ObservableCollection<Instance>();
				return _instances;
			}
		}

		private List<IInstanceSource> _instanceSources;
		public List<IInstanceSource> InstanceSources
		{
			get { return _instanceSources; }
			set
			{
				if (_instanceSources == value) return;
				_instanceSources = value;
				OnPropertyChanged("InstanceSources");
			}
		}

		private Stack<Exception> _errors = new Stack<Exception>();
		public bool ErrorsPresent
		{
			get { return _errors.Count > 0; }
		}

		public InstanceCollection(List<IInstanceSource> sources = null)
		{
			InstanceSources = sources ?? new List<IInstanceSource>();
			if (InstanceSources.Where(s => s is PuttyInstanceSource).Count() == 0)
			{
				var src = new PuttyInstanceSource();
				App.Settings.InstanceSources.Add(src);
				InstanceSources.Add(src);
			}
			App.InstanceCollection = this; // needed to load cache

			// todo: wrap exceptions
			var list = InstanceCache.Load();
			if (list != null)
				list.ForEach(a => Instances.Add(a));
			RefreshList();
		}


		public void RefreshList()
		{
			foreach (var src in InstanceSources)
			{
				
				BackgroundWorker w = new BackgroundWorker();
				IsLoading = true;
				_loadingCount++;
				w.DoWork += (obj, args) => args.Result = new Tuple<IInstanceSource, List<Instance>>(args.Argument as IInstanceSource, (args.Argument as IInstanceSource).GetInstanceList()) ;
				w.RunWorkerCompleted += (obj, args) =>
				{
					_loadingCount--;
					if (_loadingCount == 0)
						IsLoading = false;
					if (args.Error != null)
					{
						//_updateTimer.IsEnabled = false;
						_errors.Push(new ApplicationException("Error downloading server list: " + args.Error.Message, args.Error));
						OnPropertyChanged("ErrorsPresent");
						return;
					}
					
					if (MainWindowViewModel.instance != null)
						MainWindowViewModel.instance.FreezeSelection();

					var res = args.Result as Tuple<IInstanceSource, List<Instance>>;
					MergeInstances(res.Item1, res.Item2);

					if (MainWindowViewModel.instance != null)
						MainWindowViewModel.instance.RestoreSelection();
				};
				w.RunWorkerAsync(src);
			}
		}

		private void MergeInstances(IInstanceSource src, List<Instance> newInstances)
		{
			var existingInstances = Instances;
			var c = new InstanceComparer();
			var itemsToRemove = existingInstances.Where(a => a.Source == src).Except(newInstances, c).ToList();
			var itemsToAdd = newInstances.Except(existingInstances, c).ToList();
			var itemsToUpdate = existingInstances.Where(a => a.Source == src).Join(newInstances, a => a.GetId(), a => a.GetId(), (a, b) => new { Old = a, New = b }).ToList();
			itemsToAdd.ForEach(a => existingInstances.Add(a));
			itemsToRemove.ForEach(a => existingInstances.Remove(a));
			itemsToUpdate.ForEach(a => CopyPropertyAttribute.CopyProperties(a.New, a.Old));
		}

		public Exception GetNextException()
		{
			var ex = _errors.Pop();
			OnPropertyChanged("ErrorsPresent");
			return ex;
		}

		private class InstanceComparer : IEqualityComparer<Instance>
		{
			public bool Equals(Instance x, Instance y)
			{
				return x != null && y != null && x.GetId() == y.GetId();
			}

			public int GetHashCode(Instance obj)
			{
				return obj.GetId().GetHashCode();
			}
		}
	}
}
