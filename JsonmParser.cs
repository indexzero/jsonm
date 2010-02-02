using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.M;
using System.Dataflow;
using System.IO;
using jsonm.Extensions;
using System.Dynamic;

namespace jsonm
{
    public class JsonmParser
    {
        /// <summary>
        /// The underlying GLR parser provided by MGrammar.
        /// </summary>
        private static readonly Parser grammarParser;

        /// <summary>
        /// A lookup table of handlers for parsing json values.
        /// </summary>
        private Dictionary<string, Func<Node, object>> valueParsers;

        /// <summary>
        /// A lookup table of handlers for parsing primitive json values.
        /// </summary>
        private Dictionary<string, Func<object>> primitiveParsers;

        /// <summary>
        /// Initializes static members of the <see cref="JsonmParser"/> class.
        /// </summary>
        static JsonmParser()
        {
            try
            {
                MImage grammar = new MImage(@"jsonm.mx");
                grammarParser = grammar.ParserFactories["jsonm.jsonm"].Create();
                grammarParser.GraphBuilder = new NodeGraphBuilder();
                
                // Remark: Alternate parser generation approach; causes grammarParser.Parse to return a SimpleNode
                ////using (var r = new StreamReader(Environment.CurrentDirectory + @"\..\..\jsonm.mg"))
                ////{
                ////    var options = new CompilerOptions
                ////    {
                ////        Sources = 
                ////        { 
                ////            new TextItem
                ////            {
                ////                Name = "I Need A Name",
                ////                Reader = r,
                ////                ContentType = TextItemType.MGrammar
                ////            }
                ////        }
                ////    };

                ////    CompilationResults results = Compiler.Compile(options);
                ////    grammarParser = results.ParserFactories["jsonm.jsonm"].Create();
                ////}

            }
            catch (Exception ex)
            {
                // TODO: Break and post some exception here...
            }
        }

        public JsonmParser()
        {
            this.valueParsers = new Dictionary<string, Func<Node, object>>()
            {
                { "Object", node => this.ParseObject(node) },
                { "Array", node => this.ParseArray(node, new JsonmArray()) },
                { "String", node => this.ParseString(node) },
                { "Number", node => this.ParseNumber(node) },
                { "Primitive", node => this.ParsePrimitive(node) }
            };

            this.primitiveParsers = new Dictionary<string, Func<object>>()
            {
                { "false", () => false },
                { "true", () => true },
                { "null", () => null }
            };
        }

        /// <summary>
        /// Parses the json source text located at the specified source URI
        /// into an appropriate dynamic object.
        /// </summary>
        /// <param name="sourceUri">The json source URI.</param>
        /// <returns>A dynamic object representing the json source text.</returns>
        public JsonmObject Parse(Uri sourceUri)
        {
            string sourceText = File.OpenText(sourceUri.AbsolutePath).ReadToEnd();
            return this.Parse(sourceText);
        }

        /// <summary>
        /// Parses the json source text into an appropriate dynamic object.
        /// </summary>
        /// <param name="sourceUri">The json source text.</param>
        /// <returns>A dynamic object representing the json source text.</returns>
        public JsonmObject Parse(string sourceText)
        {
            try
            {
                var inputText = new StringTextStream(sourceText);
                var errorReporter = new ParserErrorReporter();
                Node rootNode = (Node)grammarParser.Parse(inputText, errorReporter);
                JsonmObject obj = this.ParseObject(rootNode);
                return obj;
            }
            catch (Exception ex)
            {
                // TODO: Break and post some exception here...
            }

            return null;
        }

        private JsonmObject ParseObject(Node objectNode)
        {
            JsonmObject jsonmObject = new JsonmObject();

            List<Tuple<string, object>> keyValuePairs = objectNode
                .Edges.FindNodeWithBrand("Pairs")
                .Edges.FindNodesWithBrand("Pair")
                .Select(node => this.ParseKeyValuePair(node))
                .ToList();

            foreach (Tuple<string, object> pair in keyValuePairs)
            {
                jsonmObject.TrySetMember(
                    new DynamicDictionaryMemberBinder(pair.Item1, false),
                    pair.Item2);
            }

            return jsonmObject;
        }

        private Tuple<string, object> ParseKeyValuePair(Node pairNode)
        {
            string key = (string)pairNode
                .Edges.FindNodeWithBrand("Key")
                .Edges.First().Node
                .Edges.FirstAtomicValue();

            object value = pairNode
                .Edges.FindNodeWithBrand("Value")
                .Edges.Select(edge => this.ParseValue(edge.Node))
                .First();

            return new Tuple<string, object>(key, value);
        }

        private object ParseValue(Node valueNode)
        {
            return this.valueParsers[valueNode.Brand.Text](valueNode);
        }

        private JsonmArray ParseArray(Node arrayNode, JsonmArray array)
        {
            if (arrayNode.Edges.Count <= 0)
            {
                return array;
            }

            Node head = arrayNode.Edges.FindNodeAtLabeledEdge("Head");
            Node tail = arrayNode.Edges.FindNodeAtLabeledEdge("Tail");

            array.Add(this.ParseValue(head));
            return this.ParseArray(tail, array);
        }

        private object ParsePrimitive(Node primitiveNode)
        {
            return this.primitiveParsers[(string)primitiveNode.Edges.FirstAtomicValue()]();
        }

        private double ParseNumber(Node numberNode)
        {
            return double.Parse((string)numberNode.Edges.FirstAtomicValue());
        }

        private string ParseString(Node stringNode)
        {
            return stringNode.Edges.Count > 0 ? ((string)stringNode.Edges.FirstAtomicValue()) : string.Empty;
        }
    }
}
