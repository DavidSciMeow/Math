using Meow.Math.Graph.Struct;
using System;
using System.Collections.Generic;

namespace Meow.Math.Graph.Interface
{
    /// <summary>
    /// 平凡节点定义接口<br/>PlainNode Interface
    /// </summary>
    /// <typeparam name="NodeType">节点类型</typeparam>
    public interface IPlainNode<NodeType> where NodeType : IEquatable<NodeType>
    {
        /// <summary>
        /// 节点识别号<br/>NodeId
        /// </summary>
        NodeType Id { get; }
        /// <summary>
        /// 获取(设置)连接节点 (根据节点Id)<br/>Get/Set Linked Node (by Id)
        /// </summary>
        /// <param name="nodeId">节点Id<br/>Node Id</param>
        /// <returns>节点的权重<br/>Weight of Linked Node</returns>
        int this[NodeType nodeId] { get; set; }
        /// <summary>
        /// 是否存在某链接节点<br/>if Node with specific Id Linked
        /// </summary>
        /// <param name="nodeId">节点Id<br/>Node Id</param>
        /// <returns>是否存在<br/>Exist or not</returns>
        bool Exist(NodeType nodeId);
        /// <summary>
        /// 添加邻居节点<br/>Add Neighbor Node
        /// </summary>
        /// <param name="nodeId">邻居节点<br/>Neighbor Node</param>
        /// <param name="weight">边权重<br/>Edge Weight</param>
        bool AddNeighbor(NodeType nodeId, int weight);
        /// <summary>
        /// 添加邻居节点<br/>Add Neighbor Node
        /// </summary>
        /// <param name="neighbor">邻居节点<br/>Neighbor Node</param>
        /// <param name="weight">边权重<br/>Edge Weight</param>
        bool AddNeighbor(Node<NodeType> neighbor, int weight);

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<NodeType, int>> GetEnumerator();
    }
}