using Meow.Math.Graph.Interface;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Meow.Math.Graph.Struct
{
    /// <summary>
    /// 图节点<br/> Graph Node Structure
    /// </summary>
    /// <param name="Id"> 节点识别号<br/>Graph Node Id </param>
    /// <typeparam name="T">图节点类型<br/> Graph Node Type</typeparam>
    public readonly struct GraphNode<T> : IMapNode<T>, IEnumerable<KeyValuePair<T, int>> where T : IEquatable<T>
    {

        /// <summary>
        /// 节点识别号<br/>Graph Node Id
        /// </summary>
        public T Id { get; }
        /// <summary>
        /// 邻接矩阵 键值对为 [节点识别号, 节点的权重]<br/>
        /// Adjacency Tables which structure [Key, Value] is [Node ID (which linked), Edges Weight]
        /// </summary>
        public Dictionary<T, int> Edges { get; }
        /// <summary>
        /// 初始化节点 <br/> Init a Node
        /// </summary>
        /// <param name="id">节点识别号<br/>Graph Node Id</param>
        public GraphNode(T id) : this()
        {
            Id = id;
            Edges = new Dictionary<T, int>();
        }
        /// <summary>
        /// 根据节点名索引链接节点权重 <br/>
        /// Get Weight of Linked Node (by Node Name)
        /// <para>时间复杂度(Time Complexity) :: <i><b><see langword="O(1)" /></b></i></para>
        /// </summary>
        /// <param name="node">节点名<br/>Node Id</param>
        /// <returns>索引到的节点权重<br/>weight that The node linked</returns>
        public int this[T node] => Edges[node];
        public void Add(T node, int weight) => Edges.Add(node, weight);
        public bool Delete(T node) => Edges.Remove(node);
        public bool Exist(T node) => Edges.ContainsKey(node);
        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => ((IEnumerable<KeyValuePair<T, int>>)Edges).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Edges).GetEnumerator();
    }

}
