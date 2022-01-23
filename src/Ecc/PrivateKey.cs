using System.Numerics;

namespace LBitcoin.Ecc {

    /// <summary>
    /// Elliptic curve private key, uses a 256-bit value as scalar.
    /// see <see href="https://en.bitcoin.it/wiki/Elliptic_Curve_Digital_Signature_Algorithm">here</see> for more info.
    /// </summary>
    public class PrivateKey {

        BigInteger secret_;
        Point256 ecPoint_;
        PublicKey pubKey_;

        /// <summary>
        /// Constructor. Takes a <see cref="BigInteger"/> as scalar, 
        /// sets corresponding pubkey point.
        /// </summary>
        /// <param name="secret">Secret as <see cref="BigInteger"/>.</param>
        public PrivateKey(BigInteger secret) {
            Secp256k1 ec = new Secp256k1();
            secret_ = secret;
            ecPoint_ = ec.GetPublicKeyPoint(secret);
            pubKey_ = new PublicKey(ecPoint_);
        }

        /// <summary>
        /// Constructor. Takes a byte[] as scalar, 
        /// sets corresponding pubkey point.
        /// </summary>
        /// <param name="secret">Secret as byte array.</param>
        public PrivateKey(byte[] secret) {
            Secp256k1 ec = new Secp256k1();
            BigInteger secretInt = new BigInteger(secret, true, true);
            secret_ = secretInt;
            ecPoint_ = ec.GetPublicKeyPoint(secretInt);
            pubKey_ = new PublicKey(ecPoint_);
        }

        /// <summary>
        /// Constructor. Takes a <see cref="PrivateKey"/> as scalar, 
        /// sets corresponding pubkey point.
        /// </summary>
        /// <param name="key">Private key.</param>
        protected PrivateKey(PrivateKey key) {
            secret_ = key.secret_;
            ecPoint_ = key.ecPoint_;
            pubKey_ = key.pubKey_;
        }

        public PublicKey pubKey() {
            return pubKey_;
        }

        public override string ToString() {
            return this.wif();
        }

        public byte[] ToBytes() {
            return secret_.ToByteArray(true, true);
        }

        /// <summary>
        /// Public key point.
        /// </summary>
        public Point256 ecPoint() {
            return ecPoint_;
        }

        /// <summary>
        /// Sign a message with the private key.
        /// </summary>
        /// <param name="message">Message as byte array.</param>
        public Signature sign(byte[] message) {
            return sign(new BigInteger(message, true, true));
        }

        /// <summary>
        /// Sign a message with the private key.
        /// </summary>
        /// <param name="z">Message as <see cref="BigInteger"/>.</param>
        public Signature sign(BigInteger z) {
            Secp256k1 curve = new Secp256k1();
            BigInteger k = deterministic_k(z); //ephemeral key
            Point p = curve.GetGeneratorPoint() * k; //R
            BigInteger r = p.X; //r (x coord)

            /*  identity of s = (z + r.kpr)/k  */
            BigInteger k_inv = BigInteger.ModPow(k, Secp256k1.N - 2, Secp256k1.N);
            BigInteger s = (z + r * secret_) * k_inv % Secp256k1.N;
            if (s > Secp256k1.N / 2) {
                s = Secp256k1.N - s;
            }
            Signature signature = new Signature(r, s);
            return signature;
        }

        /// <summary>
        /// Part of the standard RFC6979 standard for generating unique and random k
        /// see <see href="https://www.rfc-editor.org/info/rfc6979">RFC6979</see> for more info.
        /// </summary>
        private BigInteger deterministic_k(BigInteger z) {
            byte[] k = new byte[32];
            byte[] v = new byte[32];
            for(int i = 0; i < 32; i++) {
                k[i] = 0x00;
                v[i] = 0x01;
            }
            if(z > Secp256k1.N) {
                z -= Secp256k1.N;
            }

            byte[] z_bytes = z.ToByteArray(true, true);
            while (z_bytes.Length < 32) { //add leading zeros
                z_bytes = Byte.prependByte(z_bytes, 0x00);
            }
            byte[] secret_bytes = secret_.ToByteArray(true, true);
            while(secret_bytes.Length < 32) { //add leading zeros
                secret_bytes = Byte.prependByte(secret_bytes, 0x00);
            }
            byte[] secretBytesPadding1 = Byte.prependByte(secret_bytes, 0x00);
            byte[] secretBytesPadding2 = Byte.prependByte(secret_bytes, 0x01);
            k = Hash.HMACSHA256Encode(Byte.join(Byte.join(v, secretBytesPadding1), z_bytes), k);
            v = Hash.HMACSHA256Encode(v, k);
            k = Hash.HMACSHA256Encode(Byte.join(Byte.join(v, secretBytesPadding2), z_bytes), k);
            v = Hash.HMACSHA256Encode(v, k);
            while (true) {
                v = Hash.HMACSHA256Encode(v, k);
                BigInteger candidate = new BigInteger(v, true, true);
                if (candidate >= 1 && candidate < Secp256k1.N) { //between 1, N-1
                    return candidate;
                }
                k = Hash.HMACSHA256Encode(Byte.appendByte(v, 0x00), k);
                v = Hash.HMACSHA256Encode(v, k);
            }
        }

        /// <summary>
        /// Private key in wif format
        /// See <see href="https://en.bitcoin.it/wiki/Wallet_import_format">wallet import format</see> for more info.
        /// </summary>
        public string wif(bool compressed = true, bool testnet = false) {
            bool isSigned = true;
            bool isBigEndian = true;
            byte[] secret_bytes = secret_.ToByteArray(isSigned, isBigEndian);
            byte prefix = 0x80;
            if(testnet) {
                prefix = 0xef;
            }
            if(compressed) {
                secret_bytes = Byte.appendByte(secret_bytes, 0x01);
            }
            secret_bytes = Byte.prependByte(secret_bytes, prefix);
            string b58check = Base58Check.Base58CheckEncoding.Encode(secret_bytes); //encode in base58 check
            return b58check;
        }

        public BigInteger Secret { get { return secret_; } }
    }
}
