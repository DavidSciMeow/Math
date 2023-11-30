using System;
using System.Collections.Generic;

namespace Meow.Math.Graph.Struct.Comparer
{
    public class EdgeCircleEqualityComparer<NodeType> : IEqualityComparer<NodeType[]> where NodeType : IEquatable<NodeType>
    {
        public bool Equals(NodeType[] x, NodeType[] y)
        {
            for (int i = 0; i < x.Length; i++)
            {
                if (!x[i].Equals(y[i])) return false;
            }
            return true;
        }

        public int GetHashCode(NodeType[] obj)
        {
            int k = default;
            foreach (var i in obj) k ^= i.GetHashCode();
            return k;
        }
    }
}
