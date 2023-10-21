using System;
using System.Collections.Generic;

namespace Meow.Math.Graph.Struct.Comparer
{
    public class UndirectedEdgeEqualityComparer<NodeType> : IEqualityComparer<Tuple<NodeType, NodeType, int>> where NodeType : IEquatable<NodeType> 
    {
        public bool Equals(Tuple<NodeType, NodeType, int> x, Tuple<NodeType, NodeType, int> y) => x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2);
        public int GetHashCode(Tuple<NodeType, NodeType, int> obj) => obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
    }
}
