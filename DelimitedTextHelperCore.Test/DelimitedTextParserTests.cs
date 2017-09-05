using System;
using System.IO;
using Xunit;

using DelimitedTextHelperCore;

namespace DelimitedTextParserTest
{    
    public class DelimitedTextParserTests
    {
        
        /// <summary>
        /// Simple test, no quoted strings that contain delimiter
        /// </summary>
        [Fact]
        public void SimpleParserTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new DelimitedTextParser(reader))
            {
                writer.Write("1,2\r\n");
                writer.Write("3,4\r\n");
                writer.Flush();
                stream.Position = 0;

                var row = parser.Read();
                Assert.NotNull(row);

                row = parser.Read();
                Assert.NotNull(row);

                Assert.Null(parser.Read());
            }

        }

        [Fact]
        public void RowBlankLinesTest()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new DelimitedTextParser(reader))
            {
                writer.Write("1,2\r\n");
                writer.Write("\r\n");
                writer.Write("3,4\r\n");
                writer.Write("\r\n");
                writer.Write("5,6\r\n");
                writer.Flush();
                stream.Position = 0;

                var rowCount = 1;
                while (parser.Read() != null)
                {
                    Assert.Equal(rowCount, parser.LineNumber);
                    rowCount += 2;
                }
            }
        }

        [Fact]
        public void LastLineIsBlankTestSkippingComments()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var parser = new DelimitedTextParser(reader, ',', true))
            {
                writer.Write("1,2\r\n");
                writer.Write("3,4\r\n");
                writer.Write("5,6\r\n");
                writer.Write("");
                writer.Flush();
                stream.Position = 0;

                var rowCount = 1;
                while (parser.Read() != null)
                {
                    Assert.Equal(rowCount, parser.LineNumber);
                    rowCount += 1;
                }
            }
        }

        [Fact]
        public void ParseNewRecordTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine("one,two,three");
            writer.WriteLine("four,five,six");
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);

            var parser = new DelimitedTextParser(reader);

            var count = 0;
            while (parser.Read() != null)
            {
                count++;
            }

            Assert.Equal(2, count);
        }

        [Fact]
        public void ParseFieldQuotesTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine("one,\"two\",three");
            writer.WriteLine("four,\"\"\"five\"\"\",six");
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);

            var parser = new DelimitedTextParser(reader) ;

            var record = parser.Read();
            Assert.Equal("one", record[0]);
            Assert.Equal("two", record[1]);
            Assert.Equal("three", record[2]);

            record = parser.Read();
            Assert.Equal("four", record[0]);
            Assert.Equal("\"five\"", record[1]);
            Assert.Equal("six", record[2]);

            record = parser.Read();
            Assert.Null(record);
        }


        [Fact]
        public void ParseFieldQuotesWithCommaTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine("one,\"two,half\",three");
            writer.WriteLine("four,\"\"\"five\"\"\",six");
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);

            var parser = new DelimitedTextParser(reader);

            var record = parser.Read();
            Assert.Equal("one", record[0]);
            Assert.Equal("two,half", record[1]);
            Assert.Equal("three", record[2]);

            record = parser.Read();
            Assert.Equal("four", record[0]);
            Assert.Equal("\"five\"", record[1]);
            Assert.Equal("six", record[2]);

            record = parser.Read();
            Assert.Null(record);
        }

        [Fact]
        public void ParseSpacesTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine(" one , \"two three\" , four ");
            writer.WriteLine(" \" five \"\" six \"\" seven \" ");
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);

            var parser = new DelimitedTextParser(reader);

            var record = parser.Read();
            Assert.Equal(" one ", record[0]);
            Assert.Equal(" \"two three\" ", record[1]);
            Assert.Equal(" four ", record[2]);

            record = parser.Read();
            Assert.Equal(" \" five \"\" six \"\" seven \" ", record[0]);

            record = parser.Read();
            Assert.Null(record);
        }

        [Fact]
        public void CallingReadMultipleTimesAfterDoneReadingTest()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine("one,two,three");
            writer.WriteLine("four,five,six");
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);

            var parser = new DelimitedTextParser(reader);

            parser.Read();
            parser.Read();
            parser.Read();
            parser.Read();
        }

        [Fact]
        public void ParseEmptyTest()
        {
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var parser = new DelimitedTextParser(streamReader))
            {
                var record = parser.Read();
                Assert.Null(record);
            }
        }

        [Fact]
        public void ParseCrOnlyTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new DelimitedTextParser(reader))
            {
                writer.Write("\r");
                writer.Flush();
                stream.Position = 0;

                var record = parser.Read();
                Assert.Null(record);
            }
        }

        [Fact]
        public void ParseLfOnlyTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new DelimitedTextParser(reader))
            {
                writer.Write("\n");
                writer.Flush();
                stream.Position = 0;

                var record = parser.Read();
                Assert.Null(record);
            }
        }

        [Fact]
        public void ParseCrLnOnlyTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new DelimitedTextParser(reader))
            {
                writer.Write("\r\n");
                writer.Flush();
                stream.Position = 0;

                var record = parser.Read();
                Assert.Null(record);
            }
        }

        [Fact]
        public void Parse1RecordWithNoCrlfTest()
        {
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var parser = new DelimitedTextParser(streamReader))
            {
                streamWriter.Write("one,two,three");
                streamWriter.Flush();
                memoryStream.Position = 0;

                var record = parser.Read();
                Assert.NotNull(record);
                Assert.Equal(3, record.Length);
                Assert.Equal("one", record[0]);
                Assert.Equal("two", record[1]);
                Assert.Equal("three", record[2]);

                Assert.Null(parser.Read());
            }
        }

        [Fact]
        public void Parse2RecordsLastWithNoCrlfTest()
        {
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var parser = new DelimitedTextParser(streamReader))
            {
                streamWriter.WriteLine("one,two,three");
                streamWriter.Write("four,five,six");
                streamWriter.Flush();
                memoryStream.Position = 0;

                parser.Read();
                var record = parser.Read();
                Assert.NotNull(record);
                Assert.Equal(3, record.Length);
                Assert.Equal("four", record[0]);
                Assert.Equal("five", record[1]);
                Assert.Equal("six", record[2]);

                Assert.Null(parser.Read());
            }
        }

        [Fact]
        public void ParseFirstFieldIsEmptyQuotedTest()
        {
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var parser = new DelimitedTextParser(streamReader))
            {
                streamWriter.WriteLine("\"\",\"two\",\"three\"");
                streamWriter.Flush();
                memoryStream.Position = 0;

                var record = parser.Read();
                Assert.NotNull(record);
                Assert.Equal(3, record.Length);
                Assert.Equal("", record[0]);
                Assert.Equal("two", record[1]);
                Assert.Equal("three", record[2]);
            }
        }

        [Fact]
        public void ParseLastFieldIsEmptyQuotedTest()
        {
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var parser = new DelimitedTextParser(streamReader))
            {
                streamWriter.WriteLine("\"one\",\"two\",\"\"");
                streamWriter.Flush();
                memoryStream.Position = 0;

                var record = parser.Read();
                Assert.NotNull(record);
                Assert.Equal(3, record.Length);
                Assert.Equal("one", record[0]);
                Assert.Equal("two", record[1]);
                Assert.Equal("", record[2]);
            }
        }

        [Fact]
        public void ParseQuoteOnlyQuotedFieldTest()
        {
            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var parser = new DelimitedTextParser(streamReader))
            {
                streamWriter.WriteLine("\"\"\"\",\"two\",\"three\"");
                streamWriter.Flush();
                memoryStream.Position = 0;

                var record = parser.Read();
                Assert.NotNull(record);
                Assert.Equal(3, record.Length);
                Assert.Equal("\"", record[0]);
                Assert.Equal("two", record[1]);
                Assert.Equal("three", record[2]);
            }
        }

        [Fact]
        public void EmptyLastFieldTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            using (var parser = new DelimitedTextParser(reader))
            {
                writer.WriteLine("1,2,");
                writer.WriteLine("4,5,");
                writer.Flush();
                stream.Position = 0;
                
                var row = parser.Read();
                Assert.NotNull(row);
                Assert.Equal(3, row.Length);
                Assert.Equal("1", row[0]);
                Assert.Equal("2", row[1]);
                Assert.Equal("", row[2]);

                row = parser.Read();
                Assert.NotNull(row);
                Assert.Equal(3, row.Length);
                Assert.Equal("4", row[0]);
                Assert.Equal("5", row[1]);
                Assert.Equal("", row[2]);

                row = parser.Read();
                Assert.Null(row);
            }
        }

    }
}
