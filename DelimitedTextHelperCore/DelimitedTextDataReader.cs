using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelimitedTextHelperCore
{
    public class DelimitedTextDataReader: IDataReader
    {
        private DelimitedTextReader _reader;
        private bool _isDone;
        public DelimitedTextDataReader(DelimitedTextReader textReader)
        {
            this._reader = textReader;
        }

        public object this[string name]
        {
            get
            {
                return _reader.CurrentRecord[this.GetOrdinal(name)];
                
            }
        }

        public object this[int i]
        {
            get
            {
                return this.GetValue(i);
            }
        }

        public int Depth
        {
            get
            {
                return 0;
            }
        }

        public int FieldCount
        {
            get
            {
                return _reader.FieldHeaders.Length;
            }
        }

        public bool IsClosed
        {
            get
            {
                return _isDone;
            }
        }

        public int RecordsAffected
        {
            get
            {
                return -1;
            }
        }

        public void Close()
        {
            _reader.Dispose();
        }

        public void Dispose()
        {
            _reader = null;
        }

        public bool GetBoolean(int i)
        {
            return _reader.GetField<bool>(i);
        }

        public byte GetByte(int i)
        {
            return _reader.GetField<byte>(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return _reader.GetField<char>(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return "System.String";
        }

        public DateTime GetDateTime(int i)
        {
            return _reader.GetField<DateTime>(i);
        }

        public decimal GetDecimal(int i)
        {
            return _reader.GetField<decimal>(i);
        }

        public double GetDouble(int i)
        {
            return _reader.GetField<double>(i);
        }

        public Type GetFieldType(int i)
        {
            return typeof(string);
        }

        public float GetFloat(int i)
        {
            return _reader.GetField<float>(i);
        }

        public Guid GetGuid(int i)
        {
            return _reader.GetField<Guid>(i);
        }

        public short GetInt16(int i)
        {
            return _reader.GetField<short>(i);
        }

        public int GetInt32(int i)
        {
            return _reader.GetField<int>(i);
        }

        public long GetInt64(int i)
        {
            return _reader.GetField<long>(i);
        }

        public string GetName(int i)
        {
            return _reader.FieldHeaders[i];
        }

        public int GetOrdinal(string name)
        {
            for (int i = 0; i < _reader.FieldHeaders.Length; i++)
            {
                if(_reader.FieldHeaders[i] == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public DataTable GetSchemaTable()
        {
            var dt = new DataTable();

            dt.Columns.Add(new DataColumn("ColumnName", typeof(string)));
            dt.Columns.Add(new DataColumn("ColumnOrdinal", typeof(int)));
            dt.Columns.Add(new DataColumn("ColumnSize", typeof(long)));
            dt.Columns.Add(new DataColumn("NumericPrecision", typeof(int)));
            dt.Columns.Add(new DataColumn("NumericScale", typeof(int)));
            dt.Columns.Add(new DataColumn("DataType", typeof(Type)));
            dt.Columns.Add(new DataColumn("ProviderType", typeof(int)));
            dt.Columns.Add(new DataColumn("IsLong", typeof(bool)));
            dt.Columns.Add(new DataColumn("AllowDBNull", typeof(bool)));
            dt.Columns.Add(new DataColumn("IsReadOnly", typeof(bool)));
            dt.Columns.Add(new DataColumn("IsRowVersion", typeof(bool)));
            dt.Columns.Add(new DataColumn("IsUnique", typeof(bool)));
            dt.Columns.Add(new DataColumn("IsKey", typeof(bool)));
            dt.Columns.Add(new DataColumn("IsAutoIncrement", typeof(bool)));
            dt.Columns.Add(new DataColumn("BaseSchemaName", typeof(string)));
            dt.Columns.Add(new DataColumn("BaseCatalogName", typeof(string)));
            dt.Columns.Add(new DataColumn("BaseTableName", typeof(string)));
            dt.Columns.Add(new DataColumn("BaseColumnName", typeof(string)));
            
            for (int i = 0; i < _reader.FieldHeaders.Length; i++)
            {
                var field = _reader.FieldHeaders[i];
                DataRow row = dt.NewRow();
                row["ColumnName"] = field;
                row["ColumnOrdinal"] = i;
                row["ColumnSize"] = 8000;
                row["NumericPrecision"] = 255;
                row["NumericScale"] = 255;
                row["DataType"] = typeof(string);
                row["ProviderType"] = 202;
                row["IsLong"] = false;
                row["AllowDBNull"] = true;
                row["IsReadOnly"] = false;
                row["IsRowVersion"] = false;
                row["IsUnique"] = false;
                row["IsKey"] = false;
                row["IsAutoIncrement"] = false;
                
                dt.Rows.Add(row);
            }
            return dt;
        }

        public string GetString(int i)
        {
            return _reader.GetField<string>(i);
        }

        public object GetValue(int i)
        {
            return _reader.GetField<string>(i);
        }

        public int GetValues(object[] values)
        {
            int length = Math.Min(_reader.FieldHeaders.Length, values.Length);
            for (int i = 0; i < length; i++)
            {
                values[i] = GetValue(i);
            }
            return length;
        }

        public bool IsDBNull(int i)
        {
            return string.IsNullOrEmpty(_reader.CurrentRecord[i]) || _reader.CurrentRecord[i].ToLower() == "null";
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            bool RC = _reader.Read();
            _isDone = !RC;
            return RC;
        }
    }
}
