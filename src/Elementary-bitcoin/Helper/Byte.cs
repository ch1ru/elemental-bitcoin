using System;
using System.Text;
using System.Numerics;

static class Byte {

    static public string bytesToString(byte[] array) {
        if (array == null) return "";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < array.Length; i++) {
            sb.Append(array[i].ToString("X2"));
        }
        return sb.ToString().ToLower();
    }

    static public byte[] join(byte[] one, byte[] two) {

        if (one == null) {
            return two;
        } else if (two == null) {
            return one;
        }

        byte[] combined = new byte[one.Length + two.Length];

        for (int i = 0; i < combined.Length; ++i) {
            combined[i] = i < one.Length ? one[i] : two[i - one.Length];
        }
        return combined;
    }

    static public byte[] prependByte(byte[] array, byte prefix) {
        byte[] fullBytes = new byte[array.Length + 1];
        fullBytes[0] = prefix;
        array.CopyTo(fullBytes, 1);
        return fullBytes;
    }

    static public byte[] appendByte(byte[] array, byte suffix) {
        byte[] fullBytes = new byte[array.Length + 1];
        array.CopyTo(fullBytes, 0);
        fullBytes[array.Length] = suffix;
        return fullBytes;
    }

    public static byte[] swapBytes(byte[] bytes, int a, int b) {
        byte tmp = bytes[b];
        bytes[b] = bytes[a];
        bytes[a] = tmp;
        return bytes;
    }

    public static byte[] intToBigEndian(Int32 data, int size = 4) {

        if (data == 0) return new byte[] { 0x00, 0x00, 0x00, 0x00 };

        byte[] bytes = BitConverter.GetBytes(data);
        bytes = swapBytes(bytes, 0, 3);
        bytes = swapBytes(bytes, 1, 2);

        while (bytes.Length < size) {
            bytes = prependByte(bytes, 0x00);
        }
        //TODO: add less than 4 size
        return bytes;
    }



    public static byte[] intToBigEndian(Int64 data, int size = 8) {

        if (data == 0) return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] bytes = BitConverter.GetBytes(data);
        bytes = swapBytes(bytes, 0, 7);
        bytes = swapBytes(bytes, 1, 6);
        bytes = swapBytes(bytes, 2, 5);
        bytes = swapBytes(bytes, 3, 4);
        return bytes;
    }

    public static byte[] intToLittleEndian(Int32 data, int size = 4) {
        if (data == 0) return new byte[] { 0x00, 0x00, 0x00, 0x00 };
        byte[] bytes = BitConverter.GetBytes(data);
        while (bytes.Length < size) {
            bytes = appendByte(bytes, 0x00);
        }
        if (size < bytes.Length) {
            byte[] newArray = new byte[size];
            for (int i = 0; i < size; i++) {
                newArray[i] = bytes[i];
            }
            return newArray;
        }
        return bytes;
    }

    public static byte[] intToLittleEndian(Int64 data, int size = 8) {
        byte[] bytes = BitConverter.GetBytes(data);
        return bytes;
    }

    public static byte[] encodeVarInt(int num) { //can encode up to 2^64
        BigInteger num_big = new BigInteger(num);
        bool isSigned = true;
        bool isBigEndian = false;
        byte[] bytes = new byte[] { };
        if (num < 253) {
            bytes = new byte[1];
            bytes[0] = Convert.ToByte(num);
        } else if (num_big < 65536) {
            bytes = num_big.ToByteArray(isSigned, isBigEndian);
            while (bytes.Length < 2) {
                bytes = appendByte(bytes, 0x00);
            }
            bytes = prependByte(bytes, 0xfd);
        } else if (num_big < BigInteger.Parse("100000000", System.Globalization.NumberStyles.HexNumber)) {
            bytes = num_big.ToByteArray(isSigned, isBigEndian);
            while (bytes.Length < 4) {
                bytes = appendByte(bytes, 0x00);
            }
            bytes = prependByte(bytes, 0xfe);
        } else if (num_big < BigInteger.Parse("10000000000000000", System.Globalization.NumberStyles.HexNumber)) {
            bytes = num_big.ToByteArray(isSigned, isBigEndian);
            while (bytes.Length < 8) {
                bytes = appendByte(bytes, 0x00);
            }
            bytes = prependByte(bytes, 0xff);
        } else {
            throw new Exception("Number too large");
        }
        return bytes;
    }
}
