using System.Text;
using System.Diagnostics;

namespace LBitcoin.Tests {
    public static class BloomTest {

        public static void RunAll() {
            test_add();
            Debug.WriteLine("Bloom filter tests PASSED");
        }

        public static void test_add() {
            BloomFilter bf = new BloomFilter(10, 5, 99);
            byte[] item = Encoding.ASCII.GetBytes("Hello World");
            bf.Add(item);
            string expected = "0000000a080000000140";
            Debug.Assert(Byte.bytesToString(bf.FilterBytes()) == expected);
            byte[] item2 = Encoding.ASCII.GetBytes("Goodbye!");
            bf.Add(item2);
            string expected2 = "4000600a080000010940";
            Debug.Assert(Byte.bytesToString(bf.FilterBytes()) == expected2);
        }
    }
}
