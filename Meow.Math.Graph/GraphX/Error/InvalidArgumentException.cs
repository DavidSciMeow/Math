using System;

namespace GraphX.Error
{
    /// <summary>
    /// 当参数为 null 或不合法时抛出的异常。<br/>Thrown when an argument is null or otherwise invalid for the operation.
    /// </summary>
    public class InvalidArgumentException : ArgumentException
    {
        public InvalidArgumentException() : base("Invalid argument provided.") { }
        public InvalidArgumentException(string message) : base(message) { }
        public InvalidArgumentException(string message, string paramName) : base(message, paramName) { }
    }
}
