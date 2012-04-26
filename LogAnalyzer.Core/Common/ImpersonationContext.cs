using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace LogAnalyzer.Common
{
	public sealed class ImpersonationContext
	{
		[DllImport( "advapi32.dll", SetLastError = true )]
		public static extern bool LogonUser( String lpszUsername, String lpszDomain,
		String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken );

		[DllImport( "kernel32.dll", CharSet = CharSet.Auto )]
		public extern static bool CloseHandle( IntPtr handle );

		private const int Logon32ProviderDefault = 0;
		private const int Logon32LogonInteractive = 2;

		private readonly string _domain;
		private readonly string _password;
		private readonly string _username;
		private IntPtr _token;

		private WindowsImpersonationContext _context;

		private bool IsInContext
		{
			get { return _context != null; }
		}

		public ImpersonationContext( string domain, string username, string password )
		{
			_domain = domain;
			_username = username;
			_password = password;
		}

		[PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
		public void Enter()
		{
			if ( this.IsInContext )
			{
				return;
			}

			_token = new IntPtr( 0 );

			_token = IntPtr.Zero;
			bool logonSuccessfull = LogonUser(
			   _username,
			   _domain,
			   _password,
			   Logon32LogonInteractive,
			   Logon32ProviderDefault,
			   ref _token );

			if ( logonSuccessfull == false )
			{
				int error = Marshal.GetLastWin32Error();
				throw new Win32Exception( error );
			}

			WindowsIdentity identity = new WindowsIdentity( _token );
			_context = identity.Impersonate();
		}

		[PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
		public void Leave()
		{
			if ( this.IsInContext == false )
			{
				return;
			}
			_context.Undo();

			if ( _token != IntPtr.Zero )
			{
				CloseHandle( _token );
			}

			_context = null;
		}
	}

	public static class ImpersonationContextExtensions
	{
		public static IDisposable ExecuteInContext( this ImpersonationContext context )
		{
			if ( context != null )
			{
				context.Enter();
			}

			return new ContextDisposable( context );
		}

		private sealed class ContextDisposable : IDisposable
		{
			private readonly ImpersonationContext _context;

			public ContextDisposable( ImpersonationContext context )
			{
				_context = context;
			}

			public void Dispose()
			{
				if ( _context != null )
				{
					_context.Leave();
				}
			}
		}
	}
}
