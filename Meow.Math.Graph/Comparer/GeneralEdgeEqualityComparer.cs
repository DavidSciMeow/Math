using System;
using System.Collections.Generic;
using System.Xml;

namespace Meow.Math.Graph.Struct.Comparer
{
    public class GeneralEdgeEqualityComparer<NodeType> : IEqualityComparer<Tuple<NodeType, NodeType, int, bool>> where NodeType : IEquatable<NodeType>
    {
        public bool Equals(Tuple<NodeType, NodeType, int, bool> x, Tuple<NodeType, NodeType, int, bool> y) => x.Item4 && y.Item4 ? (x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2)) || (x.Item2.Equals(y.Item1) && x.Item1.Equals(y.Item2)) : x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2);

        public int GetHashCode(Tuple<NodeType, NodeType, int, bool> obj) => obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
    }
}
