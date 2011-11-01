using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace LogAnalyzer.GUI.Common
{
	public class AttachedCommandBinding : Freezable, IDisposable
	{
		private DependencyObject _Owner;
		private bool isDisposed;
		private EventInfo eventInfo;
		private Delegate eventHandler;
		private bool isKeyGestureBound;
		private static bool isInDesignMode;
		public static readonly DependencyProperty CommandProperty;
		public static readonly DependencyProperty CommandParameterProperty;
		public static readonly DependencyProperty ActionProperty;
		public static readonly DependencyProperty EventProperty;
		public static readonly DependencyProperty KeyGestureProperty;
		public DependencyObject Owner
		{
			get
			{
				return this._Owner;
			}
			set
			{
				this._Owner = value;
				this.InvalidateAttachedBinding();
			}
		}
		public ICommand Command
		{
			get
			{
				return (ICommand)base.GetValue( AttachedCommandBinding.CommandProperty );
			}
			set
			{
				base.SetValue( AttachedCommandBinding.CommandProperty, value );
			}
		}
		public object CommandParameter
		{
			get
			{
				return base.GetValue( AttachedCommandBinding.CommandParameterProperty );
			}
			set
			{
				base.SetValue( AttachedCommandBinding.CommandParameterProperty, value );
			}
		}
		public Action<object> Action
		{
			get
			{
				return (Action<object>)base.GetValue( AttachedCommandBinding.ActionProperty );
			}
			set
			{
				base.SetValue( AttachedCommandBinding.ActionProperty, value );
			}
		}
		public string Event
		{
			get
			{
				return (string)base.GetValue( AttachedCommandBinding.EventProperty );
			}
			set
			{
				base.SetValue( AttachedCommandBinding.EventProperty, value );
			}
		}
		public KeyGesture KeyGesture
		{
			get
			{
				return (KeyGesture)base.GetValue( AttachedCommandBinding.KeyGestureProperty );
			}
			set
			{
				base.SetValue( AttachedCommandBinding.KeyGestureProperty, value );
			}
		}
		static AttachedCommandBinding()
		{
			AttachedCommandBinding.CommandProperty = DependencyProperty.Register( "Command", typeof( ICommand ), typeof( AttachedCommandBinding ), new FrameworkPropertyMetadata( null ) );
			AttachedCommandBinding.CommandParameterProperty = DependencyProperty.Register( "CommandParameter", typeof( object ), typeof( AttachedCommandBinding ), new FrameworkPropertyMetadata( null ) );
			AttachedCommandBinding.ActionProperty = DependencyProperty.Register( "Action", typeof( Action<object> ), typeof( AttachedCommandBinding ), new FrameworkPropertyMetadata( null ) );
			AttachedCommandBinding.EventProperty = DependencyProperty.Register( "Event", typeof( string ), typeof( AttachedCommandBinding ), new FrameworkPropertyMetadata( null ) );
			AttachedCommandBinding.KeyGestureProperty = DependencyProperty.Register( "KeyGesture", typeof( KeyGesture ), typeof( AttachedCommandBinding ), new FrameworkPropertyMetadata( null ) );
			DependencyProperty isInDesignModeProperty = DesignerProperties.IsInDesignModeProperty;
			AttachedCommandBinding.isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty( isInDesignModeProperty, typeof( FrameworkElement ) ).Metadata.DefaultValue;
		}
		private void InvalidateAttachedBinding()
		{
			if ( this.Owner != null )
			{
				if ( this.eventInfo != null )
				{
					this.Dispose();
				}
				if ( this.isKeyGestureBound )
				{
					this.UnhookKeyGesture( this.Owner as UIElement );
				}
				if ( this.Event != null )
				{
					this.BindEvent( this.Owner, this.Event );
				}
				if ( this.KeyGesture != null )
				{
					this.BindKeyGesture( this.Owner as UIElement );
				}
			}
		}
		public void BindEvent( DependencyObject owner, string eventName )
		{
			if ( !AttachedCommandBinding.isInDesignMode )
			{
				this.eventInfo = owner.GetType().GetEvent( eventName, BindingFlags.Instance | BindingFlags.Public );
				if ( this.eventInfo == null )
				{
					throw new ArgumentException( string.Format( "Could not find an event with the name {0} on type {1}", eventName, owner.GetType() ) );
				}
				this.eventHandler = this.CreateEventHandler( this.eventInfo );
				this.eventInfo.AddEventHandler( owner, this.eventHandler );
			}
		}
		private void OnEventRaised( object sender, EventArgs e )
		{
			object obj = (this.CommandParameter == null) ? e : this.CommandParameter;
			if ( this.Command != null && this.Command.CanExecute( obj ) )
			{
				this.Command.Execute( obj );
			}
			if ( this.Action != null )
			{
				this.Action( obj );
			}
		}
		private Delegate CreateEventHandler( EventInfo eventinfo )
		{
			if ( eventinfo == null )
			{
				throw new ArgumentNullException( "eventInfo" );
			}
			if ( eventinfo.EventHandlerType == null )
			{
				throw new ArgumentException( "EventHandlerType is null" );
			}
			if ( this.eventHandler == null )
			{
				MethodInfo method = base.GetType().GetMethod( "OnEventRaised", BindingFlags.Instance | BindingFlags.NonPublic );
				this.eventHandler = Delegate.CreateDelegate( eventinfo.EventHandlerType, this, method );
			}
			return this.eventHandler;
		}
		private void BindKeyGesture( UIElement target )
		{
			if ( target != null )
			{
				target.AddHandler( UIElement.KeyDownEvent, new KeyEventHandler( this.OnKeyDown ), true );
				this.isKeyGestureBound = true;
			}
		}
		private void OnKeyDown( object sender, KeyEventArgs e )
		{
			if ( e.Key == this.KeyGesture.Key && e.KeyboardDevice.Modifiers == this.KeyGesture.Modifiers )
			{
				if ( this.Command != null )
				{
					if ( this.Command.CanExecute( this.CommandParameter ) )
					{
						this.Command.Execute( this.CommandParameter );
					}
					e.Handled = true;
				}
			}
		}
		private void UnhookKeyGesture( UIElement target )
		{
			if ( target != null )
			{
				target.RemoveHandler( UIElement.KeyDownEvent, new KeyEventHandler( this.OnKeyDown ) );
			}
		}
		protected override Freezable CreateInstanceCore()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			this.Dispose( true );
			GC.SuppressFinalize( this );
		}
		protected virtual void Dispose( bool disposing )
		{
			if ( disposing && !this.isDisposed )
			{
				this.eventInfo.RemoveEventHandler( this.Owner, this.eventHandler );
				this.isDisposed = true;
			}
		}
	}
}
