using Meow.Math.Graph.Struct;
using Meow.Math.Graph.Util;

namespace Test
{
    internal class mainx
    {
        public void Ms()
        {
            string s =  "" +
                        "A-D:2\n" +
                        "A-B:1\n" +
                        "D-B:4\n" +
                        "D-C:7\n" +
                        "D-E:10\n" +
                        "B-C:3\n" +
                        "B-F:7\n" +
                        "C-F:5\n" +
                        "C-E:6\n" +
                        "E-F:7\n" +
                        "F-G:12\n" +
                        "";

            Graph<string> g = Util.ReadMap(s.Split("\n"));

            var start = "A";
            var end = "E";

            //Console.WriteLine(g.DFS(start).QuickPrint());
            //Console.WriteLine(g.BFS(start).QuickPrint());
            //Console.WriteLine(g.Dijkstra(start, end).QuickPrint());
            //Console.WriteLine(g.BellmanFord(start, end).QuickPrint());
            //Console.WriteLine(g.FloydWarshall(start, end).QuickPrint());
            //Console.WriteLine(g.GSMST_BellmanFord(start).TreePrint());
            Console.WriteLine(g.MST_Prim(start).TreePrint());
            Console.WriteLine(g.MST_Kruskal(start).TreePrint());
        }
    }
}
