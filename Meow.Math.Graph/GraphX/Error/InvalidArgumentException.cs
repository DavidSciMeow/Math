using System;

namespace GraphX.Error
{
    /// <summary>
    /// ������Ϊ null �򲻺Ϸ�ʱ�׳����쳣��<br/>Thrown when an argument is null or otherwise invalid for the operation.
    /// </summary>
    public class InvalidArgumentException : ArgumentException
    {
        public InvalidArgumentException() : base("Invalid argument provided.") { }
        public InvalidArgumentException(string message) : base(message) { }
        public InvalidArgumentException(string message, string paramName) : base(message, paramName) { }
    }
}
