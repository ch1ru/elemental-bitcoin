using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LBitcoin {

    /// <summary>
    /// Block class for use in lightweight clients.
    /// </summary>
    class MerkleBlock : Block {

        int total_;
        byte[] flags_;

        /// <summary>
        /// Constructor. Creates a merkle block from same parameters as <see cref="Block"/>.
        /// Additionally requiring flags.
        /// </summary>
        public MerkleBlock (uint version, 
            byte[] prevBlock, byte[] merkleRoot, 
            int timestamp, byte[] bits, 
            byte[] nonce, int total, 
            List<byte[]> hashes, byte[] flags)
            : base(version, prevBlock, merkleRoot, timestamp, bits, nonce, hashes) {

            base.version_ = version; 
            base.prevBlock_ = prevBlock;
            base.merkleRoot_ = merkleRoot; 
            base.timestamp_ = timestamp;
            base.bits_ = bits; 
            base.nonce_ = nonce;
            base.hashes_ = hashes;

            this.total_ = total; 
            this.flags_ = flags;
        }

        public int TotalTxs {
            get { return total_; }
        }

        public byte[] Flags {
            get { return flags_; }
        }

        /// <summary>
        /// Parse a Merkle block from a stream.
        /// </summary>
        /// <returns><see cref="MerkleBlock"/> object.</returns>
        new public static MerkleBlock Parse(Stream s) {
            byte[] versionBytes = new byte[4];
            s.Read(versionBytes, 0, 4);
            Array.Reverse(versionBytes);
            uint version = BitConverter.ToUInt32(versionBytes);
            byte[] prevBlock = new byte[32];
            s.Read(prevBlock, 0, 32);
            byte[] merkleRoot = new byte[32];
            s.Read(merkleRoot, 0, 32);
            byte[] timestampBytes = new byte[4];
            s.Read(timestampBytes, 0, 4);
            int timestamp = BitConverter.ToInt32(timestampBytes);
            byte[] bits = new byte[4];
            s.Read(bits, 0, 4);
            byte[] nonce = new byte[4];
            s.Read(nonce, 0, 4);
            byte[] txTotalBytes = new byte[4];
            s.Read(txTotalBytes, 0, 4);
            int total = BitConverter.ToInt32(txTotalBytes);
            int numOfHashes = Helper.getVarIntLength(s);
            List<byte[]> hashes = new List<byte[]>();
            for(int i = 0; i < numOfHashes; i++) {
                byte[] hash = new byte[32];
                s.Read(hash, 0, 32);
                hashes.Add(hash);
            }
            int flagsLength = Helper.getVarIntLength(s);
            byte[] flags = new byte[flagsLength];
            s.Read(flags, 0, flagsLength);
            MerkleBlock mrklBlock = new MerkleBlock(version, prevBlock, merkleRoot, 
                timestamp, bits, nonce, total, hashes, flags);
            return mrklBlock;
        }

        public bool isValid() {
            BitArray flagBits = new BitArray(flags_);
            List<byte[]> hashesBigEndian = new List<byte[]>();
            foreach(byte[] hash in hashes_) {
                hashesBigEndian.Add(hash);
            }
            MerkleTree mrklTree = new MerkleTree(total_);
            mrklTree.populateTree(flagBits, hashesBigEndian);
            byte[] computedRoot = mrklTree.root();
            return Byte.bytesToString(computedRoot) == Byte.bytesToString(merkleRoot_);
        }
    }
}
