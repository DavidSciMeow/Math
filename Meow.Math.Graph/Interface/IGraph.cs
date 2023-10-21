using Meow.Math.Graph.Struct;
using System;
using System.Collections.Generic;

namespace Meow.Math.Graph.Interface
{
    /// <summary>
    /// 一般图接口<br/>General Graph Interface
    /// </summary>
    /// <typeparam name="NodeType">节点类型<br/>NodeType</typeparam>
    public interface IGraph<NodeType> : ISearchable<NodeType> where NodeType : IEquatable<NodeType>
    {
        /// <summary>
        /// 是否为无向图<br/> is Graph UnDirected or not
        /// </summary>
        bool UnDirected { get; }
        /// <summary>
        /// 是否为无权图<br/> is Graph UnWeighted or not
        /// </summary>
        bool UnWeighted { get; }

        /// <summary>
        /// 添加节点(根据节点名)<br/>Add Node (by Node Id)
        /// </summary>
        /// <param name="id">节点识别号<br/>Node Id</param>
        /// <returns>节点是否添加<br/>Node added or not</returns>
        bool Add(NodeType id);
        /// <summary>
        /// 移除节点(根据节点名)<br/>Del Node (by Node Id)
        /// </summary>
        /// <param name="id">节点识别号<br/>Node Id</param>
        /// <returns>节点是否移除<br/>Node deleted or not</returns>
        bool Remove(NodeType id);
        /// <summary>
        /// 判定节点是否存在(根据节点名)<br/>Decided a Node Exist (by Node Id)
        /// </summary>
        /// <param name="id">节点识别号<br/>Node Id</param>
        /// <returns>节点是否存在<br/>Node Exist or not</returns>
        bool Exist(NodeType id);
        /// <summary>
        /// 获取某个节点<br/>Get a Node
        /// </summary>
        /// <param name="id">节点识别号<br/>Node Id</param>
        /// <returns>一个一般节点<br/>a Plain Node</returns>
        Node<NodeType> this[NodeType id] { get; }
        /// <summary>
        /// 链接(双向)节点<br/>Link (Undirect) Node
        /// </summary>
        /// <param name="node1">节点1<br/>Node1</param>
        /// <param name="node2">节点2<br/>Node2</param>
        /// <param name="weight">边权重<br/>Edge Weight</param>
        /// <returns>是否链接成功<br/>Linked Edge or not</returns>
        bool Link(NodeType node1, NodeType node2, int weight = 1);
        /// <summary>
        /// 链接(单向)节点<br/>Link (Directed) Node
        /// </summary>
        /// <param name="node1">节点1<br/>Node1</param>
        /// <param name="node2">节点2<br/>Node2</param>
        /// <param name="weight">边权重<br/>Edge Weight</param>
        /// <returns>是否链接成功<br/>Linked Edge or not</returns>
        bool LinkTo(NodeType node1, NodeType node2, int weight = 1);
        /// <summary>
        /// 获取当前图内的一条路径<br/>Get a Total Weight of a path.
        /// <para>时间复杂度(Time Complexity) :: <i><b><see langword="O(n)" /></b></i></para>
        /// </summary>
        /// <param name="nodelist">一整条路径<br/>A Whole Path</param>
        /// <returns>当前路的总权重<br/>the Total Weight</returns>
        /// <exception cref="NodeNotExistException"></exception>
        int PathWeight(NodeType[] nodelist);
    }
}
