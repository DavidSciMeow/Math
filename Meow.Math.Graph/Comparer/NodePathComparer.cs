using System;
using System.Collections.Generic;

namespace Meow.Math.Graph.Struct.Comparer
{
    public class NodePathComparer<NodeType> : IComparer<NodePath<NodeType>> where NodeType : IEquatable<NodeType>
    {
        public int Compare(NodePath<NodeType> x, NodePath<NodeType> y)
        {
            return y.Path.Length.CompareTo(x.Path.Length);
        }
    }
}
