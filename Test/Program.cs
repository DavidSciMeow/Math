using Meow.Math.Graph;
using Meow.Math.Graph.Struct;
using System.Diagnostics;

DateTime start = DateTime.Now;
double usedMemory1 = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
{
    string s = "" +
        "n1-n2\n" +
        "n1-n5\n" +
        "n2-n6\n" +
        "n6-n3\n" +
        "n6-n7\n" +
        "n3-n7\n" +
        "n7-n4\n" +
        "n3-n4\n" +
        "n7-n8\n" +
        "n8-n4\n" +
        "";


    Graph<string> g = Util.ReadMap(s.Split("\n"));
    var sx = g.BellmanFord_Tree("n1");
    Console.WriteLine(sx);

    //    var l = File.ReadAllLines("./a");
    //    Graph<string> g = Util.ReadMap(l);
    //    //foreach(var i in g.BellmanFord_Edge("青岛站", "台东")) Console.WriteLine(i); // 56ms 6.18MB
    //    //foreach(var i in g.Dijkstra_Edge("青岛站", "台东")) Console.WriteLine(i); //32ms 0.87MB

    //    Console.WriteLine(g.BellmanFord_Tree("青岛站"));

    //    //string st = "" +
    //    //    "*n1\n" +
    //    //    "n1>n2\n" +
    //    //       "n2>n3\n" +
    //    //          "n3>n4\n" +
    //    //             "n4>n5\n" +
    //    //                "n5>n6\n" +
    //    //    "n1>n122\n" +
    //    //       "n122>n21\n" +
    //    //       "n122>n22\n" +
    //    //       "n122>n23\n" +
    //    //            "n23>n31\n" +
    //    //                "n31>n141\n" +
    //    //            "n23>n32\n" +
    //    //    "n1>n123\n" +
    //    //    "";

    //    //Tree<string>? gt = Util.ReadMappedTree(st.Split("\n"));
    //    //Console.WriteLine(gt);
    //    //gt?.BFS();

}
double usedMemory2 = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
DateTime end = DateTime.Now;
Console.WriteLine("----------");
Console.WriteLine($"{(end - start).TotalMilliseconds} ms / {usedMemory2 - usedMemory1} MB");
