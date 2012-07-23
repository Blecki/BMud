using System;
using System.Text;
using System.Collections.Generic;

namespace MudEngine2012
{
    public class WriteOnlyDatagram
    {
        private List<byte> buffer;
        private int bitIndex = 0;
        private int maxBitIndex = 0;

        public WriteOnlyDatagram()
        {
            buffer = new List<byte>();
        }

        public int BitIndex
        {
            get
            {
                return bitIndex;
            }
            set
            {
                if (value < 0 || (value + 7) / 8 > buffer.Count) throw new ArgumentOutOfRangeException("Unable to set the bit index outside of the buffer size.");
                bitIndex = value;
            }
        }

        public int MaxBitIndex
        {
            get
            {
                return maxBitIndex;
            }
            set
            {
                if (value < 0 || (value + 7) / 8 > buffer.Count) throw new ArgumentOutOfRangeException("Unable to set the max bit index outside of the buffer size.");
                maxBitIndex = value;
            }
        }

        public int LengthInBytes
        {
            get
            {
                return buffer.Count;
            }
        }

        public byte[] BufferAsArray
        {
            get
            {
                return buffer.ToArray();
            }
        }

        /// <summary>
        /// Expands the buffer size appending bytes so that the write functions don't overflow.
        /// Records the furthest write in the maxBitIndex
        /// </summary>
        /// <param name="bits">The number of bits to allocate and record.</param>
        private void ExpandBuffer(int bits)
        {
            if (bits < 1) throw new ArgumentOutOfRangeException("bits must be greater than 0");
            while ((bitIndex + bits + 7) / 8 > buffer.Count) buffer.Add(new byte());
            maxBitIndex = Math.Max(maxBitIndex, bitIndex + bits);
        }

        /// <summary>
        /// Writes a single bit either 0 or 1 into the buffer.
        /// </summary>
        /// <param name="value">The boolean value to write.</param>
        public void WriteBool(bool value)
        {
            ExpandBuffer(1);
            if (value) buffer[bitIndex / 8] |= (byte)(1 << (7 - bitIndex % 8));
            ++bitIndex;
        }

        /// <summary>
        /// Writes an n bit unsigned integer into the buffer.
        /// </summary>
        /// <param name="value">The unsigned integer value to write.</param>
        /// <param name="bits">The number of bits to use.</param>
        public void WriteUInt(uint value, int bits)
        {
            if (bits < 1 || bits > 32) throw new ArgumentOutOfRangeException("bits must be in the range (0, 32].");
            if (bits != 32 && value > (0x1 << bits) - 1) throw new ArgumentOutOfRangeException("Value does not fit into " + bits.ToString() + " bits.");

            ExpandBuffer(bits);

            value <<= 32 - bits;

            int offset = bitIndex % 8;
            buffer[bitIndex / 8] |= (byte)(value >> 24 + offset);
            if (offset + bits > 8)
            {
                buffer[bitIndex / 8 + 1] |= (byte)(value >> 16 + offset);
                if (offset + bits > 16)
                {
                    buffer[bitIndex / 8 + 2] |= (byte)(value >> 8 + offset);
                    if (offset + bits > 24)
                    {
                        buffer[bitIndex / 8 + 3] |= (byte)(value >> offset);
                        if (offset + bits > 32)
                        {
                            buffer[bitIndex / 8 + 4] |= (byte)(value << 8 - offset);
                        }
                    }
                }
            }
            bitIndex += bits;
        }

        /// <summary>
        /// Appends a byte array to the buffer.
        /// </summary>
        /// <param name="value">The byte array to write.</param>
        public void WriteBytes(byte[] value)
        {
            foreach (var b in value) WriteUInt((uint)b, 8);
        }

        public void WriteString(String str)
        {
            var bytes = Encoding.Unicode.GetBytes(str);
            WriteUInt((uint)bytes.Length, 16);
            WriteBytes(bytes);
        }
    }
}