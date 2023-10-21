//using Meow.Math.Graph;
//using Meow.Math.Graph.Struct;
//using System.Diagnostics;

//DateTime start = DateTime.Now;
//double usedMemory1 = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
//{

using Meow.Math.Graph.Struct;
using Meow.Math.Graph.Util;

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

    Graph<string> g = Util.ReadMap(s.Split("\n"));
    var (Table, Root) = g.MST_BellmanFord("n3");
    Console.WriteLine(Util.PrintTree(Root,Table));

//    //string st = "" +
//    //    "*n1\n"
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
//    //Tree<string> gt = Util.ReadMappedTree(st.Split("\n"));
//}
//double usedMemory2 = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
//DateTime end = DateTime.Now;
//Console.WriteLine("----------");
//Console.WriteLine($"{(end - start).TotalMilliseconds} ms / {usedMemory2 - usedMemory1} MB");


