using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.M;
using System.Dataflow;
using System.IO;
using jsonm.Extensions;

namespace jsonm
{
    public class JsonmParser
    {
        Parser grammarParser;

        public JsonmParser()
        {
            this.ConstructParserFromMxImage();
        }

        private void ConstructParserFromMxImage()
        {
            try
            {
                MImage grammar = new MImage(@"jsonm.mx");
                this.grammarParser = grammar.ParserFactories["jsonm.jsonm"].Create();
                this.grammarParser.GraphBuilder = new NodeGraphBuilder();
            }
            catch (Exception ex)
            {
                // TODO: Break and post some exception here...
            }
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
                Node rootNode = (Node)this.grammarParser.Parse(inputText, errorReporter);
                return this.ParseObject(rootNode);
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
                .Edges
                .FindNodeWithBrand("Pairs")
                .Edges
                .FindNodesWithBrand("Pair")
                .Select(node => this.ParseKeyValuePair(node))
                .ToList();

            return jsonmObject;
        }

        private Tuple<string, object> ParseKeyValuePair(Node pairNode)
        {
            return new Tuple<string, object>("1", "1");
        }
    }
}
