using System.Collections.Generic;

namespace LogAnalyzer.Auxilliary
{
	public sealed class KeyValueStorage
	{
		private readonly Dictionary<string, object> keyValues = new Dictionary<string, object>();

		private KeyValueStorage() { }

		private static readonly KeyValueStorage instance = new KeyValueStorage();
		public static KeyValueStorage Instance
		{
			get { return instance; }
		}

		public void Add( string key, object value )
		{
			keyValues.Add( key, value );
		}

		public bool Contains( string key )
		{
			return keyValues.ContainsKey( key );
		}

		public object this[string key]
		{
			get { return keyValues[key]; }
		}
	}
}
