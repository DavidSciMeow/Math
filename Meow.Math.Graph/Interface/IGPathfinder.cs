using Meow.Math.Graph.ErrorList;
using System;
using System.Collections.Generic;

namespace Meow.Math.Graph.Interface
{
    /// <summary>
    /// 一般路径搜寻算法<br/>General Pathfinder
    /// </summary>
    /// <typeparam name="NodeType">节点类型<br/>NodeType</typeparam>
    public interface IGPathfinder<NodeType>
    {
        /// <summary>
        /// 使用 <b>迪杰斯特拉算法</b> 计算两个点的最短路径<br/>
        /// calculate the least cost path using <i><b>Dijkstra Algorithm.</b></i>
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="σ(n) ~ O(n^2-2n+k)" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始点<br/>Start Node</param>
        /// <param name="end">终止点<br/>End Node</param>
        /// <returns>生成的最短路径 <br/> the Least cost path</returns>
        /// <exception cref="NodeNotExistException"></exception>
        /// <exception cref="NodeUnreachableException"></exception>
        NodeType[] Dijkstra(NodeType start, NodeType end);
        /// <summary>
        /// 使用 <b>贝尔曼福德算法</b> 计算两个点最短路径<br/>Use <i><b>Bellman-Ford Algorithm</b></i> to find Shortest Path to specific Nodes
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="σ(3n^2-n) ~ O(3n*j*(n-1))" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始点<br/>Start Node</param>
        /// <param name="end">终止点<br/>End Node</param>
        /// <returns>生成的最短路径 <br/> the Least cost path</returns>
        NodeType[] BellmanFord(NodeType start, NodeType end);
        /// <summary>
        /// 使用 <b>贝尔曼福德算法</b> 计算单源点最短距离和路径表<br/>Use <i><b>Bellman-Ford Algorithm</b></i> to find Single Node Shortest Path Related Nodes
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="σ(3n^2-n) ~ O(3n*j*(n-1))" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始点<br/>Start Node</param>
        /// <returns>
        /// <i><b>Dist</b></i> 为单源点终点权重表, <i><b>Table</b></i> 为前序节点表(节点,前序节点) <br/>
        /// <i><b>Dist</b></i> is a Dictionary stores the path cost to other Node, <i><b>Table</b></i> is a adjacency table which store as (node,Preamble node) 
        /// </returns>
        (Dictionary<NodeType, int> Dist, Dictionary<NodeType, NodeType> AdjacencyTable) BellmanFord_Map(NodeType start);
        /// <summary>
        ///  <b>贝尔曼福德算法</b>负权环检测<br/><i><b>Bellman-Ford Algorithm</b></i> Negative Weight Cycle Detector
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="O(2n)" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="Dist">算法产生的最短路径表<br/> Distance Map by BellmanFord</param>
        /// <returns>负权环边列表<br/>the edge create the negative loop</returns>
        Tuple<NodeType, NodeType, int>[] BellmanFord_NWCDetector(Dictionary<NodeType, int> Dist);
        /// <summary>
        /// 使用 <b>弗洛伊德算法</b> 计算任意两点间的最短路径<br/>Use <i><b>Floyd-Warshall Algorithm</b></i> to find Shortest Path to specific Nodes
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="O(n^3)" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始点<br/>Start Node</param>
        /// <param name="end">终止点<br/>End Node</param>
        /// <returns>生成的最短路径 <br/> the Least cost path</returns>
        NodeType[] FloydWarshall(NodeType start, NodeType end);
    }
}
