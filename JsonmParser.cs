//-----------------------------------------------------------------------
// <copyright file="JsonmParser.cs" company="Charlie Robbins">
//     Copyright (c) Charlie Robbins.  All rights reserved.
// </copyright>
// <license>
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </license>
// <summary>Contains the JsonmParser class.</summary>
//-----------------------------------------------------------------------

namespace Jsonm
{
    using System;
    using System.Collections.Generic;
    using System.Dataflow;
    using System.IO;
    using System.Linq;
    using Jsonm.Extensions;
    using Microsoft.M;

    /// <summary>
    /// An JSON parser using MGrammar and MGraph.
    /// </summary>
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
                
                // Remark: Alternate parser generation approach; dynamic compilation notably slower
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
                ////    grammarParser.GraphBuilder = new NodeGraphBuilder();
                ////}
            }
            catch (Exception ex)
            {
                // TODO: Break and post some exception here...
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonmParser"/> class.
        /// </summary>
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
        /// Parses the JSON source text located at the specified source URI
        /// into an appropriate dynamic object.
        /// </summary>
        /// <param name="sourceUri">The JSON source URI.</param>
        /// <returns>A dynamic object representing the JSON source text.</returns>
        public JsonmObject Parse(Uri sourceUri)
        {
            string sourceText = File.OpenText(sourceUri.AbsolutePath).ReadToEnd();
            return this.Parse(sourceText);
        }

        /// <summary>
        /// Parses the JSON source text into an appropriate dynamic object.
        /// </summary>
        /// <param name="sourceText">The JSON source text.</param>
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

        /// <summary>
        /// Parses a JSON object into an appropriate dynamic object.
        /// </summary>
        /// <param name="objectNode">The object node.</param>
        /// <returns>The dynamic object representing the JSON structure.</returns>
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

        /// <summary>
        /// Parses a JSON key-value pair into a Tuple.
        /// </summary>
        /// <param name="pairNode">The key-value pair node.</param>
        /// <returns>The Tuple representing the key-value pair.</returns>
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

        /// <summary>
        /// Parses a JSON value into the appropriate object.
        /// </summary>
        /// <param name="valueNode">The value node.</param>
        /// <returns>The object representing the JSON value</returns>
        private object ParseValue(Node valueNode)
        {
            return this.valueParsers[valueNode.Brand.Text](valueNode);
        }

        /// <summary>
        /// Recursively parses a JSON array into the appropriate JsonmArray.
        /// </summary>
        /// <param name="arrayNode">The array node.</param>
        /// <param name="array">The array to add values to.</param>
        /// <returns>The array representing the JSON array.</returns>
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

        /// <summary>
        /// Parses a JSON primitive into an appropriate object.
        /// </summary>
        /// <param name="primitiveNode">The primitive node.</param>
        /// <returns>The object representing the JSON primitive.</returns>
        private object ParsePrimitive(Node primitiveNode)
        {
            return this.primitiveParsers[(string)primitiveNode.Edges.FirstAtomicValue()]();
        }

        /// <summary>
        /// Parses a JSON number into an appropriate CLR double.
        /// </summary>
        /// <param name="numberNode">The number node.</param>
        /// <returns>The object representing the JSON primitive.</returns>
        private double ParseNumber(Node numberNode)
        {
            return double.Parse((string)numberNode.Edges.FirstAtomicValue());
        }

        /// <summary>
        /// Parses a JSON string into an appropriate CLR string.
        /// </summary>
        /// <param name="stringNode">The string node.</param>
        /// <returns>The object representing the JSON primitive.</returns>
        private string ParseString(Node stringNode)
        {
            return stringNode.Edges.Count > 0 ? ((string)stringNode.Edges.FirstAtomicValue()) : string.Empty;
        }
    }
}
