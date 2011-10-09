using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using ModuleLogsProvider.Interfaces;
using Morqua.Logging;

namespace Awad.Eticket.ModuleLogsProvider
{
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	internal sealed class MostLogSinkService : ILogSinkService
	{
		private readonly ILogger logger;

		public MostLogSinkService( ILogger logger )
		{
			if ( logger == null ) throw new ArgumentNullException( "logger" );
			this.logger = logger;
		}

		public void WriteError( string message )
		{
			logger.WriteLine( MessageType.Error, message );
		}

		public void WriteInfo( string message )
		{
			logger.WriteLine( MessageType.Info, message );
		}
	}
}
