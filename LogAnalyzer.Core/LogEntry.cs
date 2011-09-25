using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.ComponentModel;
using LogAnalyzer.Extensions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LogAnalyzer
{
	[DebuggerDisplay( "Entry File={ParentLogFile.FullPath} LineIndex={LineIndex}" )]
	public sealed class LogEntry : INotifyPropertyChanged, IFreezable
	{
		// todo probably read from config
		private static readonly string exceptionRegexText = @"^\s*at (?<Function>.*) in (?<File>.*):line (?<LineNumber>\d+)$";
		private static Regex exceptionRegex = null;

		public string Type { get; private set; }
		public int ThreadId { get; private set; }
		public DateTime Time { get; private set; }
		public int LineIndex { get; private set; }
		public LogFile ParentLogFile { get; private set; }

		private readonly List<string> textLines = new List<string>();
		public IList<string> TextLines
		{
			get { return textLines; }
		}

		public string UnitedText
		{
			get
			{
				return String.Join( Environment.NewLine, textLines );
			}
		}

		public int LinesCount
		{
			get { return textLines.Count; }
		}

		// todo хранить тут и номер записи в списке

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="threadId"></param>
		/// <param name="time"></param>
		/// <param name="firstLine"></param>
		/// <param name="lineIndex">Номер строки в файле лога.</param>
		/// <param name="parentLogFile"></param>
		internal LogEntry( string type, int threadId, DateTime time, string firstLine, int lineIndex, LogFile parentLogFile )
		{
			if ( lineIndex < 0 )
				throw new ArgumentOutOfRangeException( "lineIndex" );
			if ( parentLogFile == null )
				throw new ArgumentNullException( "parentLogFile" );

			this.ParentLogFile = parentLogFile;

			SetHeaderValues( type, threadId, time, firstLine );

			this.textLines.Add( firstLine );
			this.LineIndex = lineIndex;
		}

		/// <summary>
		/// Валидирует параметры и устанавливает некоторые поля.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="threadId"></param>
		/// <param name="time"></param>
		/// <param name="textLine"></param>
		private void SetHeaderValues( string type, int threadId, DateTime time, string textLine )
		{
			if ( String.IsNullOrEmpty( type ) || type.Length > 1 )
				throw new ArgumentException( "type" );
			if ( threadId < 0 )
				throw new ArgumentOutOfRangeException( "threadId" );
			if ( textLine == null )
				throw new ArgumentNullException( "text" );
			if ( isFrozen )
				throw new InvalidOperationException( "Cannot modify frozen object." );

			this.Type = type;
			this.ThreadId = threadId;
			this.Time = time;
		}

		public void UpdateHeader( string type, int threadId, DateTime time, string firstLine )
		{
			SetHeaderValues( type, threadId, time, firstLine );

			this.textLines[0] = firstLine;

			RaiseAllPropertiesChanged();
		}

		public void ReplaceLastLine( string newLine, long newLineIndex )
		{
			if ( newLine == null )
				throw new ArgumentNullException( "line" );

			if ( textLines.Count == 0 )
				throw new InvalidOperationException();

			int lastLineIndex = this.LineIndex + this.textLines.Count - 1;

			if ( lastLineIndex != newLineIndex )
				throw new InvalidOperationException();

			if ( isFrozen )
				throw new InvalidOperationException( "Cannot modify frozen object." );

			textLines[textLines.Count - 1] = newLine;
			RaiseAllPropertiesChanged();
		}

		public void AppendLine( string newLine )
		{
			if ( newLine == null )
				throw new ArgumentNullException( "line" );

			// первая строка должна извлекаться из заголовка LogEntry
			if ( textLines.Count == 0 )
				throw new InvalidOperationException();

			if ( isFrozen )
				throw new InvalidOperationException( "Cannot modify frozen object." );

			textLines.Add( newLine );
			RaiseAllPropertiesChanged();
		}

		// todo вынести это отсюда куда-нибудь еще, например, в Core
		public FileLineInfo GetExceptionLine( string line )
		{
			if ( line == null )
				throw new ArgumentNullException( "line" );

			// todo propably, make this thread-safe
			if ( exceptionRegex == null )
			{
				exceptionRegex = new Regex( exceptionRegexText, RegexOptions.Compiled );
			}

			Match match = exceptionRegex.Match( line );
			if ( match.Success )
			{
				string function = match.Groups["Function"].Value;
				string fileName = match.Groups["File"].Value;
				string excLineNumberText = match.Groups["LineNumber"].Value;

				int excLineNumber = -1;
				if ( Int32.TryParse( excLineNumberText, out excLineNumber ) )
				{
					FileLineInfo result = new FileLineInfo
					{
						MethodName = function,
						FileName = fileName,
						LineNumber = excLineNumber
					};

					return result;
				}
			}

			return null;
		}

		#region INotifyPropertyChanged Members

		// todo более экономное к памяти решение
		private PropertyChangedEventHandler propertyChanged = null;
		public event PropertyChangedEventHandler PropertyChanged
		{
			[MethodImpl( MethodImplOptions.Synchronized )]
			add
			{
				// не подписываем на события, если Frozen
				if ( isFrozen )
				{
					return;
				}

				propertyChanged = (PropertyChangedEventHandler)Delegate.Combine( propertyChanged, value );
			}
			[MethodImpl( MethodImplOptions.Synchronized )]
			remove
			{
				if ( isFrozen )
				{
					return;
				}

				propertyChanged = (PropertyChangedEventHandler)Delegate.Remove( propertyChanged, value );
			}
		}

		private void RaisePropertyChanged( string propertyName )
		{
			propertyChanged.Raise( this, propertyName );
		}

		private void RaiseAllPropertiesChanged()
		{
			RaisePropertyChanged( String.Empty );
		}

		#endregion

		#region IFreezable Members

		public void Freeze()
		{
			// already frozen?
			if ( isFrozen )
				throw new InvalidOperationException();

			isFrozen = true;

			if ( textLines.Count < textLines.Capacity )
			{
				textLines.TrimExcess();
			}

			propertyChanged = null;
		}

		private bool isFrozen = false;
		public bool IsFrozen
		{
			get { return isFrozen; }
		}

		#endregion
	}
}
