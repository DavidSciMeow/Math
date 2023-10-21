using Meow.Math.Graph.ErrorList;

namespace Meow.Math.Graph.Interface
{
    /// <summary>
    /// 可遍历结构接口<br/>Searchable Interface
    /// </summary>
    /// <typeparam name="NodeType">节点类型<br/>NodeType</typeparam>
    public interface ISearchable<NodeType>
    {
        /// <summary>
        /// 广度优先遍历节点<br/> Enumerate all childnode by Breadth First Search order.
        /// <para>时间复杂度(Time Complexity) :: <i><b><see langword="O(n)" /></b></i></para>
        /// </summary>
        /// <param name="start">搜索起始点<br/>Search Starts From</param>
        /// <returns>节点列表<br/> List Of Nodes</returns>
        /// <exception cref="NodeNotExistException"></exception>
        NodeType[] BFS(NodeType start);
        /// <summary>
        /// 深度优先遍历节点<br/> Enumerate all childnode by Depth First Search order.
        /// <para>时间复杂度(Time Complexity) :: <i><b><see langword="O(n)" /></b></i></para>
        /// </summary>
        /// <param name="start">搜索起始点<br/>Search Starts From</param>
        /// <returns>节点列表<br/> List Of Nodes</returns>
        /// <exception cref="NodeNotExistException"></exception>
        NodeType[] DFS(NodeType start);
    }
}
