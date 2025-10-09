using System;
using System.Collections.Generic;
using GraphX.Error;

namespace GraphX.Core
{
    /// <summary>
    /// ·����������������ڵ����С��ܴ�����ɴ��־��<br/>Path result container: contains node sequence, total cost and reachability flag.
    /// </summary>
    /// <typeparam name="NodeType">�ڵ����� / node type</typeparam>
    /// <typeparam name="TWeight">Ȩ������ / weight type</typeparam>
    public class PathResult<NodeType, TWeight>
    {
        /// <summary>
        /// ·���Ľڵ��б���˳�򣩡�<br/>Nodes in path (ordered).
        /// </summary>
        public List<NodeType> Nodes { get; set; }

        /// <summary>
        /// �ܴ��ۡ�<br/>Total cost.
        /// </summary>
        public TWeight Cost { get; set; } = default!;

        /// <summary>
        /// �Ƿ�ɴ<br/>Whether reachable.
        /// </summary>
        public bool Reachable { get; set; }

        public PathResult()
        {
            Nodes = new List<NodeType>();
            Cost = default;
            Reachable = false;
        }

        /// <summary>
        /// ����������������У�飺�� nodes Ϊ null ʹ�ÿ��б��� cost Ϊ null �� TWeight Ϊ�����������׳��쳣��ֵ������ʹ��Ĭ��ֵ��<br/>Factory that validates inputs. If nodes is null, uses empty list. If cost is null and TWeight is a reference type, throws; if value type, uses default.
        /// </summary>
        public static PathResult<NodeType, TWeight> Create(List<NodeType> nodes = null, object cost = null, bool reachable = false)
        {
            // nodes: ���� null ���滻Ϊ���б� / accept null and replace with empty list
            var nodesSafe = nodes ?? new List<NodeType>();

            TWeight costValue;

            if (cost == null)
            {
                // �� TWeight Ϊ�������ͣ������� null / If TWeight is reference type, null is not acceptable
                if (default(TWeight) == null)
                {
                    throw new InvalidArgumentException("Cost cannot be null for reference typed TWeight");
                }
                costValue = default;
            }
            else
            {
                // ���ṩ cost������ת��Ϊ TWeight / cost provided: try to obtain TWeight
                if (cost is TWeight cw)
                {
                    costValue = cw;
                }
                else
                {
                    try
                    {
                        // ������Ԫ���͵�ת�� / attempt conversion for common primitive types
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
