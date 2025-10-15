using System;

namespace GraphX.Error
{
    /// <summary>
    /// 当检测到负权回路时抛出的异常（由 Bellman-Ford 或相关算法）。<br/>Exception thrown when a negative weight cycle is detected by Bellman-Ford or related algorithms.
    /// </summary>
    public class BFANWCDetectedException : Exception
    {
        public BFANWCDetectedException() : base("Negative weight cycle detected by Bellman-Ford") { }
    }
}