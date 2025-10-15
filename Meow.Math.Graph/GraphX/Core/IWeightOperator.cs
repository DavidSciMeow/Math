using System;
using System.Collections.Generic;

namespace GraphX.Core
{
    /// <summary>
    /// Ȩ�ز��������󣺶���Ȩ�����͵�������Ƚ����塣<br/>Weight operator abstraction: defines arithmetic and comparison semantics for a weight type.
    /// </summary>
    /// <typeparam name="T">Ȩ������ / weight type</typeparam>
    public interface IWeightOperator<T> : IComparer<T>
    {
        /// <summary>
        /// �ӷ���λ��ֵ����ʾ·������Ϊ 0����<br/>Additive identity (zero) value for the weight type.
        /// </summary>
        T Zero { get; }

        /// <summary>
        /// ��Ȩ�ߵ�Ĭ��Ȩ�أ����� 1����<br/>Default weight representing a single unweighted edge (e.g. 1).
        /// </summary>
        T One { get; }

        /// <summary>
        /// ��ʾ������Ȩ�أ����ڳ�ʼ���򲻿ɴ��<br/>Representation of infinity for the weight type (used for initialization / unreachable).
        /// </summary>
        T Infinity { get; }

        /// <summary>
        /// Ȩ�ؼӷ�������<br/>Weight addition operation.
        /// </summary>
        T Add(T a, T b);
    }
}