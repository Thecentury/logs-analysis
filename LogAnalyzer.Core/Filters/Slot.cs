using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Filters
{
	public sealed class Slot : IEquatable<Slot>
	{
		public string Name { get; private set; }
		public Type Type { get; private set; }

		public Slot( string name, Type type )
		{
			if ( name.IsNullOrEmpty() )
				throw new ArgumentNullException( "name" );
			if ( type == null )
				throw new ArgumentNullException( "type" );

			this.Name = name;
			this.Type = type;
		}

		public override bool Equals( object obj )
		{
			Slot other = obj as Slot;
			if ( other == null )
				return false;

			return Equals( other );
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public bool Equals( Slot other )
		{
			if ( other == null )
				return false;

			return String.Equals( Name, other.Name );
		}

		public override string ToString()
		{
			return "\"{0}\" of type \"{1}\"".Format2( Name, Type.Name );
		}
	}
}
