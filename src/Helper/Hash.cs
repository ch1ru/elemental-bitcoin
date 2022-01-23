using System.Security.Cryptography;
using SshNet.Security.Cryptography;
using System.IO;

class Hash {

    static readonly int PBKDF2_ITERATION_COUNT = 2048;

    /*Performs a double hash*/
    public static byte[] hash256(byte[] data) {
        byte[] sha256DigestR1 = sha256(data);
        return sha256(sha256DigestR1);
    }

    public static byte[] sha256(byte[] data) {
        byte[] sha256Digest = new byte[data.Length];
        using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create()) {
            sha256Digest = sha256Hash.ComputeHash(data); //sha256 hash
        }
        return sha256Digest;
    }

    public static byte[] hash160(byte[] data) {
        byte[] sha256Digest = new byte[data.Length];
        using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create()) {
            sha256Digest = sha256Hash.ComputeHash(data); //sha256 hash
        }
        RIPEMD160 myRIPEMD160 = new RIPEMD160();
        byte[] hashValue = myRIPEMD160.ComputeHash(sha256Digest); //ripemd160 hash
        return hashValue;
    }

    public static byte[] ripemd160(byte[] data) {
        RIPEMD160 myRIPEMD160 = new RIPEMD160();
        return myRIPEMD160.ComputeHash(data);
    }

    public static uint murmur3(byte[] data, uint seed) {
        Stream s = new MemoryStream(data);
        return new Murmur3(seed).Hash(s);
    }

    public static byte[] HMACSHA256Encode(byte[] data, byte[] key) {
        System.Security.Cryptography.HMACSHA256 myhmacsha256 = new System.Security.Cryptography.HMACSHA256(key);
        using (MemoryStream stream = new MemoryStream(data)) {
            return myhmacsha256.ComputeHash(stream);
        }
    }

    public static byte[] HMACSHA512Encode(byte[] data, byte[] key) {
        System.Security.Cryptography.HMACSHA512 hmacsha512 = new System.Security.Cryptography.HMACSHA512(key);
        using (MemoryStream stream = new MemoryStream(data)) {
            return hmacsha512.ComputeHash(stream);
        }
    }


    public static byte[] PBKDF2(string mnemonic, byte[] salt) {

        /*salt hash*/
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        provider.GetBytes(salt);

        /*digest*/
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(mnemonic, salt, PBKDF2_ITERATION_COUNT);
        return pbkdf2.GetBytes(512);
    }
}
