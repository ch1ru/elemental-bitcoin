using LBitcoin.Ecc;
using System.Numerics;
using System.Diagnostics;

namespace Elementary_bitcoin.Tests {
    class PrivateKeyTest {

        public static void RunAll() {
            test_sign();
        }

        public static void test_sign() {
            PrivateKey pk = new PrivateKey(csrng.genKey());
            BigInteger z = new BigInteger(csrng.randomEntropy(32));
            Signature sig = pk.sign(z);
            Debug.Assert(pk.ecPoint().verify(z, sig));
        }
    }
}
