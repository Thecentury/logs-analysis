using System;
using System.Reactive.Concurrency;
using AdTech.Common.WPF;
using System.Windows.Input;
using System.Windows;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels
{
	public class TabViewModel : BindingObject, ITypeName
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

		public virtual string Header { get { return "<HEADER NOT SET>"; } }
		public virtual bool IsActive { get; set; }
		public virtual string IconFile
		{
			get { return null; }
		}

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
				OnClosing();
				applicationViewModel.Tabs.Remove( this );
			}
		}

		protected virtual void OnClosing() { }

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

		// todo Clone and Close and CloseAll and others similar commands
	}
}
