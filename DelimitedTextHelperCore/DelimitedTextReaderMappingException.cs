using System;
using System.Collections.Generic;
using System.Text;

namespace DelimitedTextHelperCore
{
    public class DelimitedTextReaderMappingException: Exception
    {
        public DelimitedTextReaderMappingException(string propertyName, string mappedColumnName):
            base($"Mapping exception occurred.  The property '{propertyName}' could not be mapped to column '{mappedColumnName}'")
        {
            
        }
    }
}
