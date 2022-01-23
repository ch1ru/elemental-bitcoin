using System;
using System.Numerics;
using System.Security.Cryptography;
using LBitcoin;
using LBitcoin.Ecc;


class Csrng {

    static private RNGCryptoServiceProvider Rand =
    new RNGCryptoServiceProvider();


    // Return a random integer between a min and max value.
    static public byte[] RandomEntropy(int size) {

        // Get 32 random bytes.
        byte[] randomBytes = new byte[size];
        Rand.GetBytes(randomBytes);
        return randomBytes;
    }

    static public BigInteger GenKey() {
        byte[] entropy = RandomEntropy(32);
        byte[] sha256Digest = Hash.sha256(entropy);
        BigInteger k = new BigInteger(sha256Digest, true, true);

        if (k > Secp256k1.N) {
            Console.WriteLine("Random number generated {0}", k);
            throw new Exception("K cannot be larger than N and must be in range");
        }
        //Rand.Dispose(); //release unmanaged resource
        return k;
    }
}





