using System;
using System.Collections.Generic;
using System.IO;

namespace LBitcoin {
    class TxIn {

        byte[] prevTxid_;
        int prevIndex_;
        public Script scriptSig_;
        UInt32 sequence_;
        public List<byte[]> witness_;

        public byte[] getPrevTxid() {
            return prevTxid_;
        }

        public int getPrevIndex() {
            return this.prevIndex_;
        }

        public Script getScriptSig() {
            return this.scriptSig_;
        }

        public uint getSequence() {
            return this.sequence_;
        }

        public TxIn(byte[] prevTxid, int prevIndex, Script scriptSig = null, uint sequence = 0) {
            sequence_ = sequence == 0 ? 0xffffffff : sequence;

            if (scriptSig == null) {
                scriptSig_ = new Script();
            } else {
                this.scriptSig_ = scriptSig;
            }
            this.prevTxid_ = prevTxid;
            this.prevIndex_ = prevIndex;
        }

        public byte[] Serialise() {
            byte[] littleEndianTxid = new byte[prevTxid_.Length];
            Array.Copy(prevTxid_, littleEndianTxid, prevTxid_.Length);
            Array.Reverse(littleEndianTxid);
            byte[] result = littleEndianTxid;
            result = Byte.join(result, Byte.intToLittleEndian(prevIndex_, 4));
            result = Byte.join(result, this.scriptSig_.Serialise()); //change this to Serialise method
            result = Byte.join(result, BitConverter.GetBytes(sequence_));
            return result;
        }

        public static TxIn Parse(Stream s) {
            /********previous txid********/
            byte[] txid = new byte[32];
            s.Read(txid, 0, 32);
            Array.Reverse(txid); //store in little endian
            /********previous txindex********/
            byte[] txindex = new byte[4];
            s.Read(txindex, 0, 4);
            int txindexInt = BitConverter.ToInt32(txindex);
            /********scriptsig********/
            int scriptSigLength = Helper.getVarIntLength(s);
            byte[] length = Helper.encodeVarInt(scriptSigLength);
            byte[] scriptSig = new byte[scriptSigLength];
            s.Read(scriptSig, 0, scriptSigLength);
            /********sequence********/
            byte[] sequenceBytes = new byte[4];
            s.Read(sequenceBytes, 0, 4);
            uint sequence = BitConverter.ToUInt32(sequenceBytes);
            Stream scriptSigStream = new MemoryStream(Byte.join(length, scriptSig));
            TxIn input = new TxIn(txid, txindexInt, Script.Parse(scriptSigStream), sequence);
            return input;
        }

        public void getData() {
            /********previous txid********/
            Console.WriteLine("\t\ttxid {0}", Byte.bytesToString(prevTxid_));
            /********previous txindex********/
            Console.WriteLine("\t\ttxindex {0}", this.prevIndex_);
            /********scriptsig********/
            if (scriptSig_.Serialise().Length == 1 && scriptSig_.Serialise()[0] == 0x00) {
                /*segwit*/
                Console.WriteLine("\t\tScriptsig: (none)");
            } else {
                Console.WriteLine("\t\tScriptsig {0}", Byte.bytesToString(this.scriptSig_.Serialise()));
            }
            /********witness********/
            if (witness_ != null) {
                Console.Write("\t\tWitness: ");
                foreach (byte[] bytes in witness_)
                    Console.Write(Byte.bytesToString(bytes));
                Console.WriteLine();
            }
            /********sequence********/
            Console.WriteLine("\t\tSequence {0}", Byte.bytesToString(BitConverter.GetBytes(sequence_)));
        }

        Transaction fetchTx(bool testnet = false) {
            return txFetcher.fetch(Byte.bytesToString(prevTxid_), testnet: testnet);
        }

        public UInt64 Amount(bool testnet = false) {
            Transaction tx = this.fetchTx(testnet);
            return tx.TxOuts[this.prevIndex_].Amount();
        }

        /*Get the ScriptPubKey by looking up the tx hash*/
        public Script scriptPubKey(bool testnet = false) {
            Transaction tx = this.fetchTx(testnet);
            Script scriptPubKey = tx.TxOuts[prevIndex_].ScriptPubKey;
            return scriptPubKey;
        }
    }
}
