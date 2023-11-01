using System;

namespace Meow.Math.Bin
{
    /// <summary>
    /// 比特工具类
    /// </summary>
    public static class BitUtil
    {
        /// <summary>
        /// 设置Byte的某一位到某个状态
        /// </summary>
        /// <param name="data">源</param>
        /// <param name="index">位(1-8)</param>
        /// <param name="flag">位状态</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void SetBit(ref this byte data, int index, bool flag)
        {
            if (index > 8 || index < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            int v = index < 2 ? index : (2 << (index - 2));
            data = flag ? (byte)(data | v) : (byte)(data & ~v);
        }
        /// <summary>
        /// 设置Byte的所有位
        /// </summary>
        /// <param name="data">源</param>
        /// <param name="flags">位状态</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void SetBit(ref this byte data, params bool[] flags)
        {
            if (flags.Length > 8 || flags.Length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(flags));
            }
            for (int i = 0; i < flags.Length; i++)
            {
                data.SetBit(i + 1, flags[i]);
            }
        }
        /// <summary>
        /// 获取Byte的所有位
        /// </summary>
        /// <param name="this">源</param>
        /// <returns></returns>
        public static bool[] GetBit(this byte @this)
        {
            bool[] vs = new bool[8];
            for (int i = 0; i < 7; i++)
            {
                vs[i] = @this.GetBit(i);
            }
            return vs;
        }
        /// <summary>
        /// 获取Byte的某一位
        /// </summary>
        /// <param name="this">源</param>
        /// <param name="index">位(1-8)</param>
        /// <returns></returns>
        public static bool GetBit(this byte @this, int index)
        {
            byte x;
            switch (index)
            {
                case 1: { x = 0x01; } break;
                case 2: { x = 0x02; } break;
                case 3: { x = 0x04; } break;
                case 4: { x = 0x08; } break;
                case 5: { x = 0x10; } break;
                case 6: { x = 0x20; } break;
                case 7: { x = 0x40; } break;
                case 8: { x = 0x80; } break;
                default: { return false; }
            }
            return (@this & x) == x;
        }
    }
}
