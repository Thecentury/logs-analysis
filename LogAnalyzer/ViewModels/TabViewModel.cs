using System;
using System.Reactive.Concurrency;
using System.Windows.Input;
using System.Windows;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels
{
	public abstract class TabViewModel : BindingObject, ITypeName
	{
		private readonly ApplicationViewModel applicationViewModel;
		public ApplicationViewModel ApplicationViewModel
		{
			get { return applicationViewModel; }
		}

		public IScheduler Scheduler
		{
			get { return applicationViewModel.Config.ResolveNotNull<IScheduler>(); }
		}

		protected TabViewModel( ApplicationViewModel applicationViewModel )
		{
			if ( applicationViewModel == null )
				throw new ArgumentNullException( "applicationViewModel" );

			this.applicationViewModel = applicationViewModel;
		}

		public string Type
		{
			get { return GetType().Name; }
		}

		public virtual string Tooltip
		{
			get { return Header; }
		}

		public virtual string Header { get { return "<HEADER NOT SET>"; } }
		public virtual bool IsActive { get; set; }
		public abstract string IconFile { get; }

		protected string MakePackUri( string uri )
		{
			return PackUriHelper.MakePackUri( uri );
		}

		public bool CanBeClosed
		{
			get { return CanBeClosedCore(); }
		}

		protected virtual bool CanBeClosedCore()
		{
			return true;
		}

		public void Close()
		{
			if ( CanBeClosedCore() )
			{
				OnTabClosing();
				applicationViewModel.Tabs.Remove( this );
			}
		}

		protected virtual void OnTabClosing() { }

		private DelegateCommand closeCommand;
		public ICommand CloseCommand
		{
			get
			{
				if ( closeCommand == null )
				{
					closeCommand = new DelegateCommand( Close, CanBeClosedCore );
				}

				return closeCommand;
			}
		}

		public Visibility CloseButtonVisibility
		{
			get { return CanBeClosed ? Visibility.Visible : Visibility.Collapsed; }
		}

		#region Loaded command

		private DelegateCommand loadedCommand;
		public ICommand LoadedCommand
		{
			get
			{
				if ( loadedCommand == null )
					loadedCommand = new DelegateCommand( OnLoaded );

				return loadedCommand;
			}
		}

		protected virtual void OnLoaded()
		{
		}

		#endregion

		// todo Clone and CloseAll and others similar commands
	}
}
