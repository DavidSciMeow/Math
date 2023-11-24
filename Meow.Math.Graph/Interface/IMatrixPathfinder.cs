using Meow.Math.Graph.Interface;

namespace Graph.Interface
{
    public interface IMatrixPathfinder<NodeType> : IGPathfinder<NodeType>
    {
        NodeType[] AStar(NodeType start, NodeType end);
    }
}
