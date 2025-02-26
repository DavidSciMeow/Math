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
        private readonly object lockobj = new object();
        /// <inheritdoc/>
        private Dictionary<NodeType, int> LinkedNode { get; } //内部维护连接表
        /// <summary>
        /// 节点识别号<br/>NodeId
        /// </summary>
        public NodeType Id { get; }
        /// <summary>
        /// 父节点<br/>Parent Node (for tree)
        /// </summary>
        public Node<NodeType> Parent { get; private set; }

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
        public bool Exist(NodeType nodeId)
        {
            lock (lockobj)
            {
                return LinkedNode.ContainsKey(nodeId);
            }
        }

        /// <inheritdoc/>
        public int this[NodeType nodeId]
        {
            get
            {
                lock (lockobj)
                {
                    return LinkedNode.TryGetValue(nodeId, out var weight) ? weight : throw new NodeNotExistException();
                }
            }
            set
            {
                lock (lockobj)
                {
                    LinkedNode[nodeId] = LinkedNode.ContainsKey(nodeId) ? value : throw new NodeNotExistException();
                }
            }
        }

        /// <summary>
        /// 添加邻居节点<br/>Add Neighbor Node
        /// </summary>
        /// <param name="neighbor">邻居节点<br/>Neighbor Node</param>
        /// <param name="weight">边权重<br/>Edge Weight</param>
        public bool AddNeighbor(Node<NodeType> neighbor, int weight)
        {
            lock (lockobj)
            {
                if (!LinkedNode.ContainsKey(neighbor.Id))
                {
                    LinkedNode.Add(neighbor.Id, weight);
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 添加邻居节点<br/>Add Neighbor Node
        /// </summary>
        /// <param name="nodeId">邻居节点<br/>Neighbor Node</param>
        /// <param name="weight">边权重<br/>Edge Weight</param>
        public bool AddNeighbor(NodeType nodeId, int weight)
        {
            lock (lockobj)
            {
                lock (lockobj)
                {
                    if (!LinkedNode.ContainsKey(nodeId))
                    {
                        LinkedNode.Add(nodeId, weight);
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<NodeType, int>> GetEnumerator() => LinkedNode.GetEnumerator();
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => LinkedNode.GetEnumerator();
    }
}