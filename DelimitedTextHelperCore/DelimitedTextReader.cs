﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DelimitedTextHelperCore
{
	public class DelimitedTextReader : IDisposable
	{
		private DelimitedTextParser _parser;
		private string[] _currentRecord;
		private string[] _headerRecord;
		//private int _currentIndex = -1;
		private bool _doneReading;
		private bool _hasBeenRead;
		private List<string> _defaultValues;
		
		public virtual bool IgnoreMappingExceptions { get; set; }
		public virtual bool AreFieldHeadersCaseSensitive { get; set; }
		public bool FirstRowIsHeader { get; set; }
		public virtual Func<string[], bool> ShouldSkipRecord { get; set; }

		public virtual DelimitedTextParser Parser { get { return _parser; } }
		/// <summary>
		/// Gets the field headers.
		/// </summary>
		public virtual string[] FieldHeaders
		{
			get
			{
				//throwIfParserHasNotBeenRead();
				if(_headerRecord == null && FirstRowIsHeader && !_hasBeenRead)
				{
					readHeaderRecord();
				}
				return _headerRecord;
			}
		}

		/// <summary>
		/// Adds a new column with default value
		/// </summary>
		public virtual void AddColumn(string columnName)
		{
			AddColumn(columnName, null);
		}
		public virtual void AddColumn(string columnName, string defaultValue)
		{
			List<string> _hr;
			if (_headerRecord == null)
			{
				_hr = new List<string>();
			}
			else
			{
				_hr = _headerRecord.ToList();
			}
			_hr.Add(columnName);
			_headerRecord = _hr.ToArray();
			_defaultValues.Add(defaultValue);
		}

		/// <summary>
		/// Get the current record;
		/// </summary>
		public virtual string[] CurrentRecord
		{
			get
			{
				throwIfParserHasNotBeenRead();
				return _currentRecord;
			}
		}
		
		public DelimitedTextReader(TextReader reader):this(reader, ',', false)
		{
		}

		public DelimitedTextReader(TextReader reader, char delimiter) : this(reader, delimiter, false)
		{
		}

		public DelimitedTextReader(TextReader reader, char delimiter, bool skipComments)
		{
			FirstRowIsHeader = true;
			_parser = new DelimitedTextParser(reader, delimiter, skipComments);
			_defaultValues = new List<string>();
		}
				
		public virtual bool Read()
		{
			//CheckDisposed();

			if (_doneReading)
			{
				return false;
			}

			if (FirstRowIsHeader && _headerRecord == null)
			{
				readHeaderRecord();
			}

			do
			{
				_currentRecord = _parser.Read();
			}
			while (SkipRecord());

			if (_currentRecord != null && _headerRecord != null)
			{
				string _defaultVal;
				for (int i = 0; i < _headerRecord.Length; i++)
				{
					_defaultVal = _defaultValues.Count > i ? _defaultValues[i] : null;
					if (i > _currentRecord.Length - 1)
					{
						var _cr = _currentRecord.ToList();
						_cr.Add(_defaultVal);
						_currentRecord = _cr.ToArray();
					}
					else if (_currentRecord[i] == null || _currentRecord[i] == "")
					{
						_currentRecord[i] = _defaultVal;
					}
				}
			}

			_hasBeenRead = true;

			if (_currentRecord == null)
			{
				_doneReading = true;
			}

			return _currentRecord != null;
		}

		public T GetField<T>(int Index)
		{
			if(Index > -1 && Index < CurrentRecord.Length)
			{
				Type t = typeof(T);
				TypeConverter tc = TypeDescriptor.GetConverter(t);
				var value = tc.ConvertFromString(CurrentRecord[Index]);
				return (T)value;
			}
			return default(T);
		}

		public virtual TRecord GetRecord<TRecord>() where TRecord : new()
		{
			throwIfParserHasNotBeenRead();
			TRecord record;
			try
			{
				record = createRecord<TRecord>();
			}
			catch (Exception)
			{

				throw;
			}
			return record;
		}

		public virtual IEnumerable<TRecord> GetAllRecords<TRecord>() where TRecord : new()
		{
			while (Read())
			{
				TRecord record;
				try
				{
					record = createRecord<TRecord>();
				}
				catch (Exception )
				{
					throw;
				}

				yield return record;
			}
		}

		private List<PropertyMapping> _propertyMappings = new List<PropertyMapping>();
		public PropertyMapping MapProperty<TRecord>(Expression<Func<TRecord, object>> expression)
		{
			var property = GetProperty<TRecord>(expression);

			var propertyMap = new PropertyMapping(property)
			{
				MappedColumnIndex = getMaxIndex() + 1
			};

			_propertyMappings.Add(propertyMap);

			return propertyMap;
		}

		public bool SkipRecord()
		{
			if(_currentRecord == null)
			{
				return false;
			}

			if(ShouldSkipRecord != null)
			{
				return ShouldSkipRecord(_currentRecord);
			}
			return false;
		}

		public void ParseNamedIndexes()
		{

		}

		public void Dispose()
		{
			_parser.Dispose();
			_parser = null;
		}

		private void readHeaderRecord()
		{
			do
			{
				_currentRecord = _parser.Read();
			}
			while (SkipRecord());
			_headerRecord = _currentRecord;
			_currentRecord = null;
			ParseNamedIndexes();
		}

		private int getMaxIndex()
		{
			if(_propertyMappings.Count == 0)
			{
				return -1;
			}

			var indexes = new List<int>();
			if(_propertyMappings.Count > 0)
			{
				indexes.Add(_propertyMappings.Max(pm => pm.MappedColumnIndex));
			}
			return indexes.Max();
		}

		private TEntity createRecord<TEntity>() where TEntity:new()
		{
			TEntity record;
				record = new TEntity();
				//try to auto map the header names to properties on the class
				//first get all of the property infos for the type:
				AutoGeneratePropertyMappings<TEntity>();

				foreach (var pm in _propertyMappings)
				{
					int index = pm.MappedColumnIndex;
					if(pm.UseColumnName && FirstRowIsHeader)
					{
						index = Array.FindIndex(FieldHeaders, t => t.Equals(pm.MappedColumnName, StringComparison.InvariantCultureIgnoreCase));
						if(index == -1)
						{
							if (!IgnoreMappingExceptions)
							{
								//string message = string.Format("Mapping exception occurred.  The property '{0}' could not be mapped to column '{1}'", pm.PropertyInfo.Name, pm.MappedColumnName);
								throw new DelimitedTextReaderMappingException(pm.PropertyInfo.Name, pm.MappedColumnName);
							}
							index = pm.MappedColumnIndex;
						}
					}
					//var value = Convert.ChangeType(CurrentRecord[index], pm.PropertyInfo.PropertyType);
					if(pm.GetTypeConverter() != null)
					{
						var value = pm.GetTypeConverter().ConvertFromString(CurrentRecord[index]);
						pm.PropertyInfo.SetValue(record, value);
					}                    
				}
				
				return record;
			
		}

		//private Dictionary<string, PropertyInfo> _propertyMappings = new Dictionary<string, PropertyInfo>();
		private void AutoGeneratePropertyMappings<TRecord>()
		{
			if (_propertyMappings.Count == 0)
			{
				var properties = typeof(TRecord).GetProperties().Where(x => x.PropertyType.Module.ScopeName == "System.Private.CoreLib.dll").ToArray();
				//var properties = typeof(TRecord).GetProperties();
				if (FirstRowIsHeader)
				{
					AutoGeneratePropertyMappingsByName<TRecord>(properties);
					//get the propertied that were not mapped
					var headers = FieldHeaders.ToList<string>();                    
					var mappedNames = _propertyMappings.Select(n => n.MappedColumnName).ToList();
					foreach (var property in properties)
					{                        
						var existingPM = _propertyMappings.Where(m => m.PropertyInfo == property).FirstOrDefault();
						if (existingPM != null)
						{
							continue;
						}

						for (int i = 0; i < FieldHeaders.Length; i++)
						{
							if (mappedNames.Contains(FieldHeaders[i]))
							{
								continue;
							}

							PropertyMapping pm = new PropertyMapping(property)
							{
								MappedColumnIndex = i,
								MappedColumnName = FieldHeaders[i],
							};

							mappedNames.Add(FieldHeaders[i]);
							_propertyMappings.Add(pm);
							break;
						}
					}
				}
				else
				{
					AutoGeneratePropertyMappingsByIndex<TRecord>(properties);
				}
			}
		}

		private void AutoGeneratePropertyMappingsByIndex<TRecord>(PropertyInfo[] properties)
		{
			
			foreach (var property in properties)
			{
				int index = getMaxIndex() + 1;

				if (index < CurrentRecord.Length)
				{
					PropertyMapping pm = new PropertyMapping(property)
					{
						MappedColumnIndex = index,
						MappedColumnName = FirstRowIsHeader ? FieldHeaders[index] : string.Empty
					};

					_propertyMappings.Add(pm);
				}
			}
		}

		private void AutoGeneratePropertyMappingsByName<TRecord>(PropertyInfo[] properties)
		{            
			foreach (var property in properties)
			{
				
				int index = Array.FindIndex(FieldHeaders, t => t.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

				if (index == -1)
				{                        
					continue;
				}
				PropertyMapping pm = new PropertyMapping(property)
				{
					MappedColumnIndex = index,
					MappedColumnName = FieldHeaders[index]
				};

				_propertyMappings.Add(pm);
			}
		}

		private PropertyInfo GetProperty<T>( string propertyName)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			if (!AreFieldHeadersCaseSensitive)
			{
				flags = flags | BindingFlags.IgnoreCase;
			}
			PropertyInfo pi = typeof(T).GetProperty(propertyName, flags);
			return pi;
		}

		private void throwIfParserHasNotBeenRead()
		{
			if (!_hasBeenRead)
			{
				throw new Exception("Read must be invoked before data can bee accessed.");
			}
		}

		private PropertyInfo GetProperty<TRecord>(Expression<Func<TRecord, object>> expression)
		{
			var member = GetMemberExpression(expression).Member;
			var property = member as PropertyInfo;
			if(property == null)
			{
				throw new Exception(string.Format("'{0}' is not a property."));
			}

			return property;
		}

		private MemberExpression GetMemberExpression<TModel, T>(Expression<Func<TModel, T>> expression)
		{
			// This method was taken from FluentNHibernate.Utils.ReflectionHelper.cs and modified.
			// http://fluentnhibernate.org/

			MemberExpression memberExpression = null;
			if (expression.Body.NodeType == ExpressionType.Convert)
			{
				var body = (UnaryExpression)expression.Body;
				memberExpression = body.Operand as MemberExpression;
			}
			else if (expression.Body.NodeType == ExpressionType.MemberAccess)
			{
				memberExpression = expression.Body as MemberExpression;
			}

			if (memberExpression == null)
			{
				throw new ArgumentException("Not a member access", "expression");
			}

			return memberExpression;
		}
	}

	public class PropertyMapping
	{
		private TypeConverter _typeConverter;
		public PropertyInfo PropertyInfo { get; set; }
		public string MappedColumnName { get; set; }
		public int MappedColumnIndex { get; set; }
		public bool UseColumnName { get; set; }

		public PropertyMapping ColumnIndex(int index)
		{
			MappedColumnIndex = index;
			return this;
		}

		public PropertyMapping TypeConverter(TypeConverter converter)
		{
			_typeConverter = converter;
			return this;
		}

		public TypeConverter GetTypeConverter()
		{
			return _typeConverter;
		}

		public PropertyMapping ColumnName(string name)
		{
			MappedColumnName = name;
			UseColumnName = true;
			return this;
		} 

		public PropertyMapping(PropertyInfo property)
		{
			PropertyInfo = property;
			_typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
		}
	}

	public class DateTimeConverter: TypeConverter
	{
		public string Format { get; set; }

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value == null)
			{
				return base.ConvertFrom(context, culture, value);
			}

			var text = (string)value;
			if (text.Trim().Length == 0)
			{
				return DateTime.MinValue;
			}

			return string.IsNullOrEmpty(this.Format)
				? DateTime.Parse(text, CultureInfo.InvariantCulture)
				: DateTime.ParseExact(text, this.Format, CultureInfo.InvariantCulture);

		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}
	}

}
