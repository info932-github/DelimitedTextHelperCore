using System;
using System.IO;
using System.Linq;
using Xunit;
using DelimitedTextHelperCore;

namespace DelimitedTextParserTest
{
    public class DelimitedTextReaderTests
    {
        [Fact]
        public void TestGetRecordsGeneric()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Write("value2,200,false,\"1/1/2016\", 67.52\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                var records = dtReader.GetAllRecords<TestRecord>().ToList();

                Assert.Equal(2, records.Count);

                TestRecord trecord = records[0];
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);

                trecord = records[1];
                Assert.NotNull(trecord);
                Assert.Equal("value2", trecord.Field1);
                Assert.Equal(200, trecord.Field2);
                Assert.False(trecord.Field3);
                Assert.Equal(DateTime.Parse("1/1/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(67.52M, trecord.Field5);
            }

                
        }

        [Fact]
        public void TestReaderGetRecord()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using(var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderGetRecordCustomFieldsDefaultValues()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using(var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = false;
                dtReader.ShouldSkipRecord = (row => row[0] == "Field1");
                dtReader.AddColumn("F1");
                dtReader.AddColumn("F2");
                dtReader.AddColumn("F3");
                dtReader.AddColumn("F4");
                dtReader.AddColumn("F5");
                dtReader.AddColumn("F6");
                dtReader.AddColumn("F7", "defaultVal");

                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
                Assert.Equal("", trecord.Field6);
                Assert.Equal("defaultVal", trecord.Field7);
                Assert.Equal("F1", dtReader.FieldHeaders[0]);
            }
        }

        [Fact]
        public void TestReaderGetRecordCustomFieldsFewerThanActualColumns()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using(var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = false;
                dtReader.ShouldSkipRecord = (row => row[0] == "Field1");
                dtReader.AddColumn("F1");
                dtReader.AddColumn("F2");
                dtReader.AddColumn("F3");
                dtReader.AddColumn("F4");

                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
            }
        }
        [Fact]
        public void TestReaderGetField()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                Assert.Equal("value1", dtReader.GetField<string>(0));
                Assert.Equal(100, dtReader.GetField<int>(1));
                Assert.True(dtReader.GetField<bool>(2));
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), dtReader.GetField<DateTime>(3).ToShortDateString());
                Assert.Equal(25.76M, dtReader.GetField<decimal>(4));
            }
        }

        [Fact]
        public void TestReaderGetRecordSkipComments()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader, ',', true))
            {
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("#value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderGetRecordShouldSkipRecord()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("HDR,AAAA11111XXX000-FFF\r]n");
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.ShouldSkipRecord = (row => row[0] == "HDR" || row[0] == "TRL");
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderGetRecordWithoutFieldHeadersOrPropertyMappings()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = false;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderGetRecordWithoutFieldHeadersOrPropertyMappingsNestedObject()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = false;
                dtReader.Read();
                TestRecordData trecord = dtReader.GetRecord<TestRecordData>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Data1);
                Assert.Equal(100, trecord.Data2);
                Assert.True(trecord.Data4);
            }
        }

        [Fact]
        public void TestReaderPipeDelimitedGetRecord()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader, '|'))
            {
                writer.Write("Field1|Field2|Field3|Field4|Field5\r\n");
                writer.Write("value1|100|true|\"12/31/2016\"| 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderGetRecordPropertiesNotInFields()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2,Field3,Field5\r\n");
                writer.Write("value1,100,true, 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.MinValue.ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderGetRecordPropertiesOutnumberFields()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2\r\n");
                writer.Write("value1,100\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
            }
        }

        [Fact]
        public void TestReaderGetRecordPropertiesNotMappedInFields()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("F1,Field2,F3,Field4,F5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderMapProperties()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader, '|'))
            {
                writer.Write("X1|X2|X3|X4|X5\r\n");
                writer.Write("value1|100|true|\"12/31/2016\"| 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.MapProperty<TestRecord>(m => m.Field1).MappedColumnIndex = 0;
                dtReader.MapProperty<TestRecord>(m => m.Field4).MappedColumnIndex = 3;
                dtReader.MapProperty<TestRecord>(m => m.Field2).MappedColumnIndex = 1;
                dtReader.MapProperty<TestRecord>(m => m.Field5).MappedColumnIndex = 4;
                dtReader.MapProperty<TestRecord>(m => m.Field3).MappedColumnIndex = 2;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderMapPropertiesByName()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader, '|'))
            {
                writer.Write("X1|X2|X3|X4|X5\r\n");
                writer.Write("value1|100|true|\"12/31/2016\"| 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.MapProperty<TestRecord>(m => m.Field1).ColumnName("X1");
                dtReader.MapProperty<TestRecord>(m => m.Field4).ColumnName("X4");
                dtReader.MapProperty<TestRecord>(m => m.Field2).ColumnName("X2");
                dtReader.MapProperty<TestRecord>(m => m.Field5).ColumnName("X5");
                dtReader.MapProperty<TestRecord>(m => m.Field3).ColumnName("X3");
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderMapPropertiesByUnknownName()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader, '|'))
            {
                writer.Write("X1|X2|X3|X4|X5\r\n");
                writer.Write("value1|100|true|\"12/31/2016\"| 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.MapProperty<TestRecord>(m => m.Field1).ColumnName("X1");
                dtReader.MapProperty<TestRecord>(m => m.Field4).ColumnName("FOO");
                dtReader.MapProperty<TestRecord>(m => m.Field2).ColumnName("X2");
                dtReader.MapProperty<TestRecord>(m => m.Field5).ColumnName("X5");
                dtReader.MapProperty<TestRecord>(m => m.Field3).ColumnName("X3");
                dtReader.Read();
                TestRecord trecord;
                Assert.Throws<DelimitedTextReaderMappingException>(() => trecord = dtReader.GetRecord<TestRecord>());                
            }
        }

        [Fact]
        public void TestReaderMapPropertiesWithFunkyDate()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("Field1,Field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"20161231\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.MapProperty<TestRecord>(m => m.Field1).ColumnIndex(0);
                dtReader.MapProperty<TestRecord>(m => m.Field2).ColumnIndex(1);
                dtReader.MapProperty<TestRecord>(m => m.Field3).ColumnIndex(2);
                dtReader.MapProperty<TestRecord>(m => m.Field4).ColumnIndex(3).TypeConverter(new DateTimeConverter() { Format = "yyyyMMdd" });
                dtReader.MapProperty<TestRecord>(m => m.Field5).ColumnIndex(4);
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void TestReaderMapPropertiesWitoutHeaderRow()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader, '|'))
            {                
                writer.Write("value1|100|true|\"12/31/2016\"| 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = false;
                dtReader.MapProperty<TestRecord>(m => m.Field1).MappedColumnIndex = 0;
                dtReader.MapProperty<TestRecord>(m => m.Field4).MappedColumnIndex = 3;
                dtReader.MapProperty<TestRecord>(m => m.Field2).MappedColumnIndex = 1;
                dtReader.MapProperty<TestRecord>(m => m.Field5).MappedColumnIndex = 4;
                dtReader.MapProperty<TestRecord>(m => m.Field3).MappedColumnIndex = 2;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        [Fact]
        public void GetRecordCaseInsensitiveTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var dtReader = new DelimitedTextReader(reader))
            {
                writer.Write("field1,field2,Field3,Field4,Field5\r\n");
                writer.Write("value1,100,true,\"12/31/2016\", 25.76\r\n");
                writer.Flush();
                stream.Position = 0;

                dtReader.FirstRowIsHeader = true;
                dtReader.Read();
                TestRecord trecord = dtReader.GetRecord<TestRecord>();
                Assert.NotNull(trecord);
                Assert.Equal("value1", trecord.Field1);
                Assert.Equal(100, trecord.Field2);
                Assert.True(trecord.Field3);
                Assert.Equal(DateTime.Parse("12/31/2016").ToShortDateString(), trecord.Field4.ToShortDateString());
                Assert.Equal(25.76M, trecord.Field5);
            }
        }

        private class TestRecord
        {
            public string Field1 { get; set; }
            public int Field2 { get; set; }
            public bool Field3 { get; set; }
            public DateTime Field4 { get; set; }
            public decimal Field5 { get; set; }
            public string Field6 { get; set; }
            public string Field7 { get; set; }
        }

        private class TestRecordData
        {
            public string Data1 { get; set; }
            public int Data2 { get; set; }
            public TestRecord Data3{get;set;}
            public bool Data4 { get; set; }
        }

    }
}
