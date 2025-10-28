using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MathX.Number
{
    /// <summary>
    /// Arbitrary-size integer (Z).<br/>
    /// 任意精度整数（Z）。<br/>
    /// Internal storage: magnitude bytes in little-endian in `_data`. Sign stored in `Flags` LSB (0=positive,1=negative).<br/>
    /// 内部存储：以小端序在 `_data` 中保存幅度字节。符号位保存在 `Flags` 的最低位（0=正，1=负）。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class GrandInt : IComparable, IComparable<GrandInt>, IConvertible, IEquatable<GrandInt>, IFormattable
    {
        [MarshalAs(UnmanagedType.I4)]
        private int _length; // number of used bytes in _data
        /// <summary>
        /// 
        /// Flags byte: LSB used as sign indicator (0 = positive,1 = negative).<br/>
        /// 标志字节：最低位作为符号指示（0 = 正，1 =负）。
        /// Other bits reserved for future use. /其他位保留以备将来使用。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte Flags; // flags byte
        [MarshalAs(UnmanagedType.LPArray)]
        private byte[] _data; // little-endian, least-significant byte first
        
        private const byte SignMask = 0x01; //  Sign bit mask (LSB).

        /// <summary>
        /// Serialization format: [8-byte little-endian ulong length][Flags][data bytes...]<br/>
        /// 序列化格式： [8 字节小端 ulong 长度][Flags][数据字节...]
        /// </summary>
        public GrandInt()
        {
            _data = new byte[1];
            Flags = 0;
            // initialize magnitude as zero
            _data[0] = 0;
            _length = 1;
        }

        /// <summary>
        /// Create from signed64-bit value.<br/>
        /// 使用有符号64 位整数创建。
        /// </summary>
        /// <param name="v">Signed64-bit input / 有符号64 位输入</param>
        public GrandInt(long v) : this()
        {
            if (v < 0)
            {
                Flags |= SignMask;
                FromUInt64Unchecked((ulong)(-v));
            }
            else
            {
                FromUInt64Unchecked((ulong)v);
            }
        }

        /// <summary>
        /// Create from unsigned64-bit value.<br/>
        /// 使用无符号64 位整数创建。
        /// </summary>
        /// <param name="v">Unsigned64-bit input / 无符号64 位输入</param>
        public GrandInt(ulong v) : this()
        {
            FromUInt64Unchecked(v);
        }

        /// <summary>
        /// Create from raw magnitude bytes (little-endian).<br/>
        /// 从原始幅度字节（小端）创建。<br/>
        /// `isNegative` sets the sign flag if true.<br/>
        /// 如果为 true 则设置为负数。
        /// </summary>
        /// <param name="magnitudeLittleEndian">Magnitude bytes, LSB first / 幅度字节，小端序</param>
        /// <param name="isNegative">Sign indicator / 是否为负</param>
        public GrandInt(byte[] magnitudeLittleEndian, bool isNegative = false) : this()
        {
            if (magnitudeLittleEndian == null || magnitudeLittleEndian.Length == 0)
            {
                _data = new byte[1]; _data[0] = 0; _length = 1;
            }
            else
            {
                _data = new byte[magnitudeLittleEndian.Length];
                Array.Copy(magnitudeLittleEndian, 0, _data, 0, magnitudeLittleEndian.Length);
                _length = magnitudeLittleEndian.Length;
                TrimMagnitude();
            }
            if (isNegative) Flags |= SignMask;
        }

        // --- internal helpers ---
        /// <summary>
        /// Ensure internal buffer has at least `size` capacity.<br/>
        /// 确保内部缓冲区至少拥有 `size` 大小的容量。
        /// </summary>
        /// <param name="size">Required capacity in bytes / 所需容量（字节）</param>
        private void EnsureCapacity(int size)
        {
            if (_data == null) _data = new byte[System.Math.Max(1, size)];
            else if (_data.Length < size)
            {
                int newSize = System.Math.Max(size, _data.Length * 2);
                var tmp = new byte[newSize];
                Array.Copy(_data, 0, tmp, 0, _length);
                _data = tmp;
            }
        }

        /// <summary>
        /// Append a single byte to the magnitude (least-significant side).<br/>
        /// 在幅度末尾追加单字节（最小有效字节方向）。
        /// </summary>
        /// <param name="b">Byte to append / 要追加的字节</param>
        private void AppendByte(byte b)
        {
            EnsureCapacity(_length + 1);
            _data[_length++] = b;
        }

        /// <summary>
        /// Replace internal magnitude with a copy of `src` (first `srcLen` bytes).<br/>
        /// 用 `src` 的前 `srcLen` 字节替换内部幅度（复制）。
        /// </summary>
        /// <param name="src">Source buffer / 源缓冲区</param>
        /// <param name="srcLen">Number of bytes to copy /复制的字节数</param>
        private void SetBytesFrom(byte[] src, int srcLen)
        {
            if (srcLen == 0)
            {
                _data = new byte[1]; _data[0] = 0; _length = 1; return;
            }
            _data = new byte[srcLen];
            Array.Copy(src, 0, _data, 0, srcLen);
            _length = srcLen;
        }

        /// <summary>
        /// Parse serialized layout: [8-byte length][Flags][data bytes...]<br/>
        /// 从序列化缓冲区解析出 GrandInt。期望格式如上。
        /// </summary>
        /// <param name="buf">Serialized buffer / 序列化缓冲区</param>
        /// <returns>Parsed GrandInt /解析出的 GrandInt</returns>
        public static GrandInt FromSerialized(byte[] buf)
        {
            if (buf == null || buf.Length == 0) throw new ArgumentException("buffer empty");
            if (buf.Length < 8 + 1) throw new ArgumentException("buffer too small");
            // read8-byte little-endian length
            ulong dataLen = 0UL;
            for (int i = 0; i < 8; i++) dataLen |= ((ulong)buf[i]) << (8 * i);
            // required total length =8 (len) +1 (flags) + dataLen
            ulong required = 8UL + 1UL + dataLen;
            if ((ulong)buf.Length < required) throw new FormatException("Buffer shorter than stated length");
            byte flags = buf[8];
            if (dataLen > (ulong)int.MaxValue) throw new NotSupportedException("Data length too large for this runtime");
            int dlen = (int)dataLen;
            var mag = new byte[dlen];
            if (dlen > 0) Array.Copy(buf, 9, mag, 0, dlen);
            return new GrandInt(mag, (flags & SignMask) != 0);
        }

        /// <summary>
        /// Serialize to [8-byte little-endian length][Flags][data bytes...]<br/>
        /// 序列化为带长度前缀的缓冲区。
        /// </summary>
        public byte[] ToSerialized()
        {
            int dataLen = _length;
            ulong ulen = (ulong)dataLen;
            if (ulen > (ulong)int.MaxValue) throw new NotSupportedException("Data length too large for this runtime");
            var outb = new byte[8 + 1 + dataLen];
            // write length little-endian
            for (int i = 0; i < 8; i++) outb[i] = (byte)((ulen >> (8 * i)) & 0xFF);
            outb[8] = Flags;
            if (dataLen > 0) Array.Copy(_data, 0, outb, 9, dataLen);
            return outb;
        }

        /// <summary>
        /// Initialize magnitude from unsigned64-bit value (internal helper).<br/>
        /// 从无符号64 位值初始化幅度（内部使用）。
        /// </summary>
        /// <param name="v">Unsigned64-bit value / 无符号64 位值</param>
        private void FromUInt64Unchecked(ulong v)
        {
            _length = 0;
            if (v == 0UL)
            {
                EnsureCapacity(1);
                _data[0] = 0;
                _length = 1;
                Flags &= unchecked((byte)~SignMask);
                return;
            }
            while (v != 0UL)
            {
                AppendByte((byte)(v & 0xFF));
                v >>= 8;
            }
        }

        /// <summary>
        /// Trim leading zero bytes from magnitude and normalize zero sign to positive.<br/>
        /// 修剪幅度前导零并将零的符号归一化为正。
        /// </summary>
        private void TrimMagnitude()
        {
            if (_length == 0) { _length = 1; _data[0] = 0; return; }
            int i = _length - 1;
            while (i > 0 && _data[i] == 0) i--;
            _length = i + 1;
            if (_length == 0) { _length = 1; _data[0] = 0; }
            // normalize zero sign to positive
            if (IsZero()) Flags &= unchecked((byte)~SignMask);
        }

        /// <summary>
        /// Whether value is negative.<br/>
        /// 是否为负数。
        /// </summary>
        public bool IsNegative => (Flags & SignMask) != 0;

        /// <summary>
        /// Whether value equals zero.<br/>
        /// 是否为零。
        /// </summary>
        public bool IsZero() => _length == 1 && _data[0] == 0;

        /// <summary>
        /// Number of bytes used by magnitude (little-endian).<br/>
        /// 幅度使用的字节数（小端序）。
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Return a copy of the magnitude bytes (little-endian).<br/>
        /// 返回幅度字节的副本（小端）。
        /// </summary>
        /// <returns>Byte array copy / 字节数组副本</returns>
        public byte[] GetMagnitudeBytes()
        {
            var a = new byte[_length];
            Array.Copy(_data, 0, a, 0, _length);
            return a;
        }

        /// <summary>
        /// Reports backing array capacity (heap allocation).<br/>
        /// 报告内部缓冲区容量（堆分配）。
        /// </summary>
        /// <returns>Capacity in bytes / 容量（字节）</returns>
        public int GetHeapAllocatedSize() => _data?.Length ?? 0;

        /// <summary>
        /// Compare absolute magnitudes. return -1,0,1<br/>
        /// 比较绝对幅度大小。返回 -1、0、1。
        /// </summary>
        /// <returns>Comparison result / 比较结果</returns>
        private static int CompareMagnitude(byte[] a, int alen, byte[] b, int blen)
        {
            if (alen != blen) return alen < blen ? -1 : 1;
            for (int i = alen - 1; i >= 0; i--)
            {
                if (a[i] != b[i]) return a[i] < b[i] ? -1 : 1;
            }
            return 0;
        }

        /// <summary>
        /// Compare as signed integers.<br/>
        /// 将两个 GrandInt作为有符号整数比较。<br/>
        /// Inherits documentation from IComparable&lt;GrandInt&gt;.<br/>
        ///继承自 IComparable&lt;GrandInt&gt; 的文档。
        /// </summary>
        /// <inheritdoc/>
        public int CompareTo(GrandInt other)
        {
            if (IsNegative != other.IsNegative) return IsNegative ? -1 : 1;
            int magCmp = CompareMagnitude(_data, _length, other._data, other._length);
            return IsNegative ? -magCmp : magCmp;
        }

        /// <summary>
        /// Compare to object (explicit interface implementation).<br/>
        /// 与对象比较（接口显式实现）。
        /// </summary>
        /// <inheritdoc/>
        int IComparable.CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (!(obj is GrandInt)) throw new ArgumentException("Object must be GrandInt");
            return CompareTo((GrandInt)obj);
        }

        /// <summary>
        /// Equality comparison for GrandInt.<br/>
        /// 比较两个 GrandInt 是否相等。
        /// </summary>
        public bool Equals(GrandInt other) => Flags == other.Flags && CompareMagnitude(_data, _length, other._data, other._length) == 0;
        
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is GrandInt g && Equals(g);
        
        /// <summary>
        /// Compute hash code for this GrandInt.<br/>
        ///计算 GrandInt 的哈希码。
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int h = Flags.GetHashCode();
                for (int i = 0; i < _length && i < 8; i++) h = (h * 397) ^ _data[i];
                return h;
            }
        }

        /// <summary>
        /// Add magnitudes a + b -> result (little-endian).<br/>
        /// 幅度相加（小端），返回结果数组。
        /// </summary>
        private static unsafe byte[] AddMagnitude(byte[] a, int alen, byte[] b, int blen)
        {
            int n = System.Math.Max(alen, blen);
            var res = new byte[n + 1];
            int carry = 0;
            fixed (byte* ap = a)
            fixed (byte* bp = b)
            fixed (byte* rp = res)
            {
                for (int i = 0; i < n; i++)
                {
                    int av = i < alen ? ap[i] : 0;
                    int bv = i < blen ? bp[i] : 0;
                    int sum = av + bv + carry;
                    rp[i] = (byte)(sum & 0xFF);
                    carry = (sum >> 8) & 0xFF;
                }
                if (carry != 0) rp[n] = (byte)carry;
            }
            // trim
            int len = res.Length;
            while (len > 1 && res[len - 1] == 0) len--;
            var outb = new byte[len];
            Array.Copy(res, 0, outb, 0, len);
            return outb;
        }

        /// <summary>
        /// Subtract magnitudes a - b, assume a >= b.<br/>
        /// 幅度相减（a >= b）。
        /// </summary>
        private static byte[] SubtractMagnitude(byte[] a, int alen, byte[] b, int blen)
        {
            var res = new byte[alen];
            int borrow = 0;
            for (int i = 0; i < alen; i++)
            {
                int av = a[i];
                int bv = i < blen ? b[i] : 0;
                int diff = av - bv - borrow;
                if (diff < 0)
                {
                    diff += 256;
                    borrow = 1;
                }
                else borrow = 0;
                res[i] = (byte)(diff & 0xFF);
            }
            // trim
            int len = res.Length;
            while (len > 1 && res[len - 1] == 0) len--;
            var outb = new byte[len];
            Array.Copy(res, 0, outb, 0, len);
            if (outb.Length == 0) outb = new byte[] { 0 };
            return outb;
        }

        /// <summary>
        /// Multiply magnitudes using schoolbook algorithm (little-endian).<br/>
        /// 学校乘法算法实现幅度相乘（小端）。
        /// </summary>
        private static unsafe byte[] MultiplyMagnitude(byte[] a, int alen, byte[] b, int blen)
        {
            var res = new int[alen + blen];
            fixed (byte* ap = a)
            fixed (byte* bp = b)
            fixed (int* rp = res)
            {
                for (int i = 0; i < alen; i++)
                {
                    int carry = 0;
                    for (int j = 0; j < blen; j++)
                    {
                        int idx = i + j;
                        int prod = rp[idx] + ap[i] * bp[j] + carry;
                        rp[idx] = prod & 0xFF;
                        carry = prod >> 8;
                    }
                    if (carry != 0) rp[i + blen] += carry;
                }
            }
            var outb = new byte[res.Length];
            for (int i = 0; i < res.Length; i++) outb[i] = (byte)(res[i] & 0xFF);
            int len = outb.Length;
            while (len > 1 && outb[len - 1] == 0) len--;
            var final = new byte[len];
            Array.Copy(outb, 0, final, 0, len);
            return final;
        }

        // Public arithmetic operators (signed)
        /// <summary>
        /// Add operator implementation (signed).<br/>
        /// 加法运算符实现（有符号）。
        /// </summary>
        public static GrandInt operator +(GrandInt a, GrandInt b)
        {
            // same sign -> add magnitudes
            if (a.IsNegative == b.IsNegative)
            {
                var gi = new GrandInt
                {
                    Flags = a.Flags // sign
                };
                var res = AddMagnitude(a._data, a._length, b._data, b._length);
                gi.SetBytesFrom(res, res.Length);
                gi.TrimMagnitude();
                return gi;
            }
            // different signs -> subtract smaller magnitude from larger
            int cmp = CompareMagnitude(a._data, a._length, b._data, b._length);
            if (cmp == 0)
            {
                return new GrandInt(0);
            }
            if (cmp > 0)
            {
                var gi = new GrandInt
                {
                    Flags = a.Flags // sign of a (larger)
                };
                var res = SubtractMagnitude(a._data, a._length, b._data, b._length);
                gi.SetBytesFrom(res, res.Length);
                gi.TrimMagnitude();
                return gi;
            }
            else
            {
                var gi = new GrandInt
                {
                    Flags = b.Flags
                };
                var res = SubtractMagnitude(b._data, b._length, a._data, a._length);
                gi.SetBytesFrom(res, res.Length);
                gi.TrimMagnitude();
                return gi;
            }
        }

        /// <summary>
        /// Subtraction operator (a - b).<br/>
        /// 减法运算符实现。
        /// </summary>
        public static GrandInt operator -(GrandInt a, GrandInt b)
        {
            // a - b = a + (-b)
            var nb = new GrandInt();
            nb.SetBytesFrom(b._data, b._length);
            nb.Flags = (byte)(b.Flags ^ SignMask); // flip sign
            return a + nb;
        }

        /// <summary>
        /// Multiplication operator (signed).<br/>
        ///乘法运算符实现（有符号）。
        /// </summary>
        public static GrandInt operator *(GrandInt a, GrandInt b)
        {
            var gi = new GrandInt
            {
                Flags = (byte)((a.IsNegative ^ b.IsNegative) ? SignMask : 0)
            };
            var res = MultiplyMagnitude(a._data, a._length, b._data, b._length);
            gi.SetBytesFrom(res, res.Length);
            gi.TrimMagnitude();
            return gi;
        }

        /// <summary>
        /// Divide magnitude by small uint, returning quotient and remainder.<br/>
        /// 将幅度除以小整型，返回商（幅度数组）和余数。
        /// </summary>
        private static (byte[] quotient, uint remainder) DivModMagnitudeByUInt(byte[] mag, int len, uint divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            if (len == 0) return (new byte[] { 0 }, 0);
            var qBig = new byte[len]; // big-endian digits
            ulong rem = 0;
            int qi = 0;
            for (int i = len - 1; i >= 0; i--)
            {
                rem = (rem << 8) | mag[i];
                uint qbyte = (uint)(rem / divisor);
                rem %= divisor;
                qBig[qi++] = (byte)qbyte;
            }
            // trim leading zeros in big-endian
            int s = 0; while (s < qi - 1 && qBig[s] == 0) s++;
            int outLen = qi - s;
            var qLittle = new byte[System.Math.Max(1, outLen)];
            for (int j = 0; j < outLen; j++) qLittle[j] = qBig[qi - 1 - j];
            if (qLittle.Length == 0) qLittle = new byte[] { 0 };
            // trim zeros
            int finalLen = qLittle.Length; while (finalLen > 1 && qLittle[finalLen - 1] == 0) finalLen--;
            if (finalLen != qLittle.Length)
            {
                var tmp = new byte[finalLen]; Array.Copy(qLittle, 0, tmp, 0, finalLen); qLittle = tmp;
            }
            return (qLittle, (uint)rem);
        }

        /// <summary>
        /// Convert value to decimal string.<br/>
        /// 转换为十进制字符串表示。
        /// </summary>
        public string ToDecimalString()
        {
            if (IsZero()) return "0";
            var work = new byte[_length]; Array.Copy(_data, 0, work, 0, _length);
            int workLen = _length;
            var digits = new List<char>();
            while (!(workLen == 1 && work[0] == 0))
            {
                var (q, r) = DivModMagnitudeByUInt(work, workLen, 10);
                digits.Add((char)('0' + r));
                work = q; workLen = q.Length;
            }
            digits.Reverse();
            var sb = new StringBuilder();
            if (IsNegative) sb.Append('-');
            foreach (var ch in digits) sb.Append(ch);
            return sb.ToString();
        }

        /// <summary>
        /// Returns string representation (decimal).<br/>
        /// 返回字符串表示（十进制）。
        /// </summary>
        public override string ToString() => ToDecimalString();

        /// <inheritdoc/>
        public static implicit operator GrandInt(int v) => new GrandInt(v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(long v) => new GrandInt(v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(uint v) => new GrandInt(v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(ulong v) => new GrandInt(v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(short v) => new GrandInt((long)v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(ushort v) => new GrandInt((ulong)v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(byte v) => new GrandInt((ulong)v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(sbyte v) => new GrandInt((long)v);
        /// <inheritdoc/>
        public static implicit operator GrandInt(char v) => new GrandInt((ulong)v);
        /// <inheritdoc/>
        public static explicit operator GrandInt(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d)) throw new InvalidCastException();
            bool neg = d < 0.0;
            double abs = Math.Abs(d);
            if (abs > ulong.MaxValue) throw new OverflowException();
            ulong mag = (ulong)abs;
            var gi = new GrandInt(mag);
            if (neg) gi.Flags |= SignMask;
            return gi;
        }
        /// <inheritdoc/>
        public static explicit operator GrandInt(float f) => (GrandInt)(double)f;
        /// <inheritdoc/>
        public static explicit operator GrandInt(decimal m)
        {
            bool neg = m < 0m;
            decimal abs = neg ? -m : m;
            if (abs > ulong.MaxValue) throw new OverflowException();
            ulong mag = (ulong)abs;
            var gi = new GrandInt(mag);
            if (neg) gi.Flags |= SignMask;
            return gi;
        }

        /// <summary>
        /// Create GrandInt from raw magnitude bytes (factory).<br/>
        /// 从原始幅度字节创建 GrandInt 的工厂方法。
        /// </summary>
        public static GrandInt FromMagnitude(byte[] mag, bool negative = false)
        {
            return new GrandInt(mag ?? new byte[] { 0 }, negative);
        }

        // -------------------------
        // Formatting and Parsing
        // -------------------------
        /// <summary>
        /// Format using IFormattable contract.<br/>
        /// 使用 IFormattable 合约进行格式化。继承接口文档。 
        /// </summary>
        /// <inheritdoc/>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format) || string.Equals(format, "G", StringComparison.OrdinalIgnoreCase) || string.Equals(format, "D", StringComparison.OrdinalIgnoreCase))
            {
                return ToString();
            }
            if (format.StartsWith("X", StringComparison.OrdinalIgnoreCase))
            {
                // hex output
                return (IsNegative ? "-" : "") + ToHexString();
            }
            // unknown -> fallback
            return ToString();
        }

        /// <summary>
        /// Produce hexadecimal string of magnitude (MSB first).<br/>
        /// 以十六进制形式输出幅度（最高字节优先）。
        /// </summary>
        private string ToHexString()
        {
            // produce hex from most-significant byte to least
            var sb = new StringBuilder();
            for (int i = _length - 1; i >= 0; i--)
            {
                sb.AppendFormat("{0:X2}", _data[i]);
            }
            // trim leading zeros
            var s = sb.ToString().TrimStart('0');
            if (string.IsNullOrEmpty(s)) s = "0";
            return s;
        }

        // -------------------------
        // Parsing helpers (array-based)
        // -------------------------
        /// <summary>
        /// Ensure static capacity for parsing helpers.
        /// 为解析辅助函数确保静态缓冲区容量。
        /// </summary>
        private static void EnsureCapacityStatic(ref byte[] mag, int required, int currentLen)
        {
            if (mag == null) mag = new byte[System.Math.Max(1, required)];
            else if (mag.Length < required)
            {
                int newSize = System.Math.Max(required, mag.Length * 2);
                var tmp = new byte[newSize];
                if (currentLen > 0) Array.Copy(mag, 0, tmp, 0, currentLen);
                mag = tmp;
            }
        }

        /// <summary>
        /// Multiply magnitude in-place by small uint (parsing helper).
        /// 就地将幅度乘以小整型（解析辅助）。
        /// </summary>
        private static void MultiplyInPlace(ref byte[] mag, ref int len, uint factor)
        {
            if (factor == 0)
            {
                mag = new byte[1]; mag[0] = 0; len = 1; return;
            }
            EnsureCapacityStatic(ref mag, len + 4, len);
            ulong carry = 0;
            for (int i = 0; i < len; i++)
            {
                ulong prod = (ulong)mag[i] * factor + carry;
                mag[i] = (byte)(prod & 0xFF);
                carry = prod >> 8;
            }
            int pos = len;
            while (carry != 0)
            {
                EnsureCapacityStatic(ref mag, pos + 1, len);
                mag[pos++] = (byte)(carry & 0xFF);
                carry >>= 8;
            }
            len = pos == 0 ? 1 : pos;
        }

        /// <summary>
        /// Add small uint to magnitude in-place (parsing helper).
        /// 就地将小整型加到幅度中（解析辅助）。
        /// </summary>
        private static void AddUIntInPlace(ref byte[] mag, ref int len, uint add)
        {
            EnsureCapacityStatic(ref mag, len + 4, len);
            ulong carry = add;
            int i = 0;
            while (carry != 0 && i < len)
            {
                ulong sum = (ulong)mag[i] + carry;
                mag[i] = (byte)(sum & 0xFF);
                carry = sum >> 8;
                i++;
            }
            int pos = Math.Max(len, i);
            while (carry != 0)
            {
                EnsureCapacityStatic(ref mag, pos + 1, len);
                mag[pos++] = (byte)(carry & 0xFF);
                carry >>= 8;
            }
            len = pos == 0 ? 1 : pos;
        }

        /// <summary>
        /// Try parse string into GrandInt. Supports decimal,0x hex,0b binary and '_' separators.<br/>
        /// 尝试将字符串解析为 GrandInt。支持十进制、0x 十六进制、0b 二进制以及下划线分隔。
        /// </summary>
        public static bool TryParse(string s, out GrandInt result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(s)) return false;
            string t = s.Trim();
            bool neg = false;
            if (t[0] == '+' || t[0] == '-')
            {
                neg = t[0] == '-';
                t = t.Substring(1);
                if (t.Length == 0) return false;
            }

            // hex
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                string body = t.Substring(2).Replace("_", string.Empty);
                if (body.Length == 0) return false;
                var gi = new GrandInt();
                byte[] tmp = new byte[] { 0 };
                int tmpLen = 1;
                foreach (char c in body)
                {
                    int v;
                    if (c >= '0' && c <= '9') v = c - '0';
                    else if (c >= 'A' && c <= 'F') v = 10 + (c - 'A');
                    else if (c >= 'a' && c <= 'f') v = 10 + (c - 'a');
                    else return false;
                    MultiplyInPlace(ref tmp, ref tmpLen, 16);
                    AddUIntInPlace(ref tmp, ref tmpLen, (uint)v);
                }
                // copy tmp into gi
                gi._data = tmp; gi._length = gi._data.Length; gi.TrimMagnitude();
                if (neg) gi.Flags |= SignMask; else gi.Flags &= unchecked((byte)~SignMask);
                result = gi;
                return true;
            }

            // binary
            if (t.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                string body = t.Substring(2).Replace("_", string.Empty);
                if (body.Length == 0) return false;
                var gi = new GrandInt();
                byte[] tmp = new byte[] { 0 };
                int tmpLen = 1;
                foreach (char c in body)
                {
                    if (c != '0' && c != '1') return false;
                    MultiplyInPlace(ref tmp, ref tmpLen, 2);
                    if (c == '1') { AddUIntInPlace(ref tmp, ref tmpLen, 1); }
                }
                gi._data = tmp; gi._length = gi._data.Length; gi.TrimMagnitude();
                if (neg) gi.Flags |= SignMask; else gi.Flags &= unchecked((byte)~SignMask);
                result = gi;
                return true;
            }

            // decimal
            string dec = t.Replace("_", string.Empty);
            if (dec.Length == 0) return false;
            var g = new GrandInt();
            byte[] tmp2 = new byte[] { 0 };
            int tmp2Len = 1;
            foreach (char c in dec)
            {
                if (c < '0' || c > '9') return false;
                MultiplyInPlace(ref tmp2, ref tmp2Len, 10);
                AddUIntInPlace(ref tmp2, ref tmp2Len, (uint)(c - '0'));
            }
            g._data = tmp2; g._length = g._data.Length; g.TrimMagnitude();
            if (neg) g.Flags |= SignMask; else g.Flags &= unchecked((byte)~SignMask);
            result = g;
            return true;
        }

        /// <summary>
        /// Parse string into GrandInt or throw on failure.<br/>
        /// 将字符串解析为 GrandInt，失败抛出异常。
        /// </summary>
        public static GrandInt Parse(string s)
        {
            if (!TryParse(s, out var gi)) throw new FormatException();
            return gi;
        }

        // -------------------------
        // IConvertible implementation
        // -------------------------
        /// <inheritdoc/>
        public TypeCode GetTypeCode() => TypeCode.Object;

        /// <summary>
        /// Try to obtain the unsigned64-bit absolute value of this GrandInt. Used by numeric conversions.
        /// 尝试获取此 GrandInt 的无符号64 位绝对值。用于数值转换。
        /// </summary>
        private bool TryGetUInt64(out ulong value)
        {
            value = 0UL;
            // process from most-significant to least
            for (int i = _length - 1; i >= 0; i--)
            {
                // check overflow before shift
                if (value > (ulong.MaxValue >> 8)) return false;
                value = (value << 8) | _data[i];
            }
            return true;
        }

        /// <inheritdoc/>
        public bool ToBoolean(IFormatProvider provider) => !IsZero();
        /// <inheritdoc/>
        public char ToChar(IFormatProvider provider) => (char)ToInt32(provider);
        /// <inheritdoc/>
        public sbyte ToSByte(IFormatProvider provider) => (sbyte)ToInt32(provider);
        /// <inheritdoc/>
        public byte ToByte(IFormatProvider provider)
        {
            if (!TryGetUInt64(out var v)) throw new OverflowException();
            if (v > byte.MaxValue) throw new OverflowException();
            return (byte)v;
        }
        /// <inheritdoc/>
        public short ToInt16(IFormatProvider provider) => (short)ToInt32(provider);
        /// <inheritdoc/>
        public ushort ToUInt16(IFormatProvider provider)
        {
            if (!TryGetUInt64(out var v)) throw new OverflowException();
            if (v > ushort.MaxValue) throw new OverflowException();
            return (ushort)v;
        }
        /// <inheritdoc/>
        public int ToInt32(IFormatProvider provider)
        {
            if (IsNegative)
            {
                if (!TryGetUInt64(out var av)) throw new OverflowException();
                // av is absolute value
                if (av > (ulong)int.MaxValue + 1UL) throw new OverflowException();
                long res = (long)av;
                if (av == (ulong)int.MaxValue + 1UL) return int.MinValue;
                return (int)(-res);
            }
            else
            {
                if (!TryGetUInt64(out var v)) throw new OverflowException();
                if (v > int.MaxValue) throw new OverflowException();
                return (int)v;
            }
        }
        /// <inheritdoc/>
        public uint ToUInt32(IFormatProvider provider)
        {
            if (!TryGetUInt64(out var v)) throw new OverflowException();
            if (v > uint.MaxValue) throw new OverflowException();
            return (uint)v;
        }
        /// <inheritdoc/>
        public long ToInt64(IFormatProvider provider)
        {
            if (IsNegative)
            {
                if (!TryGetUInt64(out var av)) throw new OverflowException();
                // allow long.MinValue
                if (av > (ulong)long.MaxValue + 1UL) throw new OverflowException();
                if (av == (ulong)long.MaxValue + 1UL) return long.MinValue;
                return -(long)av;
            }
            else
            {
                if (!TryGetUInt64(out var v)) throw new OverflowException();
                if (v > (ulong)long.MaxValue) throw new OverflowException();
                return (long)v;
            }
        }
        /// <inheritdoc/>
        public ulong ToUInt64(IFormatProvider provider)
        {
            if (!TryGetUInt64(out var v)) throw new OverflowException();
            return v;
        }
        /// <inheritdoc/>
        public float ToSingle(IFormatProvider provider)
        {
            double d = ToDouble(provider);
            return (float)d;
        }
        /// <inheritdoc/>
        public double ToDouble(IFormatProvider provider)
        {
            double v = 0.0;
            for (int i = _length - 1; i >= 0; i--)
            {
                v = v * 256.0 + _data[i];
            }
            return IsNegative ? -v : v;
        }
        /// <inheritdoc/>
        public decimal ToDecimal(IFormatProvider provider)
        {
            decimal v = 0m;
            for (int i = _length - 1; i >= 0; i--)
            {
                v = v * 256m + _data[i];
            }
            return IsNegative ? -v : v;
        }
        /// <inheritdoc/>
        public DateTime ToDateTime(IFormatProvider provider) => throw new InvalidCastException();
        /// <inheritdoc/>
        public string ToString(IFormatProvider provider) => ToString();
        /// <inheritdoc/>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(string)) return ToString(provider);
            if (conversionType == typeof(double)) return ToDouble(provider);
            if (conversionType == typeof(float)) return ToSingle(provider);
            if (conversionType == typeof(decimal)) return ToDecimal(provider);
            // fallback: attempt change via double
            var d = ToDouble(provider);
            return Convert.ChangeType(d, conversionType, provider);
        }
    }
}
