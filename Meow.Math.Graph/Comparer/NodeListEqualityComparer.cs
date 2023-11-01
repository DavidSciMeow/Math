using System;
using System.Collections.Generic;

namespace Meow.Math.Graph.Struct.Comparer
{
    public class NodeListEqualityComparer<NodeType> : IEqualityComparer<List<NodeType>> where NodeType : IEquatable<NodeType>
    {
        public bool Equals(List<NodeType> x, List<NodeType> y)
        {
            if (x.Count != y.Count) return false;
            for (int i = 0; i < x.Count; i++) if (!x[i].Equals(y[i])) return false;
            return true;
        }

        public int GetHashCode(List<NodeType> obj)
        {
            int k = 0;
            foreach(var i in obj) k ^= i.GetHashCode();
            return k;
        }
    }
}
