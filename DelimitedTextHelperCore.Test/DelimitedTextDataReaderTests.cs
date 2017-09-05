using DelimitedTextHelperCore;
using System;
using System.Data;
using System.IO;
using Xunit;

namespace DelimitedTextHelperCore.Test
{
    public class DelimitedTextDataReaderTests
    {
        /*
        0  BoolField
        1  ByteField
        2  CharField
        3  DateTimeField
        4  DecimalField
        5  DoubleField
        6  FloatField
        7  GuidField
        8  Int16Field
        9  Int32Field
        10 Int64Field
        11 StringField
        */
        private string headerLine = "BoolField,ByteField,CharField,DateTimeField,DecimalField,DoubleField,FloatField,GuidField,Int16Field,Int32Field,Int64Field,StringField,NullField,NullField2,StringField2\r\n";
        private string dataLine1 = string.Format("true,255,c,01/01/2016,123.4567,890.12,34.56,3E11803C-AB25-454C-953E-D940E4FCF655,{0},{1},{2},\"Son of a beach!\",\"\",,\"End\"r\n", Int16.MaxValue.ToString(), Int32.MaxValue.ToString(), Int64.MaxValue.ToString());

        [Fact]
        public void TestReaderCreation()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    Assert.NotNull(dataReader);
                    Assert.False(dataReader.IsClosed);
                }
            }
        }

        [Fact]
        public void TestReaderRead()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Write(dataLine1.Replace("Son of a beach!", "Son of a gun!"));
                writer.Write(dataLine1.Replace("Son of a beach!", "Monkey's Uncle!"));
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    Assert.NotNull(dataReader);
                    Assert.False(dataReader.IsClosed);
                    Assert.True(dataReader.Read(), "1st read failed");
                    Assert.Equal("Son of a beach!", dataReader["StringField"]);
                    Assert.True(dataReader.Read(), "2nd read failed");
                    Assert.Equal("Son of a gun!", dataReader["StringField"]);
                    Assert.True(dataReader.Read(), "3rd read failed");
                    Assert.Equal("Monkey's Uncle!", dataReader["StringField"]);
                    Assert.False(dataReader.Read(), "last read failed");
                }
            }
        }

        [Fact]
        public void TestDataTableLoadsReader()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Write(dataLine1.Replace("Son of a beach!", "Son of a gun!"));
                writer.Write(dataLine1.Replace("Son of a beach!", "Monkey's Uncle!"));
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    Assert.NotNull(dataReader);
                    Assert.False(dataReader.IsClosed);

                    DataTable dt = new DataTable();
                    dt.Load(dataReader);
                    Assert.Equal(3, dt.Rows.Count);
                    Assert.Equal("Son of a beach!", dt.Rows[0]["StringField"]);
                    Assert.Equal("Son of a gun!", dt.Rows[1]["StringField"]);
                    Assert.Equal("Monkey's Uncle!", dt.Rows[2]["StringField"]);
                }
            }
        }

        [Fact]
        public void TestReaderDispose()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            {

                var dtReader = new DelimitedTextReader(reader);
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal("Son of a beach!", dataReader["StringField"]);
                }

                dtReader.Dispose();
                Assert.Null(dtReader.Parser);
            }
        }

        [Fact]
        public void TestReaderThisByName()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal("Son of a beach!", dataReader["StringField"]);
                }
            }
        }

        [Fact]
        public void TestReaderThisByIndex()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal("Son of a beach!", dataReader[11]);
                }
            }
        }

        [Fact]
        public void TestFieldCount()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(15, dataReader.FieldCount);
                }
            }
        }

        [Fact]
        public void TestIsClosed()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.False(dataReader.IsClosed, "data reader should not be closed yet.");
                    dataReader.Read();
                    Assert.True(dataReader.IsClosed, "data reader should be closed but is not.");
                }
            }
        }

        [Fact]
        public void TestGetBoolean()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.True(dataReader.GetBoolean(0), "Boolean value should be true.");
                }
            }
        }

        [Fact]
        public void TestGetByte()
        {
            byte b = Byte.Parse("255");
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(b, dataReader.GetByte(1));
                }
            }
        }

        [Fact]
        public void TestGetChar()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal('c', dataReader.GetChar(2));
                }
            }
        }

        [Fact]
        public void TestGetDateTime()
        {
            var datetime = DateTime.Parse("01/01/2016");
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(datetime, dataReader.GetDateTime(3));
                }
            }
        }

        [Fact]
        public void TestGetDecimal()
        {
            decimal dec = 123.4567M;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(dec, dataReader.GetDecimal(4));
                }
            }
        }

        [Fact]
        public void TestGetDouble()
        {
            double dbl = 890.12D;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(dbl, dataReader.GetDouble(5));
                }
            }
        }

        [Fact]
        public void TestGetFloat()
        {
            float flt = 34.56F;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(flt, dataReader.GetFloat(6));
                }
            }
        }

        [Fact]
        public void TestGetGuid()
        {
            Guid guid = Guid.Parse("3E11803C-AB25-454C-953E-D940E4FCF655");
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(guid, dataReader.GetGuid(7));
                }
            }
        }

        [Fact]
        public void TestGetInt16()
        {
            Int16 integer16 = Int16.MaxValue;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(integer16, dataReader.GetInt16(8));
                }
            }
        }

        [Fact]
        public void TestGetInt32()
        {
            Int32 integer32 = Int32.MaxValue;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(integer32, dataReader.GetInt32(9));
                }
            }
        }

        [Fact]
        public void TestGetInt64()
        {
            Int64 integer64 = Int64.MaxValue;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(integer64, dataReader.GetInt64(10));
                }
            }
        }

        [Fact]
        public void TestGetName()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal("StringField", dataReader.GetName(11));
                }
            }
        }

        [Fact]
        public void TestGetOrdinal()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal(11, dataReader.GetOrdinal("StringField"));
                }
            }
        }

        [Fact]
        public void TestGetSchemaTable()
        {

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    DataTable dt = dataReader.GetSchemaTable();
                    Assert.NotNull(dt);
                    Assert.Equal(15, dt.Rows.Count);
                    Assert.Equal("StringField", dt.Rows[11][0].ToString());
                }
            }
        }

        [Fact]
        public void TestGetSchemaTableNotRead()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    DataTable dt = dataReader.GetSchemaTable();
                    Assert.NotNull(dt);
                    Assert.Equal(15, dt.Rows.Count);
                    Assert.Equal("StringField", dt.Rows[11][0].ToString());
                }
            }
        }

        [Fact]
        public void TestGetString()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.Equal("Son of a beach!", dataReader.GetString(11));
                }
            }
        }

        [Fact]
        public void TestIsDbNull()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write(headerLine);
                writer.Write(dataLine1);
                writer.Flush();
                stream.Position = 0;

                using (var dataReader = new DelimitedTextDataReader(dtReader))
                {
                    dataReader.Read();
                    Assert.True(dataReader.IsDBNull(12));
                    Assert.True(dataReader.IsDBNull(13));
                }
            }
        }
    }
}
