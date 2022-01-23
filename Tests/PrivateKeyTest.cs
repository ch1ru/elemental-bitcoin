using LBitcoin.Ecc;
using System.Numerics;
using System.Diagnostics;

namespace Elementary_bitcoin.Tests {
    public class PrivateKeyTest {

        public static void RunAll() {
            test_sign();
        }

        public static void test_sign() {
            PrivateKey pk = new PrivateKey(Csrng.GenKey());
            BigInteger z = new BigInteger(Csrng.RandomEntropy(32));
            Signature sig = pk.sign(z);
            Debug.Assert(pk.ecPoint().Verify(z, sig));
        }
    }
}
