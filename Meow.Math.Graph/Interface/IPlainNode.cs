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
        /// 获取(设置)连接节点 (根据节点Id)<br/>Get/Set Linked Node (by Id)
        /// </summary>
        /// <param name="nodeId">节点Id<br/>Node Id</param>
        /// <returns>节点的权重<br/>Weight of Linked Node</returns>
        int this[NodeType nodeId] { get; set; }
        /// <summary>
        /// 添加一个链接节点<br/>Add Linked Node
        /// </summary>
        /// <param name="nodeId">节点Id<br/>Node Id</param>
        /// <param name="weight">节点的权重<br/>Weight of Linked Node</param>
        /// <returns>是否添加成功<br/>Is Node Added</returns>
        bool this[NodeType nodeId, int weight] { get; }
        /// <summary>
        /// 节点识别号<br/>NodeId
        /// </summary>
        NodeType Id { get; }
        /// <summary>
        /// 是否存在某链接节点<br/>if Node with specific Id Linked
        /// </summary>
        /// <param name="nodeId">节点Id<br/>Node Id</param>
        /// <returns>是否存在<br/>Exist or not</returns>
        bool Exist(NodeType nodeId);
        /// <inheritdoc/>
        IEnumerator<KeyValuePair<NodeType, int>> GetEnumerator();
    }
}