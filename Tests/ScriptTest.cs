using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;
using System.Diagnostics;

namespace LBitcoin.Tests {
    public static class ScriptTest {

        public static void RunAll() {
            test_parse();
            test_serialise();
            test_address();
            Debug.WriteLine("Script tests PASSED");
        }

        public static void test_parse() {
            byte[] rawScript = BigInteger.Parse("6a47304402207899531a52d59a6de200179928ca900254a36b8dff8bb75f5f5d71b1cdc26125022008b422690b8461cb52c3cc30330b23d574351872b7c361e9aae3649071c1a7160121035d5c93d9ac96881f19ba1f686f15f009ded7c62efe85a872e6a19b43c15a2937", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawScript);
            Script script = Script.Parse(s);
            string want = "304402207899531a52d59a6de200179928ca900254a36b8dff8bb75f5f5d71b1cdc26125022008b422690b8461cb52c3cc30330b23d574351872b7c361e9aae3649071c1a71601";
            Debug.Assert(Byte.bytesToString(script.Commands[0]) == want);
            want = "035d5c93d9ac96881f19ba1f686f15f009ded7c62efe85a872e6a19b43c15a2937";
            Debug.Assert(Byte.bytesToString(script.Commands[1]) == want);
        }

        public static void test_serialise() {
            byte[] want = BigInteger.Parse("6a47304402207899531a52d59a6de200179928ca900254a36b8dff8bb75f5f5d71b1cdc26125022008b422690b8461cb52c3cc30330b23d574351872b7c361e9aae3649071c1a7160121035d5c93d9ac96881f19ba1f686f15f009ded7c62efe85a872e6a19b43c15a2937", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(want);
            Script script = Script.Parse(s);
            Debug.Assert(Byte.bytesToString(script.Serialise()) == Byte.bytesToString(want));
        }

        public static void test_address() {
            BitcoinAddress addr1 = new BitcoinAddress("1BenRpVUFK65JFWcQSuHnJKzc4M8ZP8Eqa");
            byte[] h160 = addr1.GetHash(segwit: false, type: 0);
            Script p2pkh_scriptPubKey = Script.P2PKH(h160);
            Debug.Assert(p2pkh_scriptPubKey.Address().ToString() == addr1.ToString());
            BitcoinAddress addr2 = new BitcoinAddress("mrAjisaT4LXL5MzE81sfcDYKU3wqWSvf9q");
            Debug.Assert(p2pkh_scriptPubKey.Address(testnet: true).ToString() == addr2.ToString());
            BitcoinAddress addr3 = new BitcoinAddress("3CLoMMyuoDQTPRD3XYZtCvgvkadrAdvdXh");
            h160 = addr3.GetHash(segwit: false, type: 1);
            Script p2sh_scriptpubkey = Script.P2SH(h160);
            Debug.Assert(p2sh_scriptpubkey.Address().ToString() == addr3.ToString());
            BitcoinAddress addr4 = new BitcoinAddress("2N3u1R6uwQfuobCqbCgBkpsgBxvr1tZpe7B");
            Debug.Assert(p2sh_scriptpubkey.Address(testnet: true).ToString() == addr4.ToString());
        }
    }
}
