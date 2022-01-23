using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Numerics;
using LBitcoin;

namespace LBitcoin {

    /// <summary>
    /// Custom stack for bitcoin operations.
    /// </summary>
    public class BitcoinStack : IEnumerable<byte[]> {

        static Stack<byte[]> stack_ = new Stack<byte[]>();
        static Stack<byte[]> altstack_ = new Stack<byte[]>();

        public IEnumerator<byte[]> GetEnumerator() {
            foreach(byte[] element in stack_) {
                yield return element;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public byte[] this[int index] {
            get {
                if (index < 0 || index >= stack_.Count)
                    throw new IndexOutOfRangeException("Index out of range");

                return stack_.ElementAt(index);
            }
        }

        public int Count { get { return stack_.Count; } }

        public Stack<byte[]> Stack { get { return stack_; } }

        public Stack<byte[]> AltStack { get { return altstack_; } }


        public BitcoinStack(Stack<byte[]> stack) {
            stack_ = stack;
        }

        public override string ToString() {
            string str = "";
            foreach(byte[] items in stack_) {
                str += Byte.bytesToString(items) + "\n";
            }
            return str;
        }

        public void Push(byte[] data) {
            stack_.Push(data);
        }

        public byte[] Pop() {
            return stack_.Pop();
        }

        public byte[] Peek() {
            return stack_.Peek();
        }

        public bool ExecCommand(Opcodes opcode) {
            return stackOps[opcode]();
        }

        public bool ExecCommand(Opcodes opcode, BigInteger z) {
            return sigOps[opcode](z);
        }

        public bool ExecCommand(Opcodes opcode, ref Stack<byte[]> altstack) {
            altstack_ = altstack;
            bool result = altstackOps[opcode]();
            altstack = altstack_;
            return result;
        }


        Dictionary<Opcodes, Func<BigInteger, bool>> sigOps =
            new Dictionary<Opcodes, Func<BigInteger, bool>> {

                { Opcodes.OP_CHECKMULTISIG, (BigInteger z) => Op.op_checkMultisig(ref stack_, z) },
                { Opcodes.OP_CHECKMULTISIGVERIFY, (BigInteger z) => Op.op_checkMultiSigVerify(ref stack_, z) },
                { Opcodes.OP_CHECKSIG, (BigInteger z) => Op.op_checksig(ref stack_, z) },
                { Opcodes.OP_CHECKSIGVERIFY, (BigInteger z) => Op.op_checksigVerify(ref stack_, z) },

            };


        Dictionary<Opcodes, Func<bool>> altstackOps =
            new Dictionary<Opcodes, Func<bool>> {

                { Opcodes.OP_TOALTSTACK, () => Op.op_toAltStack(ref stack_, ref altstack_) },
                { Opcodes.OP_FROMALTSTACK, () => Op.op_fromAltStack(ref stack_, ref altstack_) },

            };


        Dictionary<Opcodes, Func<bool>> stackOps = new Dictionary<Opcodes, Func<bool>> {

            
                //numeric
                { Opcodes.OP_0, () => Op.op_0(ref stack_) },
                { Opcodes.OP_1, () => Op.op_1(ref stack_) },
                { Opcodes.OP_2, () => Op.op_2(ref stack_) },
                { Opcodes.OP_3, () => Op.op_3(ref stack_) },
                { Opcodes.OP_4, () => Op.op_4(ref stack_) },
                { Opcodes.OP_5, () => Op.op_5(ref stack_) },
                { Opcodes.OP_6, () => Op.op_6(ref stack_) },
                { Opcodes.OP_7, () => Op.op_7(ref stack_) },
                { Opcodes.OP_8, () => Op.op_8(ref stack_) },
                { Opcodes.OP_9, () => Op.op_9(ref stack_) },
                { Opcodes.OP_10, () => Op.op_10(ref stack_) },
                { Opcodes.OP_11, () => Op.op_11(ref stack_) },
                { Opcodes.OP_12, () => Op.op_12(ref stack_) },
                { Opcodes.OP_13, () => Op.op_13(ref stack_) },
                { Opcodes.OP_14, () => Op.op_14(ref stack_) },
                { Opcodes.OP_15, () => Op.op_15(ref stack_) },
                { Opcodes.OP_16, () => Op.op_16(ref stack_) },

                { Opcodes.OP_DUP, () => Op.op_dup(ref stack_) },
                { Opcodes.OP_ADD, () => Op.op_add(ref stack_) },
                { Opcodes.OP_HASH160, () => Op.op_hash160(ref stack_) },
                { Opcodes.OP_SHA256, () => Op.op_sha256(ref stack_) },
                { Opcodes.OP_RIPEMD160, () => Op.op_ripemd160(ref stack_) },
                { Opcodes.OP_HASH256, () => Op.op_hash256(ref stack_) },
                { Opcodes.OP_EQUALVERIFY, () => Op.op_equalVerify(ref stack_) },
                { Opcodes.OP_EQUAL, () => Op.op_equal(ref stack_) },
                { Opcodes.OP_VERIFY, () => Op.op_verify(ref stack_) },

        };

    }
}
