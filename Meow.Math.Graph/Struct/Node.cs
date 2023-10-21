using Meow.Math.Graph.ErrorList;
using Meow.Math.Graph.Interface;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Meow.Math.Graph.Struct
{
    /// <summary>
    /// 一般节点<br/>Node
    /// </summary>
    /// <typeparam name="NodeType">节点类型<br/>NodeType</typeparam>
    public class Node<NodeType> : IEnumerable<KeyValuePair<NodeType, int>>, IPlainNode<NodeType> where NodeType : IEquatable<NodeType>
    {
        /// <inheritdoc/>
        private Dictionary<NodeType, int> LinkedNode { get; } //内部维护连接表
        /// <summary>
        /// 节点识别号<br/>NodeId
        /// </summary>
        public NodeType Id { get; }
        /// <summary>
        /// 初始化一个一般节点<br/>Init a Node
        /// </summary>
        /// <param name="id">节点识别号<br/>NodeId</param>
        public Node(NodeType id)
        {
            Id = id;
            LinkedNode = new Dictionary<NodeType, int>();
        }
        /// <inheritdoc/>
        public bool Exist(NodeType nodeId) => LinkedNode.ContainsKey(nodeId);
        /// <inheritdoc/>
        public bool this[NodeType nodeId, int weight]
        {
            get
            {
                if (!LinkedNode.ContainsKey(nodeId))
                {
                    LinkedNode.Add(nodeId, weight);
                    return true;
                }
                return false;
            }
        }
        /// <inheritdoc/>
        public int this[NodeType nodeId]
        {
            get => LinkedNode.TryGetValue(nodeId, out var weight) ? weight : throw new NodeNotExistException();
            set => LinkedNode[nodeId] = LinkedNode.ContainsKey(nodeId) ? value : throw new NodeNotExistException();
        }
        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<NodeType, int>> GetEnumerator() => LinkedNode.GetEnumerator();
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => LinkedNode.GetEnumerator();
    }
}
