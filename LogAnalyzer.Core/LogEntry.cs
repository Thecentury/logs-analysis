﻿using System;
using System.Collections.Generic;
using System.IO;
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
	[DebuggerDisplay( "LogEntry File={ParentLogFile.FullPath} LineIndex={LineIndex}" )]
	public sealed class LogEntry : INotifyPropertyChanged, IFreezable, IHaveTime, ILogEntry, ILogVisitable
	{
		// todo brinchuk remove me
		// todo probably read from config
		private const string ExceptionRegexText = @"^\s*at (?<Function>.*) in (?<File>.*):line (?<LineNumber>\d+)$";
		private static readonly Regex exceptionRegex = new Regex( ExceptionRegexText, RegexOptions.Compiled );

		public string Type { get; private set; }
		public int ThreadId { get; private set; }
		public DateTime Time { get; private set; }
		public int LineIndex { get; private set; }

		public LogFile ParentLogFile { get; set; }

		private List<string> _textLines = new List<string>();
		public IList<string> TextLines
		{
			get { return _textLines; }
		}

		private string _unitedText;
		public string UnitedText
		{
			get { return _unitedText ?? CreateUnitedText(); }
		}

		private string CreateUnitedText()
		{
			return _unitedText = String.Join( Environment.NewLine, _textLines );
		}

		public int LinesCount
		{
			get
			{
				int count;
				if ( _textLines != null )
				{
					count = _textLines.Count;
				}
				else
				{
					count = _unitedText.Count( c => c == '\n' );
				}

				return count;
			}
		}

		/// <summary>
		/// Для тестов.
		/// </summary>
		/// <param name="parentLogFile"></param>
		internal LogEntry( LogFile parentLogFile )
		{
			ParentLogFile = parentLogFile;
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
		public LogEntry( string type, int threadId, DateTime time, string firstLine, int lineIndex, LogFile parentLogFile )
		{
			if ( lineIndex < 0 )
				throw new ArgumentOutOfRangeException( "lineIndex" );

			this.ParentLogFile = parentLogFile;

			SetHeaderValues( type, threadId, time, firstLine );

			this._textLines.Add( firstLine );
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
			if ( String.IsNullOrEmpty( type ) /*|| type.Length > 1*/ )
				throw new ArgumentException( "type" );
			if ( threadId < 0 )
				throw new ArgumentOutOfRangeException( "threadId" );
			if ( textLine == null )
				throw new ArgumentNullException( "textLine" );
			if ( _isFrozen )
				throw new InvalidOperationException( "Cannot modify frozen object." );

			this.Type = type;
			this.ThreadId = threadId;
			this.Time = time;
		}

		public void UpdateHeader( string type, int threadId, DateTime time, string firstLine )
		{
			SetHeaderValues( type, threadId, time, firstLine );

			this._textLines[0] = firstLine;

			RaiseAllPropertiesChanged();
		}

		public void ReplaceLastLine( string newLine, long newLineIndex )
		{
			if ( newLine == null )
			{
				throw new ArgumentNullException( "newLine" );
			}

			if ( _textLines.Count == 0 )
			{
				throw new InvalidOperationException();
			}

			int lastLineIndex = this.LineIndex + this._textLines.Count - 1;

			if ( lastLineIndex != newLineIndex )
			{
				throw new InvalidOperationException();
			}

			if ( _isFrozen )
			{
				throw new InvalidOperationException( "Cannot modify frozen object." );
			}

			_textLines[_textLines.Count - 1] = newLine;
			RaiseAllPropertiesChanged();
		}

		public void AppendLine( string newLine )
		{
			if ( newLine == null )
			{
				throw new ArgumentNullException( "newLine" );
			}

			// первая строка должна извлекаться из заголовка LogEntry
			if ( _textLines.Count == 0 )
			{
				throw new InvalidOperationException();
			}

			if ( _isFrozen )
			{
				throw new InvalidOperationException( "Cannot modify frozen object." );
			}

			_textLines.Add( newLine );
			RaiseAllPropertiesChanged();
		}

		// todo вынести это отсюда куда-нибудь еще, например, в Core
		public FileLineInfo GetExceptionLine( string line )
		{
			if ( line == null )
			{
				throw new ArgumentNullException( "line" );
			}

			Match match = exceptionRegex.Match( line );
			if ( match.Success )
			{
				string function = match.Groups["Function"].Value;
				string fileName = match.Groups["File"].Value;
				string excLineNumberText = match.Groups["LineNumber"].Value;

				int excLineNumber;
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
		private PropertyChangedEventHandler _propertyChanged;
		public event PropertyChangedEventHandler PropertyChanged
		{
			[MethodImpl( MethodImplOptions.Synchronized )]
			add
			{
				// не подписываем на события, если Frozen
				if ( _isFrozen )
				{
					return;
				}

				_propertyChanged = (PropertyChangedEventHandler)Delegate.Combine( _propertyChanged, value );
			}
			[MethodImpl( MethodImplOptions.Synchronized )]
			remove
			{
				if ( _isFrozen )
				{
					return;
				}

				_propertyChanged = (PropertyChangedEventHandler)Delegate.Remove( _propertyChanged, value );
			}
		}

		private void RaisePropertyChanged( string propertyName )
		{
			_propertyChanged.Raise( this, propertyName );
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
			if ( _isFrozen )
			{
				return;
			}

			_isFrozen = true;

			CreateUnitedText();

			_textLines = null;
			_propertyChanged = null;
		}

		private bool _isFrozen;
		public bool IsFrozen
		{
			get { return _isFrozen; }
		}

		#endregion

		public void Accept( ILogVisitor visitor )
		{
			visitor.Visit( this );
		}
	}
}
