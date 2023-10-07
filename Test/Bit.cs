using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Math
{
    /// <summary>
    /// 二进制帮助类
    /// <para> 本二进制帮助类采用如下索引方式 </para>
    /// <para>0x [7][6][5][4][3][2][1][0] </para>
    /// </summary>
    public struct Bit : ISpanFormattable, IFormattable
    {
        private byte output = 0;
        /// <summary>
        /// 生成一个二进制帮助类(默认) <br/>
        /// 本二进制帮助类采用如下索引方式 <br/>
        /// 0x [7][6][5][4][3][2][1][0]
        /// </summary>
        public Bit() { output = 0; }
        /// <summary>
        /// 生成一个二进制帮助类(使用bool[])
        /// 本二进制帮助类采用如下索引方式 <br/>
        /// 0x [7][6][5][4][3][2][1][0]
        /// </summary>
        /// <param name="flags">源(8位)</param>
        public Bit(params bool[] flags) => output.SetBit(flags);
        /// <summary>
        /// 生成一个二进制帮助类(使用Byte)
        /// 本二进制帮助类采用如下索引方式 <br/>
        /// 0x [7][6][5][4][3][2][1][0]
        /// </summary>
        /// <param name="data">源</param>
        public Bit(byte data) => output = data;

        /// <summary>
        /// 返回Byte
        /// </summary>
        /// <returns></returns>
        public readonly byte ToByte() => output;
        /// <summary>
        /// 索引位
        /// 本二进制帮助类采用如下索引方式 <br/>
        /// 0x [7][6][5][4][3][2][1][0]
        /// </summary>
        /// <param name="index">第n位</param>
        /// <returns></returns>
        public bool this[int index]
        {
            readonly get
            {
                if (index > 7 || index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return output.GetBit(index + 1);
            }
            set
            {
                if (index > 7 || index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                output.SetBit(index + 1, value);
            }
        }

        /// <inheritdoc/>
        public static implicit operator Bit(byte d) => new(d);
        /// <inheritdoc/>
        public static explicit operator byte(Bit d) => d.output;
        /// <inheritdoc/>
        public override readonly string? ToString() => null;
        /// <inheritdoc/>
        public readonly string ToString(string format) => output.ToString(format);
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider provider) => output.ToString(provider);
        /// <inheritdoc/>
        public readonly string ToString(string? format, IFormatProvider? provider) => output.ToString(format, provider);
        /// <inheritdoc/>
        public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => output.TryFormat(destination, out charsWritten, format, provider);
    }


}
