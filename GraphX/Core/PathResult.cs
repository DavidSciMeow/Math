using System;
using System.Collections.Generic;
using GraphX.Error;

namespace GraphX.Core
{
    /// <summary>
    /// 路径结果容器：包含节点序列、总代价与可达标志。<br/>Path result container: contains node sequence, total cost and reachability flag.
    /// </summary>
    /// <typeparam name="NodeType">节点类型 / node type</typeparam>
    /// <typeparam name="TWeight">权重类型 / weight type</typeparam>
    public class PathResult<NodeType, TWeight>
    {
        /// <summary>
        /// 路径的节点列表（按顺序）。<br/>Nodes in path (ordered).
        /// </summary>
        public List<NodeType> Nodes { get; set; }

        /// <summary>
        /// 总代价。<br/>Total cost.
        /// </summary>
        public TWeight Cost { get; set; } = default!;

        /// <summary>
        /// 是否可达。<br/>Whether reachable.
        /// </summary>
        public bool Reachable { get; set; }

        public PathResult()
        {
            Nodes = new List<NodeType>();
            Cost = default;
            Reachable = false;
        }

        /// <summary>
        /// 工厂方法并带输入校验：若 nodes 为 null 使用空列表；若 cost 为 null 且 TWeight 为引用类型则抛出异常；值类型则使用默认值。<br/>Factory that validates inputs. If nodes is null, uses empty list. If cost is null and TWeight is a reference type, throws; if value type, uses default.
        /// </summary>
        public static PathResult<NodeType, TWeight> Create(List<NodeType> nodes = null, object cost = null, bool reachable = false)
        {
            // nodes: 接受 null 并替换为空列表 / accept null and replace with empty list
            var nodesSafe = nodes ?? new List<NodeType>();

            TWeight costValue;

            if (cost == null)
            {
                // 若 TWeight 为引用类型，不接受 null / If TWeight is reference type, null is not acceptable
                if (default(TWeight) == null)
                {
                    throw new InvalidArgumentException("Cost cannot be null for reference typed TWeight");
                }
                costValue = default;
            }
            else
            {
                // 已提供 cost：尝试转换为 TWeight / cost provided: try to obtain TWeight
                if (cost is TWeight cw)
                {
                    costValue = cw;
                }
                else
                {
                    try
                    {
                        // 常见基元类型的转换 / attempt conversion for common primitive types
                        costValue = (TWeight)Convert.ChangeType(cost, typeof(TWeight));
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidArgumentException($"Cost value cannot be converted to {typeof(TWeight).Name}: {ex.Message}");
                    }
                }
            }

            return new PathResult<NodeType, TWeight>
            {
                Nodes = nodesSafe,
                Cost = costValue,
                Reachable = reachable
            };
        }
    }
}
