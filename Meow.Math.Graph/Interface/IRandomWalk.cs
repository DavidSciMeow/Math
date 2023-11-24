using Meow.Math.Graph.Interface;

namespace Graph.Interface
{
    public interface IRandomWalk<NodeType> : IGPathfinder<NodeType>
    {
        NodeType[] RandomWalk(NodeType start);
    }
}
