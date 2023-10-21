using Meow.Math.Graph.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Meow.Math.Graph.Util
{
    public static class Util
    {
        public static string PrintTree<NodeType>(NodeType Root, Dictionary<NodeType, HashSet<NodeType>> Table)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Root:{Root} | NodeNum:{Table.Count}");
            HashSet<NodeType> visited = new HashSet<NodeType>() { Root };//首元素访问标记
            Stack<NodeType> ss = new Stack<NodeType>();
            ss.Push(Root);//搜索元素入栈
            sb.Append($"{Root}\n");

            while (ss.Count > 0)//σ(n) 若栈不空
            {
                var parent = ss.Peek();//获取栈顶元素
                bool _isEdgeVisited = true;//标记边访问
                int j = 0;
                if (Table.ContainsKey(parent))
                {
                    foreach (NodeType node in Table[parent])//σ(n-k) 获得下一节点
                    {
                        j++;
                        if (visited.Add(node))//未访问节点则标记访问
                        {
                            char[] pred = new char[ss.Count];
                            for (int i = 1; i < ss.Count; i++)//对于任意前序节点
                            {
                                var llast = Table[ss.ElementAt(i)].Last();//检测节点父节点的最后一个
                                var desnode = ss.ElementAtOrDefault(i - 1);//当前遍历内节点
                                pred[ss.Count - i] = llast?.Equals(desnode) ?? false ? ' ' : '│';//是的话就不写杠
                            }
                            sb.Append(pred);//正着输出前序关系
                            sb.AppendLine($"{(j < Table[parent].Count ? "├" : "└")}{node}");//节点访问到
                            ss.Push(node);//入栈
                            _isEdgeVisited = false;//取消边访问
                            break;
                        }
                    }
                }

                if (_isEdgeVisited) ss.Pop();//边访问, 元素出栈
            }
            return sb.ToString();
        }
        public static Graph<string> ReadMap(string[] seplines)
        {
            Graph<string> ms = new Graph<string>();
            foreach (string line in seplines)
            {
                int weight = 1;
                string node1;
                string node2;
                if (line.Contains('-'))
                {
                    var n1 = line.Split('-');
                    node1 = n1[0];

                    if (n1[1].Contains(":"))
                    {
                        var n2 = n1[1].Split(':');
                        if (int.TryParse(n2[1], out var _w))
                        {
                            node2 = n2[0];
                            weight = _w;
                        }
                        else
                        {
                            Console.WriteLine($"{n2[1]} is not a num.");
                            continue;
                        }
                    }
                    else
                    {
                        node2 = n1[1];
                    }

                    if (!ms.Exist(node1)) ms.Add(node1);
                    if (!ms.Exist(node2)) ms.Add(node2);
                    _ = ms[node1][node2, weight];
                    _ = ms[node2][node1, weight];
                }
                else if (line.Contains(">"))
                {
                    var n1 = line.Split('>');
                    node1 = n1[0];

                    if (n1[1].Contains(":"))
                    {
                        var n2 = n1[1].Split(':');
                        if (int.TryParse(n2[1], out var _w))
                        {
                            node2 = n2[0];
                            weight = _w;
                        }
                        else
                        {
                            Console.WriteLine($"{n2[1]} is not a num.");
                            continue;
                        }
                    }
                    else
                    {
                        node2 = n1[1];
                    }

                    if (!ms.Exist(node1)) ms.Add(node1);
                    if (!ms.Exist(node2)) ms.Add(node2);
                    _ = ms[node1][node2, weight];
                }
            }
            return ms;
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="seplines"></param>
        ///// <returns></returns>
        //public static Tree<string> ReadMappedTree(string[] seplines)
        //{
        //    Tree<string> tree = null;
        //    bool _isRootDefine = false;
        //    foreach (string line in seplines)
        //    {
        //        if (line.Contains('*'))
        //        {
        //            if (!_isRootDefine)
        //            {
        //                tree = new Tree<string>(line.Replace("*", ""));
        //                _isRootDefine = true;
        //            }
        //            else
        //            {
        //                Console.WriteLine($"{line} err by only one tree root can be define");
        //            }
        //        }
        //        else if (line.Contains('>'))
        //        {
        //            if (!_isRootDefine)
        //            {
        //                Console.WriteLine($"No Root Node Define. >skip");
        //            }
        //            else
        //            {
        //                var spl = line.Split('>');
        //                var fn = spl[0];
        //                var sn = spl[1];
        //                if (!string.IsNullOrEmpty(fn) && !string.IsNullOrEmpty(sn) && tree is Tree<string> st)
        //                {
        //                    st.AddNode(sn, fn);
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"{line} have no splitable node");
        //                }
        //            }
        //        }
        //    }
        //    return tree;
        //}
    }
}


