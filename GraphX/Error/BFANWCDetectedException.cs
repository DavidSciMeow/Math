using System;

namespace GraphX.Error
{
    /// <summary>
    /// ����⵽��Ȩ��·ʱ�׳����쳣���� Bellman-Ford ������㷨����<br/>Exception thrown when a negative weight cycle is detected by Bellman-Ford or related algorithms.
    /// </summary>
    public class BFANWCDetectedException : Exception
    {
        public BFANWCDetectedException() : base("Negative weight cycle detected by Bellman-Ford") { }
    }
}