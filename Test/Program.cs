using Meow.Math.Graph.Struct;
using Meow.Math.Graph.Util;
using System.Buffers;
using System.Collections;
using System.Runtime.InteropServices;

string s = "" +
        "n1-n6:3\n" +
        "n6-n5:6\n" +
        "n5-n4:11\n" +
        "n6-n7:4\n" +
        "n5-n7:12\n" +
        "n2-n1:6\n" +
        "n1-n8:7\n" +
        "n2-n8:2\n" +
        "n3-n8:4\n" +
        "n4-n8:7\n" +
        "n7-n8:1\n" +
        "";

ArrayList x1 = ["a","b","c"];
List<string> x2 = ["a","b","c"];
HashSet<string> x3 = ["a","b","c"];
ArrayList x4 = [.. x1, .. x2];


Graph<string> g = Util.ReadMap(s.Split("\n"));

//Console.WriteLine(g.DFS("n1").QuickPrint());
//Console.WriteLine(g.BFS("n1").QuickPrint());
//Console.WriteLine(g.Dijkstra("n1", "n5").QuickPrint());
//Console.WriteLine(g.BellmanFord("n1", "n5").QuickPrint());
//Console.WriteLine(g.FloydWarshall("n1", "n5").QuickPrint());
//Console.WriteLine(g.MST_BellmanFord("n1").TreePrint());
//Console.WriteLine(g.MST_Prim("n1").TreePrint());
Console.WriteLine(g.MST_Kruskal("n1").TreePrint());


//var i = g.MST_Prim("n1");
//Console.WriteLine(Util.PrintTree(i.Root, i.Table));
