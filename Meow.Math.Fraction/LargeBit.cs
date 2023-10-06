using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meow.Math
{
    /// <summary>
    /// 
    /// </summary>
    public struct LargeBit : IEquatable<LargeBit>, IComparable<LargeBit>
    {
        /*-locals-*/
        /// <summary>
        /// 逻辑指针
        /// </summary>
        public readonly bool[] Ptr;
        public readonly bool Negative;
        /*-Init-*/
        public LargeBit(bool[] ptr)
        {
            Negative = false;
            Ptr = new bool[ptr.Length];
            for (int i = 0; i < ptr.Length; i++) Ptr[i] = ptr[i];
        }
        public LargeBit((bool negative, bool[] ptr) n)
        {
            Negative = n.negative;
            Ptr = new bool[n.ptr.Length];
            for (int i = 0; i < n.ptr.Length; i++) Ptr[i] = n.ptr[i];
        }

        /*-methods-*/
        /// <summary>
        /// 设置指针的某一位, 本方法时间复杂度为 <b>O(1)</b> <br/>
        /// Set one <b>Bit</b> in specific position of this Ptrs, which have a <i>O(1)</i> Time complexity 
        /// </summary>
        /// <param name="position">
        /// 指针的某一位,<b>高位为0</b> <br/>
        /// the position you want to alt, <i>which High bit is 0</i>
        /// </param>
        /// <param name="state">
        /// 要设置到的状态 <b>(true=1,false=0)</b> <br/>
        /// the state you want to set, which <i>true=1</i> and <i>false=0</i> bitwise.
        /// </param>
        public readonly void SetBit(long position, bool state) => Ptr[position] = state;
        /// <summary>
        /// 获取在某位置的一个位, 本方法时间复杂度为 <b>O(1) <br/>
        /// Get the bit in Position that you select, which have a <i>O(1)</i> Time complexity 
        /// </summary>
        /// <param name="position">
        /// 指针的某一位,<b>高位为0</b> <br/>
        /// the position you want to alt, <i>which High bit is 0</i>
        /// </param>
        /// <returns>
        /// 获取到的位 <br/>
        /// the bit in Position that you select
        /// </returns>
        public readonly bool GetBit(long position) => Ptr[position];
        /// <summary>
        /// 获取整个数值组, 本方法时间复杂度为 <b>O(1)</b> <br/>
        /// Get the whole Set of this Instance, which have a <i>O(1)</i> Time complexity 
        /// </summary>
        /// <returns>
        /// 获取的数值组
        /// The Sets (which in list of boolean)
        /// </returns>
        public readonly bool[] GetGroupList() => Ptr;
        /// <summary>
        /// 转换成字节数组
        /// </summary>
        /// <param name="input">
        /// 输入的布尔数组
        /// </param>
        /// <returns>
        /// 比特字节数组
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 
        /// </exception>
        public readonly byte[] ToByteArray()
        {
            if (Ptr.LongLength % 8 != 0)
            {
                throw new ArgumentException(null, nameof(Ptr));
            }
            byte[] ret = new byte[(long)System.Math.Ceiling(Ptr.LongLength / (double)8)];
            for (int i = 0; i < Ptr.LongLength; i += 8)
            {
                int value = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (Ptr[i + j])
                    {
                        value += 1 << (7 - j);
                    }
                }
                ret[i / 8] = (byte)value;
            }
            return ret;
        }

        public static LargeBit Parse(string str)
        {
            List<bool> rlb = new();
            bool nega = false;
            if (str[0] == '-')
            {
                nega = true;
            }
            else
            {
                rlb.Add(str[0] == '1');
            }
            for (int i = 1; i < str.Length; i++)
            {
                rlb.Add(str[i] == '1');
            }
            return new((nega, rlb.ToArray()));
        }

        /*-operations-*/
        /// <summary>
        /// 获取或设置本类型的寄存指针的某一位 <br/>
        /// Get/Set the Exist bit of this Type Instance
        /// </summary>
        /// <param name="pos">
        /// 位,<b>高位为0</b> <br/>
        /// Bits Position, <i>which High bit is 0</i>
        /// </param>
        /// <returns>
        /// 获得的布尔值 <br/>
        /// The boolean Value that retrive
        /// </returns>
        public readonly bool this[long pos] { get => GetBit(pos); set => SetBit(pos, value); }
        /// <summary>
        /// 默认以[0],[1]表示的二进制值 <br/>
        /// use default display to show the sequence with 0,1 which is binary
        /// </summary>
        /// <returns>
        /// 以[0],[1]表示的二进制值字符串 <br/>
        /// the string contains the 0,1
        /// </returns>
        public override readonly string ToString()
        {
            if (Ptr.LongLength < 0) return "0";
            StringBuilder sb = new();
            if (Negative) sb.Append('-');
            for (int i = 0; i < Ptr.LongLength; i++) sb.Append(Ptr[i] ? '1' : '0');
            return sb.ToString();
        }
        /// <inheritdoc/>
        public readonly bool Equals(LargeBit other)
        {
            if(other.Negative != Negative)
            {
                return false;
            }
            else if (other.Ptr.LongLength != Ptr.LongLength)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < Ptr.LongLength; i++)
                {
                    if (Ptr[i] != other.Ptr[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /*-operators-*/
        public static LargeBit operator <<(LargeBit a, int k) => new((a.Negative,BinaryOperation.LeftPad(a.Ptr, k)));
        //ERR
        //public static LargeBit operator >>(LargeBit a, int k) => new((a.Negative,BinaryOperation.RightPad(a.Ptr, k)));
        public static LargeBit operator +(LargeBit a, LargeBit b)
        {
            bool[] lb;
            if (!a.Negative)
            {
                if (!b.Negative)
                {
                    lb = BinaryOperation.Add(a.Ptr, b.Ptr); //+a+b
                }
                else
                {
                    lb = BinaryOperation.Sub(a.Ptr, b.Ptr).Result; //+a-b
                }
            }
            else
            {
                if (!b.Negative)
                {
                    lb = BinaryOperation.Sub(b.Ptr, a.Ptr).Result; //-a+b
                }
                else
                {
                    lb = BinaryOperation.Add(a.Ptr, b.Ptr); //-a-b
                }
            }
            return new((a.Negative && b.Negative, lb));
        }
        public static LargeBit operator -(LargeBit a) => new((true, a.Ptr));
        public static LargeBit operator -(LargeBit a, LargeBit b) => a + (-b);

        public override readonly bool Equals(object? obj) => obj is LargeBit bit && Equals(bit);
        public override readonly int GetHashCode() => HashCode.Combine(Negative, Ptr);
        public readonly int CompareTo(LargeBit other)
        {
            bool flag = false;
            if (Negative == other.Negative && Ptr.LongLength == other.Ptr.LongLength)
            {
                for (int i = 0; i < Ptr.LongLength; i++)
                {
                    if (Ptr[i] != other.Ptr[i])
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                if (Negative)
                {
                    if (!other.Negative) return -1;
                }
                else
                {
                    if (other.Negative) return 1;
                }
                return (this - other).Negative ? -1 : 1;
            }
            else
            {
                return 0;
            }
        }
        public static bool operator ==(LargeBit left, LargeBit right) => left.Equals(right);
        public static bool operator !=(LargeBit left, LargeBit right) => !(left == right);
        public static bool operator <(LargeBit left, LargeBit right) => left.CompareTo(right) < 0;
        public static bool operator <=(LargeBit left, LargeBit right) => left.CompareTo(right) <= 0;
        public static bool operator >(LargeBit left, LargeBit right) => left.CompareTo(right) > 0;
        public static bool operator >=(LargeBit left, LargeBit right) => left.CompareTo(right) >= 0;

        //public static explicit operator bool[](LargeBit b) => b.GetGroupList();
        public static implicit operator LargeBit(int b)
        {
            bool[] bools = new bool[32];
            for (int i = 0; i < 31; i++) bools[31 - i] = (b & (1 << i)) >> i == 1;
            return new LargeBit(BinaryOperation.Simplify(bools));
        }
        //public static implicit operator LargeBit(bool[] d) => new(d);

    }
    public static class BinaryOperation
    {
        /// <summary>
        /// 左移 O(n)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static bool[] LeftPad(bool[] a, long k)
        {
            bool[] ret = new bool[a.Length + k];
            for (int i = 0; i < a.Length; i++) ret[ret.Length - k - i - 1] = a[a.Length - i - 1];
            return ret;
        }

        /// <summary>
        /// 右移 O(n)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static bool[] RightPad(bool[] a, long k)
        {
            bool[] ret = new bool[a.Length + k];
            for (int i = 0; i < a.Length; i++) ret[k + i] = a[i];
            return ret;
        } //ERR

        /// <summary>
        /// 全加器 O(1)
        /// </summary>
        /// <param name="a">真值A</param>
        /// <param name="b">真值B</param>
        /// <param name="carry">上位进位</param>
        /// <returns>和/进位</returns>
        public static bool FullAdder(bool a, bool b, ref bool carry)
        {
            var ret = a != b != carry;
            carry = ((a != b) && carry) || (a && b);
            return ret;
        }

        /// <summary>
        /// 全减器 O(1)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="borrow"></param>
        /// <returns></returns>
        public static bool FullSuber(bool x, bool y, ref bool borrow)
        {
            var ret = x ^ y ^ borrow;
            borrow = (!x && borrow) || (!x && y) || (y && borrow);
            return ret;
        }

        /// <summary>
        /// 二进制加法 O(2n)
        /// </summary>
        /// <param name="a">
        /// 加数
        /// </param>
        /// <param name="b">
        /// 被加数
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public static bool[] Add(bool[] a, bool[] b)
        {
            long maxlength = a.LongLength > b.LongLength ? a.LongLength : b.LongLength;
            bool[] ret = new bool[maxlength + 1];
            bool carry = false;
            for (int i = 0; i < maxlength; i++) //O(n)
            {
                var u = i < a.LongLength && a[a.LongLength - i - 1];
                var v = i < b.LongLength && b[b.LongLength - i - 1];
                ret[ret.LongLength - i - 1] = FullAdder(u, v, ref carry);
            }
            if (carry) ret[maxlength] = carry;
            return Simplify(ret); //O(n)
        }

        /// <summary>
        /// 二进制减法 O(2n)
        /// </summary>
        /// <param name="a">
        /// 被减数
        /// </param>
        /// <param name="b">
        /// 减数
        /// </param>
        /// <returns>
        /// 二进制返回值
        /// </returns>
        public static (bool Negative, bool[] Result) Sub(bool[] a, bool[] b)
        {
            bool[] ret = new bool[a.LongLength < b.LongLength ? b.LongLength : a.LongLength];
            bool borrow = false;
            for (long i = 0; i < b.LongLength; i++) //O(n)
            {
                var u = i < a.LongLength && a[a.LongLength - i - 1];
                var v = i < b.LongLength && b[b.LongLength - i - 1];
                ret[ret.LongLength - i - 1] = a.LongLength < b.LongLength ? FullSuber(v, u, ref borrow) : FullSuber(u, v, ref borrow);
            }
            return (a.LongLength < b.LongLength, Simplify(ret)); //O(n)
        }

        /// <summary>
        /// 最简二进制模式 O(n)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static bool[] Simplify(bool[] a)
        {
            List<bool> result = new(a.Length);
            int i = 0;
            for (; i < a.Length; i++)
            {
                if (a[i]) break;
            }
            for (; i < a.Length; i++)
            {
                result.Add(a[i]);
            }
            result.TrimExcess();
            if (result.Count == 0) result.Add(false);
            return result.ToArray();
        }
    }
}
