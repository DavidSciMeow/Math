using System;
using System.Collections.Generic;

namespace GraphX.Util
{
    /// <summary>
    /// 简单并查集（按秩合并 + 路径压缩），常用于 Kruskal。<br/>Simple Union-Find (union by rank + path compression) for Kruskal.
    /// </summary>
    public class UnionFind<T>
    {
        private readonly Dictionary<T, T> _parent = new();
        private readonly Dictionary<T, int> _rank = new();

        /// <summary>
        /// 创建单元素集合。<br/>Create a singleton set.
        /// </summary>
        public void MakeSet(T x)
        {
            if (!_parent.ContainsKey(x))
            {
                _parent[x] = x;
                _rank[x] = 0;
            }
        }

        /// <summary>
        /// 查找元素的代表元（带路径压缩）。<br/>Find representative of the set (with path compression).
        /// </summary>
        public T Find(T x)
        {
            if (!_parent.ContainsKey(x)) throw new ArgumentException("Element not found");
            if (!EqualityComparer<T>.Default.Equals(_parent[x], x))
            {
                _parent[x] = Find(_parent[x]);
            }
            return _parent[x];
        }

        /// <summary>
        /// 合并两个集合（按秩）。<br/>Union two sets (by rank).
        /// </summary>
        public void Union(T a, T b)
        {
            var ra = Find(a);
            var rb = Find(b);
            if (EqualityComparer<T>.Default.Equals(ra, rb)) return;
            var rA = _rank[ra];
            var rB = _rank[rb];
            if (rA < rB)
            {
                _parent[ra] = rb;
            }
            else if (rA > rB)
            {
                _parent[rb] = ra;
            }
            else
            {
                _parent[rb] = ra;
                _rank[ra] = rA + 1;
            }
        }
    }
}
