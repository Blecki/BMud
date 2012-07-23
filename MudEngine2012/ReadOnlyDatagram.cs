using System;
using System.Text;
using System.Collections.Generic;

namespace MudEngine2012
{
    public class ReadOnlyDatagram
    {
        private byte[] buffer;
        private int bitIndex = 0;

        public ReadOnlyDatagram(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public int BitIndex
        {
            get
            {
                return bitIndex;
            }
            set
            {
                if (value < 0 || (value + 7) / 8 > buffer.Length) throw new ArgumentOutOfRangeException("Unable to set the bit index outside of the buffer size.");
                bitIndex = value;
            }
        }

        public bool More
        {
            get
            {
                return bitIndex < buffer.Length * 8;
            }
        }
                
        public bool ReadBytes(byte[] into, uint count)
        {
            for (uint i = 0; i < count; ++i)
            {
                uint b = 0;
                if (!ReadUInt(out b, 8)) return false;
                into[i] = (byte)b;
            }
            return true;
        }

        /// <summary>
        /// Reads one bit from the buffer.
        /// </summary>
        /// <param name="value">Boolean.</param>
        /// <returns>false on error.</returns>
        public bool ReadBool(out bool value)
        {
            value = false;
            if ((bitIndex + 1 + 7) / 8 > buffer.Length)
            {
                return false;
            }
            value = ((buffer[bitIndex / 8] >> (7 - bitIndex % 8)) & 0x1) == 1;
            ++bitIndex;
            return true;
        }

        /// <summary>
        /// Reads an n bit custom unsigned integer from the buffer.
        /// </summary>
        /// <param name="value">Unsigned Integer.</param>
        /// <param name="bits">The number of bits used to write.</param>
        /// <returns>false on error.</returns>
        public bool ReadUInt(out uint value, int bits)
        {
            if (bits < 1 || bits > 32) throw new ArgumentOutOfRangeException("bits must be in the range (0, 32].");

            value = 0;
            if ((bitIndex + bits + 7) / 8 > buffer.Length)
            {
                return false;
            }

            int offset = bitIndex % 8;
            value = (uint)buffer[bitIndex / 8] << 24 + offset;
            if (offset + bits > 8)
            {
                value |= (uint)buffer[bitIndex / 8 + 1] << 16 + offset;
                if (offset + bits > 16)
                {
                    value |= (uint)buffer[bitIndex / 8 + 2] << 8 + offset;
                    if (offset + bits > 24)
                    {
                        value |= (uint)buffer[bitIndex / 8 + 3] << offset;
                        if (offset + bits > 32)
                        {
                            value |= (uint)buffer[bitIndex / 8 + 4] >> 8 - offset;
                        }
                    }
                }
            }

            value >>= 32 - bits;
            bitIndex += bits;
            return true;
        }

        public bool ReadString(out String str)
        {
            str = "";
            uint length = 0;
            if (!ReadUInt(out length, 16)) return false;
            var bytes = new byte[length];
            if (!ReadBytes(bytes, length)) return false;
            str = Encoding.Unicode.GetString(bytes);
            return true;
        }
    }
}