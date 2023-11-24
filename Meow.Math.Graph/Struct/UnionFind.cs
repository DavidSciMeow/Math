using Meow.Math.Graph.ErrorList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Graph.Struct
{
    public interface IUnionFind<NodeType> where NodeType : IEquatable<NodeType>
    {
        bool Union(NodeType node1, NodeType node2, bool _leftJoin = true);
        bool Connected(NodeType node1, NodeType node2);
        void AddNode(NodeType node);
        NodeType Parent(NodeType node);
        int Component_Connect { get; }
    }

    public class UnionFind<NodeType> : IEnumerable<KeyValuePair<NodeType, NodeType>>, IUnionFind<NodeType> where NodeType : IEquatable<NodeType>
    {
        Dictionary<NodeType, NodeType> PT = new Dictionary<NodeType, NodeType>();
        Dictionary<NodeType, int> PW = new Dictionary<NodeType, int>();
        public int Component_Connect { get; private set; }
        public IEnumerable<KeyValuePair<NodeType, NodeType>> UnionParentTable => PT.AsEnumerable();
        public UnionFind(NodeType[] n)
        {
            Component_Connect = n.Length;
            foreach (var node in n)
            {
                PT.Add(node, node);
                PW.Add(node, 1);
            }
        }
        public UnionFind(Dictionary<NodeType, NodeType> ParentTable)
        {
            PT = ParentTable;
            Dictionary<NodeType, HashSet<NodeType>> clist = new Dictionary<NodeType, HashSet<NodeType>>();
            foreach(var i in ParentTable)
            {
                var kp = Parent(i.Key);
                if (clist.ContainsKey(kp))
                {
                    clist[kp].Add(i.Key);
                }
                else
                {
                    clist.Add(kp, new HashSet<NodeType>() { i.Key });
                }
            }
            Component_Connect = clist.Keys.Count;
        }

        public void AddNode(NodeType node) => PT.Add(node, node);
        public NodeType Parent(NodeType node)
        {
            if(!PT.ContainsKey(node)) throw new NodeNotExistException();
            while (PT[node].Equals(node)) node = PT[node];
            return node;
        }
        public bool Connected(NodeType node1, NodeType node2)
        {
            if (!PT.ContainsKey(node1) || !PT.ContainsKey(node2)) throw new NodeNotExistException();
            return Parent(node1).Equals(Parent(node2));
        }
        public bool Union(NodeType node1, NodeType node2, bool _leftJoin = true)
        {
            if (!PT.ContainsKey(node1) || !PT.ContainsKey(node2)) throw new NodeNotExistException();
            if (Component_Connect < 2) return false;
            NodeType rp1 = Parent(node1);
            NodeType rp2 = Parent(node2);
            if (rp1.Equals(rp2)) return false;
            if(_leftJoin) PT[rp1] = rp2;
            else PT[rp2] = rp1;
            Component_Connect--;
            return true;
        }

        public IEnumerator<KeyValuePair<NodeType, NodeType>> GetEnumerator()
        {
            return UnionParentTable.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)UnionParentTable).GetEnumerator();
        }
    }
}
