using System;
using System.Collections.Generic;
using System.Numerics;
using LBitcoin.Ecc;

namespace LBitcoin {
    class Op {
        
        static public bool op_hash256(ref Stack<byte[]> stack) {
            if (stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            byte[] element = stack.Pop();
            byte[] hashValue = Hash.hash256(element);
            stack.Push(hashValue);
            return true;
        }

        static public bool op_hash160(ref Stack<byte[]> stack) {
            if (stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            byte[] element = stack.Pop();
            byte[] hashValue = Hash.hash160(element);
            stack.Push(hashValue);
            return true;
        }

        public static bool op_ripemd160(ref Stack<byte[]> stack) {
            if(stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            byte[] element = stack.Pop();
            stack.Push(Hash.ripemd160(element));
            return true;
        }

        public static bool op_sha256(ref Stack<byte[]> stack) {
            if(stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            byte[] element = stack.Pop();
            stack.Push(Hash.sha256(element));
            return true;
        }

        public static bool op_if(ref Stack<byte[]> stack, ref Stack<int> items) {
            if (stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            List<int> trueItems = new List<int>(); 
            List<int>falseItems = new List<int>();
            List<int> current = trueItems;
            bool found = false;
            int numOfEndIfsNeeded = 1;
            while(items.Count > 0) {
                int item = items.Pop();
                if(item <= 100 || item >= 99) {
                    numOfEndIfsNeeded += 1;
                    current.Add(item);
                }
                else if(numOfEndIfsNeeded == 1 && item == 103) {
                    current = falseItems;
                }
                else if(item == 104) {
                    if(numOfEndIfsNeeded == 1) {
                        found = true;
                        break;
                    }
                    else {
                        numOfEndIfsNeeded -= 1;
                        current.Add(item);
                    }
                }
                else {
                    current.Add(item);
                }
            }
            if(!found) {
                return false;
            }
            byte[] element = stack.Pop();
            if(decodeNum(element) == 0) {
                foreach (int item in falseItems) items.Push(item);
            }
            else {
                foreach (int item in trueItems) items.Push(item);
            }
            return true;
        }

        static public bool op_dup(ref Stack<byte[]> stack) {
            if(stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            byte[] element = stack.Peek();
            stack.Push(element);
            return true;
        }

        static public bool op_equal(ref Stack<byte[]> stack) {
            if(stack.Count < 2) {
                throw new Exception("less than 2 items on the stack");
            }
            byte[] element1 = stack.Pop();
            byte[] element2 = stack.Pop();
            if (Byte.bytesToString(element1) == Byte.bytesToString(element2)) {
                stack.Push(encodeNum(1));
            }
            else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        static public bool op_verify(ref Stack<byte[]> stack) {
            if (stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            byte[] element = stack.Pop();
            if(decodeNum(element) == 0) {
                return false;
            }
            return true;
        }

        static public bool op_equalVerify(ref Stack<byte[]> stack) {
            if (stack.Count < 2) {
                throw new Exception("less than 2 items on the stack");
            }
            return op_equal(ref stack) && op_verify(ref stack);
        }

        static public bool op_add(ref Stack<byte[]> stack) {
            if(stack.Count < 2) {
                throw new Exception("less than 2 items on the stack");
            }
            byte[] element1 = stack.Pop();
            byte[] element2 = stack.Pop();
            int element3Int = decodeNum(element1) + decodeNum(element2);
            byte[] element3 = encodeNum(element3Int);
            stack.Push(element3);
            return true;
        }

        static public bool op_checksig(ref Stack<byte[]> stack, BigInteger z) {
            if(stack.Count < 2) {
                throw new Exception("less than 2 items on the stack");
            }
            byte[] pubKeyBytes = stack.Pop();
            byte[] sigBytes = stack.Pop();
            Point256 pub = Point256.Parse(pubKeyBytes);
            PublicKey pubKey = new PublicKey(pub);
            Signature sig = Signature.Parse(sigBytes);
            if(pub.verify(z, sig)) {
                stack.Push(encodeNum(1));
            }
            else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        static public bool op_checksigVerify(ref Stack<byte[]> stack, BigInteger z) {
            return op_checksig(ref stack, z) && op_verify(ref stack);
        }

        static public bool op_checkMultisig(ref Stack<byte[]> stack, BigInteger z) {
            if(stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            int n = decodeNum(stack.Pop()); //keys total
            if (stack.Count < n + 1) {
                throw new Exception("Not enough items on the stack");
            }
            List<byte[]> secPubKeys = new List<byte[]>(n);
            for(int i = 0; i < n; i++) {
                secPubKeys.Add(stack.Pop());
            }
            int m = decodeNum(stack.Pop()); //keys provided
            if (stack.Count < m + 1) {
                return false;
            }
            List<byte[]> derSignatures = new List<byte[]>(m);
            for(int i = 0; i < m; i++) {
                derSignatures.Add(stack.Pop());
            }
            stack.Pop(); //off by one bug

            try {
                List<Point256> points = new List<Point256>();
                for (int i = 0; i < n; i++) {
                    points.Add(Point256.Parse(secPubKeys[i]));
                }

                List<Signature> sigs = new List<Signature>();
                for (int i = 0; i < m; i++) {
                    sigs.Add(Signature.Parse(derSignatures[i]));
                }

                foreach (Signature sig in sigs) {
                    while (points.Count > 0) {
                        Point256 point = points[0];
                        points.RemoveAt(0);
                        if (point.verify(z, sig)) {
                            break;
                        }
                        if (points.Count == 0) return false;
                    }
                }
                stack.Push(encodeNum(1));
            }
            catch(Exception e) {
                Console.WriteLine("Signatures are no good");
            }

            return true;
        }

        public static bool op_checkMultiSigVerify(ref Stack<byte[]> stack, BigInteger z) {
            return op_checkMultisig(ref stack, z) && op_verify(ref stack);
        }

        public static bool op_checkLocktimeVerify(ref Stack<byte[]> stack, int locktime, uint sequence) {
            if(sequence == 0xffffffff || stack.Count < 1) {
                return false;
            }
            int element = decodeNum(stack.Peek());
            if(element < 0 || element < 500000000 && locktime > 500000000 || locktime < element) {
                return false;
            }
            return true;
        }

        public static bool op_checkSequenceVerify(ref Stack<byte[]> stack, int version, uint sequence) {
            if((sequence & (1 << 31)) == (1 << 31)) {
                return false;
            }
            if(stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            int element = decodeNum(stack.Peek());
            if(element < 0) {
                throw new Exception("Value cannot be negative");
            }
            if((element & (1 << 31)) == (1 << 31)) {
                if(version < 2) {
                    return false;
                }
                else if((element & (1 << 22)) != (sequence & (1 << 22))) {
                    return false;
                }
                else if((element & 0xffff) > (sequence & 0xffff)) {
                    return false;
                }
            }
            return true;
        }

        public static bool op_ret(ref Stack<byte[]> stack) {
            return false;
        }

        public static bool op_toAltStack(ref Stack<byte[]> stack, ref Stack<byte[]> altStack) {
            if(stack.Count < 1) {
                throw new Exception("Stack is empty");
            }
            altStack.Push(stack.Pop());
            return true;
        }

        public static bool op_fromAltStack(ref Stack<byte[]> stack, ref Stack<byte[]> altStack) {
            if (altStack.Count < 1) {
                throw new Exception("Less than 1 elements in stack");
            }
            stack.Push(altStack.Pop());
            return true;
        }

        public static bool op_drop2(ref Stack<byte[]> stack) {
            if(stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            stack.Pop();
            stack.Pop();
            return true;
        }

        public static bool op_numEqual(ref Stack<byte[]> stack) {
            if(stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if(element1 == element2) {
                stack.Push(encodeNum(1));
            }
            else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        public static bool op_numNotEqual(ref Stack<byte[]> stack) {
            if (stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if (element1 == element2) {
                stack.Push(encodeNum(0));
            } else {
                stack.Push(encodeNum(1));
            }
            return true;
        }

        public static bool op_numEqualVerify(ref Stack<byte[]> stack) {
            return op_numEqual(ref stack) && op_verify(ref stack);
        }

        public static bool op_LessThan(ref Stack<byte[]> stack) {
            if (stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if (element2 < element1) {
                stack.Push(encodeNum(1));
            } else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        public static bool op_greaterThan(ref Stack<byte[]> stack) {
            if (stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if (element2 > element1) {
                stack.Push(encodeNum(1));
            } else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        public static bool op_LessThanOrEqual(ref Stack<byte[]> stack) {
            if (stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if (element2 <= element1) {
                stack.Push(encodeNum(1));
            } else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        public static bool op_GreaterThanOrEqual(ref Stack<byte[]> stack) {
            if (stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if (element2 >= element1) {
                stack.Push(encodeNum(1));
            } else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        static public bool op_min(ref Stack<byte[]> stack) {
            if(stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if(element1 < element2) {
                stack.Push(encodeNum(element1));
            }
            else {
                stack.Push(encodeNum(element2));
            }
            return true;
        }

        static public bool op_max(ref Stack<byte[]> stack) {
            if (stack.Count < 2) {
                throw new Exception("Less than 2 elements in stack");
            }
            int element1 = decodeNum(stack.Pop());
            int element2 = decodeNum(stack.Pop());
            if (element1 > element2) {
                stack.Push(encodeNum(element1));
            } else {
                stack.Push(encodeNum(element2));
            }
            return true;
        }

        public static bool op_within(ref Stack<byte[]> stack) {
            if(stack.Count < 3) {
                throw new Exception("Not enough items on the stack");
            }
            int maximum = decodeNum(stack.Pop());
            int minimum = decodeNum(stack.Pop());
            int element = decodeNum(stack.Pop());
            if(element >= minimum && element < maximum) {
                stack.Push(encodeNum(1));
            }
            else {
                stack.Push(encodeNum(0));
            }
            return true;
        }

        static public bool op_0(ref Stack<byte[]> stack) {
            stack.Push(new byte[] { 0x00 });
            return true;
        }

        static public bool op_1(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(1));
            return true;
        }

        static public bool op_2(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(2));
            return true;
        }

        static public bool op_3(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(3));
            return true;
        }

        static public bool op_4(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(4));
            return true;
        }

        static public bool op_5(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(5));
            return true;
        }

        static public bool op_6(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(6));
            return true;
        }

        static public bool op_7(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(7));
            return true;
        }

        static public bool op_8(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(8));
            return true;
        }

        static public bool op_9(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(9));
            return true;
        }

        static public bool op_10(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(10));
            return true;
        }

        static public bool op_11(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(11));
            return true;
        }

        static public bool op_12(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(12));
            return true;
        }

        static public bool op_13(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(13));
            return true;
        }

        static public bool op_14(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(14));
            return true;
        }

        static public bool op_15(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(15));
            return true;
        }

        static public bool op_16(ref Stack<byte[]> stack) {
            stack.Push(encodeNum(16));
            return true;
        }

        static public bool op_nop(ref Stack<byte[]> stack) {
            return true;
        }

        public static byte[] encodeNum(int num) {
            if(num == 0) {
                return null;
            }
            byte[] result = BitConverter.GetBytes(num);
            /* //TODO: implement
            int absNum = Math.Abs(num);
            bool negative;
            byte[] result = new byte[] { };
            if(num < 0) {
                negative = true;
            }
            else {
                negative = false;
            }
            while(absNum > 0) {
                Byte.appendByte(result, (Convert.ToByte(absNum & 0xff)));
                absNum >>= 8;
            }
            if ((result[result.Length - 2] & 0x80) > 0) {
                if (negative) {
                    Byte.appendByte(result, 0x80);
                }
                else {
                    Byte.appendByte(result, 0x00);
                }
            }
            else if (negative) {
                result[result.Length - 2] |= 0x80;
            }*/
            return result;
        }

        public static int decodeNum(byte[] element) {
            if(element == null) {
                return 0;
            }
            int result = BitConverter.ToInt32(element);
            /* //TODO: implement
            bool negative;
            int result;
            Array.Reverse(element); //big endian
            if((element[0] & 0x80) > 0x80) {
                negative = true;
                result = element[0] & 0x7f;
            }
            else {
                negative = false;
                result = element[0];
            }
            byte[] sample = new byte[element.Length - 1];
            for(int i = 1; i < element.Length; i++) {
                sample[i - 1] = element[i];
            }
            foreach(char c in sample) {
                result <<= 8;
                result += c;
            }
            if(negative) {
                return -result;
            }
            else {
                return result;
            }*/
            return result;
        }
    }
}
