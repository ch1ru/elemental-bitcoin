using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.IO;

namespace LBitcoin.Tests {
    static class MerkleBlockTest {

        public static void RunAll() {

            test_parse();
            Debug.WriteLine("Merkle block tests PASSED");
        }

        public static void test_parse() {
            byte[] bytes = Byte.join(new byte[] { 0x00, 0x00, 0x00 }, BigInteger.Parse("20df3b053dc46f162a9b00c7f0d5124e2676d47bbe7c5d0793a500000000000000ef445fef2ed495c275892206ca533e7411907971013ab83e3b47bd0d692d14d4dc7c835b67d8001ac157e670bf0d00000aba412a0d1480e370173072c9562becffe87aa661c1e4a6dbc305d38ec5dc088a7cf92e6458aca7b32edae818f9c2c98c37e06bf72ae0ce80649a38655ee1e27d34d9421d940b16732f24b94023e9d572a7f9ab8023434a4feb532d2adfc8c2c2158785d1bd04eb99df2e86c54bc13e139862897217400def5d72c280222c4cbaee7261831e1550dbb8fa82853e9fe506fc5fda3f7b919d8fe74b6282f92763cef8e625f977af7c8619c32a369b832bc2d051ecd9c73c51e76370ceabd4f25097c256597fa898d404ed53425de608ac6bfe426f6e2bb457f1c554866eb69dcb8d6bf6f880e9a59b3cd053e6c7060eeacaacf4dac6697dac20e4bd3f38a2ea2543d1ab7953e3430790a9f81e1c67f5b58c825acf46bd02848384eebe9af917274cdfbb1a28a5d58a23a17977def0de10d644258d9c54f886d47d293a411cb6226103b55635", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true));
            Stream s = new MemoryStream(bytes);
            MerkleBlock mb = MerkleBlock.Parse(s);
            uint version = 0x00000020;
            Debug.Assert(mb.Version == version);
            string merkleRoot = "ef445fef2ed495c275892206ca533e7411907971013ab83e3b47bd0d692d14d4";
            Debug.Assert(merkleRoot == Byte.bytesToString(mb.MerkleRoot));
            string prevBlock = "df3b053dc46f162a9b00c7f0d5124e2676d47bbe7c5d0793a500000000000000";
            Debug.Assert(prevBlock == Byte.bytesToString(mb.PrevBlock));
            int timestamp = BitConverter.ToInt32(BigInteger.Parse("dc7c835b", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true));
            Debug.Assert(timestamp == mb.Timestamp);
            string bits = "67d8001a";
            Debug.Assert(bits == Byte.bytesToString(mb.Bits));
            string nonce = "c157e670";
            Debug.Assert(Byte.bytesToString(mb.Nonce) == nonce);
            BigInteger total = BigInteger.Parse("00000dbf", System.Globalization.NumberStyles.HexNumber);
            Debug.Assert((int)total == mb.TotalTxs);

            byte[][] hashes = {
             BigInteger.Parse("ba412a0d1480e370173072c9562becffe87aa661c1e4a6dbc305d38ec5dc088a", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("7cf92e6458aca7b32edae818f9c2c98c37e06bf72ae0ce80649a38655ee1e27d", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("34d9421d940b16732f24b94023e9d572a7f9ab8023434a4feb532d2adfc8c2c2", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("158785d1bd04eb99df2e86c54bc13e139862897217400def5d72c280222c4cba", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("ee7261831e1550dbb8fa82853e9fe506fc5fda3f7b919d8fe74b6282f92763ce", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("f8e625f977af7c8619c32a369b832bc2d051ecd9c73c51e76370ceabd4f25097", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("c256597fa898d404ed53425de608ac6bfe426f6e2bb457f1c554866eb69dcb8d", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("6bf6f880e9a59b3cd053e6c7060eeacaacf4dac6697dac20e4bd3f38a2ea2543", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("d1ab7953e3430790a9f81e1c67f5b58c825acf46bd02848384eebe9af917274c", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("dfbb1a28a5d58a23a17977def0de10d644258d9c54f886d47d293a411cb62261", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true)
            };
            List<byte[]> testHashes = mb.TxHashes;
            for(int i = 0; i < hashes.Length; i++) {
                Debug.Assert(Byte.bytesToString(testHashes[i]) == Byte.bytesToString(hashes[i]));
            }
            string flags = "b55635";
            Debug.Assert(flags == Byte.bytesToString(mb.Flags));

            /*Test if it is valid*/
            if(!mb.isValid()) {
                throw new Exception("block not valid");
            }
        }
    }

    static class MerkleTreeTest {

        public static void RunAll() {
            test_init();
            test_populateTree1();
            test_populateTree2();
        }

        static void test_init() {
            MerkleTree tree = new MerkleTree(9);
            Debug.Assert(tree.Nodes[0].Length == 1);
            Debug.Assert(tree.Nodes[1].Length == 2);
            Debug.Assert(tree.Nodes[2].Length == 3);
            Debug.Assert(tree.Nodes[3].Length == 5);
            Debug.Assert(tree.Nodes[4].Length == 9);
        }

        static void test_populateTree1() {
            byte[][] hashes = new byte[][] {
            BigInteger.Parse("9745f7173ef14ee4155722d1cbf13304339fd00d900b759c6f9d58579b5765fb", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("5573c8ede34936c29cdfdfe743f7f5fdfbd4f54ba0705259e62f39917065cb9b", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("82a02ecbb6623b4274dfcab82b336dc017a27136e08521091e443e62582e8f05", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("507ccae5ed9b340363a0e6d765af148be9cb1c8766ccc922f83e4ae681658308", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("a7a4aec28e7162e1e9ef33dfa30f0bc0526e6cf4b11a576f6c5de58593898330", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("bb6267664bd833fd9fc82582853ab144fece26b7a8a5bf328f8a059445b59add", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("ea6d7ac1ee77fbacee58fc717b990c4fcccf1b19af43103c090f601677fd8836", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("457743861de496c429912558a106b810b0507975a49773228aa788df40730d41", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("7688029288efc9e9a0011c960a6ed9e5466581abf3e3a6c26ee317461add619a", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("b1ae7f15836cb2286cdd4e2c37bf9bb7da0a2846d06867a429f654b2e7f383c9", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("9b74f89fa3f93e71ff2c241f32945d877281a6a50a6bf94adac002980aafe5ab", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("b3a92b5b255019bdaf754875633c2de9fec2ab03e6b8ce669d07cb5b18804638", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("b5c0b915312b9bdaedd2b86aa2d0f8feffc73a2d37668fd9010179261e25e263", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("c9d52c5cb1e557b92c84c52e7c4bfbce859408bedffc8a5560fd6e35e10b8800", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("c555bc5fc3bc096df0a0c9532f07640bfb76bfe4fc1ace214b8b228a1297a4c2", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("f9dbfafc3af3400954975da24eb325e326960a25b87fffe23eef3e7ed2fb610e", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true)
            };

            MerkleTree tree = new MerkleTree(hashes.Length);
            List<byte[]> hashesList = new List<byte[]>();
            foreach(byte[] hash in hashes) {
                hashesList.Add(hash);
            }
            bool[] flags = new bool[31];
            for(int i = 0; i < flags.Length; i++) {
                flags[i] = true;
            }
            BitArray bitArray = new BitArray(flags);
            tree.populateTree(bitArray, hashesList);
            string root = "597c4bafe3832b17cbbabe56f878f4fc2ad0f6a402cee7fa851a9cb205f87ed1";
            Debug.Assert(root == Byte.bytesToString(tree.root()));
        }

        static void test_populateTree2() {
            byte[][] hashes = new byte[][] {
            BigInteger.Parse("42f6f52f17620653dcc909e58bb352e0bd4bd1381e2955d19c00959a22122b2e", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("94c3af34b9667bf787e1c6a0a009201589755d01d02fe2877cc69b929d2418d4", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("959428d7c48113cb9149d0566bde3d46e98cf028053c522b8fa8f735241aa953", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("a9f27b99d5d108dede755710d4a1ffa2c74af70b4ca71726fa57d68454e609a2", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            BigInteger.Parse("62af110031e29de1efcad103b3ad4bec7bdcf6cb9c9f4afdd586981795516577", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true),
            };

            List<byte[]> hashesList = new List<byte[]>();
            for(int i = 0; i < hashes.Length; i++) {
                hashesList.Add(hashes[i]);
            }
            MerkleTree tree = new MerkleTree(hashes.Length);
            bool[] flags = new bool[11];
            for (int i = 0; i < flags.Length; i++) {
                flags[i] = true;
            }
            BitArray bitArray = new BitArray(flags);
            tree.populateTree(bitArray, hashesList);
            string root = "a8e8bd023169b81bc56854137a135b97ef47a6a7237f4c6e037baed16285a5ab";
            Debug.Assert(root == Byte.bytesToString(tree.root()));
        }
    }
}
