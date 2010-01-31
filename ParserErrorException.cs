using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jsonm
{
    public class ParserErrorException : Exception
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public int Length { get; set; }

        public ParserErrorException(int line, int column, int length, string message)
            : base(message)
        {
            Line = line;
            Column = column;
            Length = length;
        }
    }
}
