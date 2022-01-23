using System;
using System.Collections;
using System.Text;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;

static class Helper {


    public static int TWO_WEEKS = 60 * 60 * 24 * 14;
    public static BigInteger MAX_TARGET = 0xffff * BigInteger.Pow(256, 0x1d - 3);

    public static byte[] CalculateNewBits(byte[] prevBits, int timeDifferential) {
        if (timeDifferential > TWO_WEEKS * 4) {
            timeDifferential = TWO_WEEKS * 4; //can be no greater than 4 times previous difficulty
        } else if (timeDifferential < TWO_WEEKS / 4) {
            timeDifferential = TWO_WEEKS / 4; //can be no less than 4 times previous difficulty
        }
        BigInteger newTarget = BitsToTarget(prevBits) * (timeDifferential / TWO_WEEKS);

        if (newTarget > MAX_TARGET) {
            newTarget = MAX_TARGET;
        }
        return TargetToBits(newTarget);
    }

    public static BigInteger BitsToTarget(byte[] bits) {
        byte exponent = bits[bits.Length - 1];
        byte[] coefBytes = bits[0..3];

        BigInteger coef = new BigInteger(coefBytes, true);
        BigInteger target = coef * BigInteger.Pow(256, exponent - 3);
        return target;
    }

    public static byte[] TargetToBits(BigInteger target) {
        byte[] rawBytes = target.ToByteArray();
        byte[] coef;
        int exponent;
        if (rawBytes[0] > 0x7f) {
            exponent = Convert.ToByte(rawBytes.Length + 1);
            coef = Byte.prependByte(rawBytes[0..3], 0x00);
        } else {
            exponent = Convert.ToByte(rawBytes.Length);
            coef = rawBytes[0..3];
        }
        Array.Reverse(coef); //little endian
        byte[] bits = Byte.appendByte(coef, (byte)exponent);
        return bits; ;
    }

    public static byte[] MerkleParent(byte[] hash1, byte[] hash2) {
        return Hash.hash256(Byte.join(hash1, hash2));
    }

    public static bool Pop(ref BitArray bits) {
        bool returnFlag = bits[0];
        bool[] boolArray = new bool[bits.Length - 1];
        for (int i = 1; i < bits.Length; i++) {
            boolArray[i - 1] = bits[i];
        }
        BitArray bitArray = new BitArray(boolArray);
        bits = bitArray;
        return returnFlag;
    }


    /*Gets a variable length from a stream*/
    public static int getVarIntLength(Stream s) {
        byte[] varint = new byte[4];
        s.Read(varint, 0, 1); //read first byte
        int num = 1;
        if (varint[0] == 0xff) { //length in 8 bytes
            try {
                num = BitConverter.ToInt32(varint);
            } catch (Exception e) {
                Console.WriteLine(e.Message); //int is too big to be stored in 32 bytes
                Console.ReadLine();
            }
        } else if (varint[0] == 0xfe) { //length in 4 bytes
            s.Read(varint, 0, 4);
            num = BitConverter.ToInt32(varint);
        } else if (varint[0] == 0xfd) { //length in 2 bytes
            s.Read(varint, 0, 2);
            num = BitConverter.ToInt32(varint);
        } else { //varint is 1 byte
            num = BitConverter.ToInt32(varint);
        }
        return num;
    }

    public static byte[] encodeVarInt(long i) {
        byte[] length = new byte[] { };
        byte prefix;
        if (i < 0xFD) {
            length = new byte[1];
            length[0] = Convert.ToByte(i);
            return length;
        } else if (i <= 0xffff) {
            length = new byte[3];
            prefix = 0xFD;
        } else if (i <= 0xffffffff) {
            length = new byte[5];
            prefix = 0xFE;
        } else {
            length = new byte[9];
            prefix = 0xFF;
        }
        byte[] len = BitConverter.GetBytes(i);
        length = Byte.prependByte(len, prefix);
        return length;
    }

    public static byte[] encodeVarStr(string m) {
        byte[] length = encodeVarInt(m.Length);
        byte[] mBytes = Encoding.ASCII.GetBytes(m);
        return Byte.join(length, mBytes);
    }

    public static int getIntFromBitArray(BitArray bitArray) {
        int value = 0;

        /*lsb on the right*/
        for (int i = 0; i < bitArray.Count; i++) {
            if (bitArray[bitArray.Count - 1 - i]) {
                value += Convert.ToInt16(Math.Pow(2, i));
            }
        }

        return value;
    }

    public static byte[] bitArrayToBytes(BitArray bitArray, int unitSize = 8) {

        int numOfBytes = bitArray.Length / unitSize;

        byte[] result = new byte[numOfBytes];
        for (int i = 0; i < bitArray.Length; i++) {
            int byteIndex = i / unitSize;
            int bitIndex = i % unitSize;
            if (bitArray[i]) {
                result[byteIndex] |= Convert.ToByte(1 << bitIndex);
            }
        }
        return result;
    }

    public static int[] BitarrayToInt(BitArray bitArray, int unitSize) {
        int numOfElements = bitArray.Length / unitSize;
        if (numOfElements % unitSize != 0) {
            throw new Exception("unit size doesn't match length of the bit array");
        }
        int size = 0;
        int[] elements = new int[numOfElements];
        for (int i = 0; i < numOfElements; i++) {
            for (int j = 0; j < unitSize; j++) {
                if (bitArray.Get(j)) {
                    size += (int)Math.Pow(2, j);
                }
            }
            elements[i] = size;
            size = 0;
            bitArray.LeftShift(unitSize);
        }
        return elements;
    }

    public static BitArray join(BitArray array1, BitArray array2) {

        if (array1 == null) {
            return array2;
        } else if (array2 == null) {
            return array1;
        }

        BitArray newBitArray = new BitArray(array1.Count + array2.Count);
        for (int i = 0; i < array1.Count; i++) {
            newBitArray[i] = array1[i];
        }
        for (int i = 0; i < array2.Count; i++) {
            newBitArray[i + array1.Count] = array2[i];
        }
        return newBitArray;
    }

    public static uint mod(uint a, uint n) {
        if (n == 0) return 1;
        uint result = a % n;
        if ((result < 0 && n > 0) || (result > 0 && n < 0)) {
            result += n;
        }
        return result;
    }


    public static BigInteger Sqrt(this BigInteger n) {
        if (n == 0) return 0;
        if (n > 0) {
            int bitLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(n, 2)));
            BigInteger root = BigInteger.One << (bitLength / 2);

            while (!isSqrt(n, root)) {
                root += n / root;
                root /= 2;
            }

            return root;
        }

        throw new ArithmeticException("NaN");
    }

    private static Boolean isSqrt(BigInteger n, BigInteger root) {
        BigInteger lowerBound = root * root;
        BigInteger upperBound = (root + 1) * (root + 1);

        return (n >= lowerBound && n < upperBound);
    }

    public static BitArray reverseWordBits(BitArray array) {
        BitArray entropyBits = null;

        int d = 0;
        while (d <= array.Length - 8) {
            bool[] word = new bool[8];
            for (int i = 0; i < 8; i++) {
                word[i] = array[d + i];
            }
            Array.Reverse(word);
            entropyBits = Helper.join(entropyBits, new BitArray(word));
            d += 8;
        }
        return entropyBits;
    }
}
