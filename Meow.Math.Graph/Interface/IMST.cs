using System.Collections.Generic;

namespace Graph.Interface
{
    /// <summary>
    /// 最小生成树接口<br/>Minimum Spanning Tree Algorithm
    /// </summary>
    /// <typeparam name="NodeType">节点类型<br/>NodeType</typeparam>
    public interface IMST<NodeType>
    {
        /// <summary>
        /// 以 <b>普利姆</b> 为最短路径基准生成最小生成树 <br/> Use <i><b>Prim Algorithm</b></i> to create a Minimum Spanning Tree
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="*" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始节点(根节点)<br/>Start Node In Graph (Root Node of tree)</param>
        /// <returns>一个最小生成树<br/>Minimum Spanning Tree</returns>
        (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_Prim(NodeType start);
        /// <summary>
        /// 以 <b>克鲁斯卡尔</b> 为最短路径基准生成最小生成树 <br/> Use <i><b>Kruskal Algorithm</b></i> to create a Minimum Spanning Tree
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="*" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始节点(根节点)<br/>Start Node In Graph (Root Node of tree)</param>
        /// <returns>一个最小生成树<br/>Minimum Spanning Tree</returns>
        (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_Kruskal(NodeType start);
        /// <summary>
        /// 以 <b>贝尔曼福德算法</b> 为最短路径基准生成最小生成树 <br/> Use <i><b>Bellman-Ford Algorithm</b></i> to create a Minimum Spanning Tree
        /// <para>
        /// 时间复杂度(Time Complexity) :: <i><b><see langword="σ(3n^2-n) ~ O(3n*j*(n-1))" /></b></i><br/>
        /// </para>
        /// </summary>
        /// <param name="start">起始节点(根节点)<br/>Start Node In Graph (Root Node of tree)</param>
        /// <returns>一个最小生成树<br/>Minimum Spanning Tree</returns>
        (Dictionary<NodeType, HashSet<NodeType>> Table, NodeType Root) MST_BellmanFord(NodeType start);
    }
}
