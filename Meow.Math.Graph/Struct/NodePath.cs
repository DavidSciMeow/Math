using System;
using System.Text;

namespace Meow.Math.Graph.Struct
{
    public class NodePath<NodeType> : IEquatable<NodePath<NodeType>> where NodeType : IEquatable<NodeType>
    {
        public NodeType[] Path { get; }

        public NodePath(NodeType[] path)
        {
            Path = path;
        }
        public bool Equals(NodePath<NodeType> other)
        {
            if (other == null)
            {
                return false;
            }

            if (Path.Length != other.Path.Length)
            {
                return false;
            }

            for (int i = 0; i < Path.Length; i++)
            {
                if (!Path[i].Equals(other.Path[i]))
                {
                    return false;
                }
            }

            return true;
        }
        public override int GetHashCode()
        {
            int hash = 19;
            foreach (var node in Path)
            {
                hash = hash * 31 + node.GetHashCode();
            }
            return hash;
        }
        public bool Contains(NodePath<NodeType> other)
        {
            if (other.Path.Length > Path.Length)
            {
                return false;
            }

            for (int i = 0; i <= Path.Length - other.Path.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < other.Path.Length; j++)
                {
                    if (!Path[i + j].Equals(other.Path[j]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return true;
                }
            }

            return false;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var i in Path)
            {
                sb.Append($" [{i}] ");
            }
            return sb.ToString();
        }
    }
}
