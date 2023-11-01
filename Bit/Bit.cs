using System;

namespace Meow.Math.Bin
{
    /// <summary>
    /// 二进制帮助类
    /// <para> 本二进制帮助类采用如下索引方式 </para>
    /// <para>0x [7][6][5][4][3][2][1][0] </para>
    /// </summary>
    public struct Bit
    {
        private byte output;
        /// <summary>
        /// 生成一个二进制帮助类(使用bool[])
        /// 本二进制帮助类采用如下索引方式 <br/>
        /// 0x [7][6][5][4][3][2][1][0]
        /// </summary>
        /// <param name="flags">源(8位)</param>
        public Bit(params bool[] flags)
        {
            output = 0;
            output.SetBit(flags);
        }

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
        public byte ToByte() => output;
        /// <summary>
        /// 索引位
        /// 本二进制帮助类采用如下索引方式 <br/>
        /// 0x [7][6][5][4][3][2][1][0]
        /// </summary>
        /// <param name="index">第n位</param>
        /// <returns></returns>
        public bool this[int index]
        {
            get
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
        public static implicit operator Bit(byte d) => new Bit(d);
        /// <inheritdoc/>
        public static explicit operator byte(Bit d) => d.output;
        /// <inheritdoc/>
        public override string ToString() => null;
    }


}
