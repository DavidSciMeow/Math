using System;

namespace GraphX.Error
{
    /// <summary>
    /// ��ͼ������ĳ���㷨�����ǰ������ʱ�׳����쳣��<br/>Thrown when a graph does not satisfy preconditions required by a graph algorithm.
    /// �������ڷ�������ԭ��ȵĿ�ѡ��Ϣ��<br/>Contains optional details about which method and why it is not applicable.
    /// </summary>
    public class GraphMethodNotApplicableException : Exception
    {
        /// <summary>
        /// ��⵽ǰ������ʧ�ܵķ������㷨������ѡ����<br/>Algorithm or method name that detected the precondition failure (optional).
        /// </summary>
        public string? MethodName { get; }

        /// <summary>
        /// ����Ϊʲô���������õļ��ԭ�򣨿�ѡ����<br/>Short reason explaining why the method is not applicable (optional).
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// ����ʧ�ܵ�ͼ״̬ժҪ������ "directed=true, negativeEdges=true"������ѡ����<br/>Optional summary of the graph state that caused the failure (e.g. "directed=true, negativeEdges=true").
        /// </summary>
        public string? GraphSummary { get; }

        public GraphMethodNotApplicableException()
            : base("Graph does not satisfy preconditions for this method.") { }

        public GraphMethodNotApplicableException(string message)
            : base(message)
        {
            Reason = message;
        }

        public GraphMethodNotApplicableException(string methodName, string reason)
            : base(FormatMessage(methodName, reason, null))
        {
            MethodName = methodName;
            Reason = reason;
        }

        public GraphMethodNotApplicableException(string methodName, string reason, string graphSummary)
            : base(FormatMessage(methodName, reason, graphSummary))
        {
            MethodName = methodName;
            Reason = reason;
            GraphSummary = graphSummary;
        }

        private static string FormatMessage(string methodName, string reason, string? graphSummary)
        {
            var msg = string.IsNullOrWhiteSpace(methodName) ? "Graph method precondition failed." : $"Method '{methodName}' precondition failed.";
            if (!string.IsNullOrWhiteSpace(reason)) msg += " Reason: " + reason + ".";
            if (!string.IsNullOrWhiteSpace(graphSummary)) msg += " Graph: " + graphSummary + ".";
            return msg;
        }

        public override string ToString()
        {
            var baseStr = base.ToString();
            if (string.IsNullOrEmpty(MethodName) && string.IsNullOrEmpty(Reason) && string.IsNullOrEmpty(GraphSummary)) return baseStr;
            return $"{baseStr}{Environment.NewLine}Method: {MethodName ?? "(unknown)"}; Reason: {Reason ?? "(none)"}; Graph: {GraphSummary ?? "(none)"}";
        }
    }
}
