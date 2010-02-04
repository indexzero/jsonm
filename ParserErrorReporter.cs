using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dataflow;

namespace Jsonm
{
    public class ParserErrorReporter : ErrorReporter
    {
        public List<ErrorInformation> Errors { get; set; }

        public ParserErrorReporter()
        {
            Errors = new List<ErrorInformation>();
        }

        protected override void OnError(ErrorInformation errorInformation)
        {
            string msg = string.Format(errorInformation.Message, errorInformation.Arguments.ToArray());

            throw new ParserErrorException(errorInformation.Location.Span.Start.Line,
                errorInformation.Location.Span.Start.Column,
                errorInformation.Location.Span.Length,
                msg);

            //throw new FormatException(
            //    string.Format("Syntax error at [{0}, {1}]: {2}",
            //    errorInformation.Location.Span.Start.Line,
            //    errorInformation.Location.Span.Start.Column,
            //    msg));
        }
    }
}
