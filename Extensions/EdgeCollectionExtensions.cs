using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dataflow;

namespace jsonm.Extensions
{
    public static class EdgeCollectionExtensions
    {
        public static object GetAtomicValueOfNodeWithBrand(this EdgeCollection edges, string brand)
        {
            return edges.FindNodeWithBrand(brand).Edges.First().Node.AtomicValue;
        }

        public static Node FindNodeWithBrand(this EdgeCollection edges, string brand)
        {
            return edges.Where(edge => edge.Node.Brand.Text.Equals(brand)).FirstOrDefault().Node;
        }

        public static IEnumerable<Node> FindNodesWithBrand(this EdgeCollection edges, string brand)
        {
            return edges.Where(edge => edge.Node.Brand.Text.Equals(brand)).Select(edge => edge.Node);
        }

        public static object FirstAtomicValue(this EdgeCollection edges)
        {
            return edges.Where(edge => edge.Node.NodeKind == NodeKind.Atomic).First().Node.AtomicValue;
        }

        public static Node FindNodeAtLabeledEdge(this EdgeCollection edges, string label)
        {
            return edges.Where(edge => edge.Label.Text.Equals(label)).FirstOrDefault().Node;
        }
    }
}
