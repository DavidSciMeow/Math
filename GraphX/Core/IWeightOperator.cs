using System;
using System.Collections.Generic;

namespace GraphX.Core
{
    /// <summary>
    /// 权重操作器抽象：定义权重类型的算术与比较语义。<br/>Weight operator abstraction: defines arithmetic and comparison semantics for a weight type.
    /// </summary>
    /// <typeparam name="T">权重类型 / weight type</typeparam>
    public interface IWeightOperator<T> : IComparer<T>
    {
        /// <summary>
        /// 加法单位零值（表示路径长度为 0）。<br/>Additive identity (zero) value for the weight type.
        /// </summary>
        T Zero { get; }

        /// <summary>
        /// 无权边的默认权重（例如 1）。<br/>Default weight representing a single unweighted edge (e.g. 1).
        /// </summary>
        T One { get; }

        /// <summary>
        /// 表示无穷大的权重（用于初始化或不可达）。<br/>Representation of infinity for the weight type (used for initialization / unreachable).
        /// </summary>
        T Infinity { get; }

        /// <summary>
        /// 权重加法操作。<br/>Weight addition operation.
        /// </summary>
        T Add(T a, T b);
    }
}