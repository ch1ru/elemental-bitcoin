using System;
using System.Numerics;
using System.IO;
using System.Diagnostics;

namespace LBitcoin.Tests {
    static class BlockTest {

        //unit test for blocks

        public static void RunAll() {
            test_Parse();
            test_serialise();
            test_hash();
            test_difficulty();
            test_bip9();
            test_bip141();
            test_bip91();
            Debug.WriteLine("Block tests PASSED");
        }

        static public void test_Parse() {
            BigInteger blockint = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes = blockint.ToByteArray(true, true);
            Stream s = new MemoryStream(blockBytes);
            Block block = Block.Parse(s);
            Debug.Assert(block.Version == 0x20000002);
            byte[] prevBlockSmallEnd = block.PrevBlock;
            Array.Reverse(prevBlockSmallEnd);
            Debug.Assert(Byte.bytesToString(prevBlockSmallEnd) == "000000000000000000fd0c220a0a8c3bc5a7b487e8c8de0dfa2373b12894c38e");
            byte[] merkleRootSmallEnd = block.MerkleRoot;
            Array.Reverse(merkleRootSmallEnd);
            Debug.Assert(Byte.bytesToString(merkleRootSmallEnd) == "be258bfd38db61f957315c3f9e9c5e15216857398d50402d5089a8e0fc50075b");
            Debug.Assert(block.Timestamp == 0x59a7771e);
            byte[] bitsSmallEnd = block.Bits;
            //TODO: find out why bits is not in little endian
            Debug.Assert(Byte.bytesToString(bitsSmallEnd) == "e93c0118");
            byte[] nonceSmallEnd = block.Nonce;
            Debug.Assert(Byte.bytesToString(nonceSmallEnd) == "a4ffd71d");
        }

        public static void test_serialise() {
            BigInteger blockint = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes = blockint.ToByteArray(true, true);
            Stream s = new MemoryStream(blockBytes);
            Block block = Block.Parse(s);
            Debug.Assert(Byte.bytesToString(block.Serialise()) == Byte.bytesToString(blockBytes));
        }

        public static void test_hash() {
            BigInteger blockint = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes = blockint.ToByteArray(true, true);
            Stream s = new MemoryStream(blockBytes);
            Block block = Block.Parse(s);
            Debug.Assert(Byte.bytesToString(block.hash()) == "2375044d646ad73594dd0b37b113becdb03964584c9e7e000000000000000000");
        }

        public static void test_bip9() {
            BigInteger blockint = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes = blockint.ToByteArray(true, true);
            Stream s = new MemoryStream(blockBytes);
            Block block = Block.Parse(s);
            Debug.Assert(block.bip9() == true);

            BigInteger blockint2 = BigInteger.Parse("0400000039fa821848781f027a2e6dfabbf6bda920d9ae61b63400030000000000000000ecae536a304042e3154be0e3e9a8220e5568c3433a9ab49ac4cbb74f8df8e8b0cc2acf569fb9061806652c27", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes2 = blockint2.ToByteArray(true, true);
            Stream s2 = new MemoryStream(blockBytes2);
            Block block2 = Block.Parse(s2);
            Debug.Assert(block2.bip9() == false);
        }

        public static void test_bip91() {
            BigInteger blockint = BigInteger.Parse("1200002028856ec5bca29cf76980d368b0a163a0bb81fc192951270100000000000000003288f32a2831833c31a25401c52093eb545d28157e200a64b21b3ae8f21c507401877b5935470118144dbfd1", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes = blockint.ToByteArray(true, true);
            Stream s = new MemoryStream(blockBytes);
            Block block = Block.Parse(s);
            Debug.Assert(block.bip91() == true);

            BigInteger blockint2 = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes2 = blockint2.ToByteArray(true, true);
            Stream s2 = new MemoryStream(blockBytes2);
            Block block2 = Block.Parse(s2);
            Debug.Assert(block2.bip91() == false);
        }

        public static void test_bip141() {
            BigInteger blockint = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes = blockint.ToByteArray(true, true);
            Stream s = new MemoryStream(blockBytes);
            Block block = Block.Parse(s);
            Debug.Assert(block.bip141() == true);

            BigInteger blockint2 = BigInteger.Parse("0000002066f09203c1cf5ef1531f24ed21b1915ae9abeb691f0d2e0100000000000000003de0976428ce56125351bae62c5b8b8c79d8297c702ea05d60feabb4ed188b59c36fa759e93c0118b74b2618", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes2 = blockint2.ToByteArray(true, true);
            Stream s2 = new MemoryStream(blockBytes2);
            Block block2 = Block.Parse(s2);
            Debug.Assert(block2.bip141() == false);
        }

        public static void test_difficulty() {
            BigInteger blockint = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
            byte[] blockBytes = blockint.ToByteArray(true, true);
            Stream s = new MemoryStream(blockBytes);
            Block block = Block.Parse(s);
            Debug.Assert(block.difficulty() == 888171856257);
        }
    }
}
