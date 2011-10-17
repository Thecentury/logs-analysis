using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.Regions
{
	internal sealed class FutureRegion : IRegion
	{
		private readonly List<object> children = new List<object>();
		public List<object> Children
		{
			get { return children; }
		}

		public FutureRegion( string name )
		{
			if ( name == null ) throw new ArgumentNullException( "name" );

			this.name = name;
		}

		private readonly string name;
		public string Name
		{
			get { return name; }
		}

		public void Add( object child )
		{
			if ( child == null ) throw new ArgumentNullException( "child" );

			children.Add( child );
		}
	}
}
