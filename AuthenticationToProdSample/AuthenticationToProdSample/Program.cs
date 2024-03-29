﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace AuthenticationToProdSample
{
	class Program
	{
		static void Main( string[] args )
		{
			ImpersonationContext ctx = new ImpersonationContext( "MO", "mikhail.brinchuk", "." );

			try
			{
				ctx.Enter();

				const string path = @"\\192.168.101.41\logs";
				var dirs = Directory.GetDirectories( path );
			}
			finally
			{
				ctx.Leave();
			}
		}
	}

	public sealed class ImpersonationContext : IDisposable
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

		public static IDisposable ExecuteInContext( string domain, string username, string password )
		{
			var ctx = new ImpersonationContext( domain, username, password );
			ctx.Enter();
			return ctx;
		}

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

		[PermissionSetAttribute( SecurityAction.Demand, Name = "FullTrust" )]
		public void Enter()
		{
			if ( this.IsInContext ) return;
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

		[PermissionSetAttribute( SecurityAction.Demand, Name = "FullTrust" )]
		public void Leave()
		{
			if ( this.IsInContext == false ) return;
			_context.Undo();

			if ( _token != IntPtr.Zero ) CloseHandle( _token );
			_context = null;
		}

		public void Dispose()
		{
			Leave();
		}
	}
}
