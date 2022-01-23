using System;
using System.Collections;
using System.Text;
using System.IO;
using LBitcoin.Networking;

namespace LBitcoin {

    /// <summary>
    /// Creates a bloom filter for fetching transactions from a lightweight client.
    /// </summary>
    public class BloomFilter {

        static readonly uint BIP37CONST = 0xfba4c795u;

        BitArray bitfield_;
        uint size_;
        uint tweak_;
        uint functionCount_;

        public uint Size { get { return size_; } }

        public uint Tweak { get { return tweak_; } }

        public uint FunctionCount { get { return functionCount_; } }

        /// <summary>
        /// Constructor. Creates a new bloom filter.
        /// </summary>
        /// <param name="size">Size of filter.</param>
        /// <param name="functionCount">Rounds of functions.</param>
        /// <param name="tweak">Tweak modifyer.</param>
        /// <param name="filterBytes">Bytes to add to filter.</param>
        public BloomFilter(uint size, uint functionCount, uint tweak, byte[] filterBytes = null) {

            size_ = size;
            functionCount_ = functionCount;
            tweak_ = tweak;
            if(filterBytes == null) {
                bitfield_ = new BitArray((int)size * 8);
            }
            else {
                bitfield_ = new BitArray(filterBytes);
            }
        }

        /// <summary>
        /// Add data to the filter.
        /// </summary>
        public void Add(byte[] data) {
            for(uint i = 0; i < functionCount_; i++) {
                uint seed = (i * BIP37CONST) + tweak_;
                Murmur3 murmur3 = new Murmur3(seed);
                Stream s = new MemoryStream(data);
                uint hash = murmur3.Hash(s);
                uint bit = Helper.mod(hash, size_ * 8);
                bitfield_[(int)bit] = true;
            }
        }

        public byte[] FilterBytes() {
            return Helper.bitArrayToBytes(bitfield_);
        }

        public GenericMessage FilterLoad(uint flag = 1) {
            byte[] payload = Byte.encodeVarInt((int)size_);
            payload = Byte.join(payload, this.FilterBytes());
            payload = Byte.join(payload, BitConverter.GetBytes(functionCount_));
            payload = Byte.join(payload, BitConverter.GetBytes(tweak_));
            payload = Byte.appendByte(payload, Convert.ToByte(flag));
            return new GenericMessage(Encoding.ASCII.GetBytes("filterload"), payload);
        }
    }
}
