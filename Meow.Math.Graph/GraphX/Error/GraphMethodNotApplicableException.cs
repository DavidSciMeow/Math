using System;

namespace GraphX.Error
{
    /// <summary>
    /// 当图不满足某个算法所需的前置条件时抛出该异常。<br/>Thrown when a graph does not satisfy preconditions required by a graph algorithm.
    /// 包含关于方法名和原因等的可选信息。<br/>Contains optional details about which method and why it is not applicable.
    /// </summary>
    public class GraphMethodNotApplicableException : Exception
    {
        /// <summary>
        /// 检测到前置条件失败的方法或算法名（可选）。<br/>Algorithm or method name that detected the precondition failure (optional).
        /// </summary>
        public string? MethodName { get; }

        /// <summary>
        /// 解释为什么方法不适用的简短原因（可选）。<br/>Short reason explaining why the method is not applicable (optional).
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// 导致失败的图状态摘要（例如 "directed=true, negativeEdges=true"）（可选）。<br/>Optional summary of the graph state that caused the failure (e.g. "directed=true, negativeEdges=true").
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
