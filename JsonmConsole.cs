using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jsonm
{
    class JsonmConsole
    {
        static void Main(string[] args)
        {
            JsonmParser parser = new JsonmParser();
            dynamic result = parser.Parse(new Uri(@"C:\Users\Charlie\Self\GitHub\jsonm\sample.json"));
            Console.Out.WriteLine(result.Object);
        }
    }
}
