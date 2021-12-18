using System;
using System.Numerics;
using System.Text;
using LBitcoin.Ecc;

namespace LBitcoin.Ecc {

    /// <summary>
    /// Public key is the point on an elliptic curve defined as:
    /// base point * scalar (private key) bounded in prime field P of order N.
    /// see <see href="https://en.bitcoin.it/wiki/Elliptic_Curve_Digital_Signature_Algorithm">Secp256k1</see> for more info.
    /// </summary>
    class PublicKey {

        byte[] compressed_;
        byte[] uncompressed_;

        public byte[] Compressed { get { return compressed_; } }

        public byte[] Uncompressed { get { return uncompressed_; } }

        /// <summary>
        /// Constructor. Takes a <see cref="Point"/> on elliptic curve.
        /// </summary>
        public PublicKey(Point p) {
            BigInteger xcoord = p.x;
            BigInteger ycoord = p.y;
            bool isBigEndian = true;
            bool isSigned = true;
            this.compressed_ = xcoord.ToByteArray(isSigned, isBigEndian);
            this.uncompressed_ = Byte.prependByte(Byte.join(xcoord.ToByteArray(isSigned, isBigEndian),
                ycoord.ToByteArray(isSigned, isBigEndian)), 0x04);

            if (ycoord % 2 == 0) {
                byte prefix = 0x02;
                this.compressed_ = Byte.prependByte(this.compressed_, prefix); //if y is even
            }
            else {
                byte prefix = 0x03;
                this.compressed_ = Byte.prependByte(this.compressed_, prefix); //if y is odd
            }
        }

        /// <summary>
        /// Constructor. Takes SEC format.
        /// </summary>
        public PublicKey(byte[] pubKeyBytes) {
            if(pubKeyBytes[0] == 0x02 || pubKeyBytes[0] == 0x03) {
                compressed_ = pubKeyBytes;
            }
            else if(pubKeyBytes[0] == 0x04) {
                uncompressed_ = pubKeyBytes;
            }
        }

        /// <summary>
        /// Used by child class <see cref="HDPublicKey"/>.
        /// </summary>
        protected PublicKey(PublicKey pubKey) {
            compressed_ = pubKey.Compressed;
            uncompressed_ = pubKey.Uncompressed;
        }

        /// <summary>
        /// Converts a public key to address.
        /// </summary>
        /// <param name="addrType">Type of address e.g. "segwit".</param>
        /// <param name="isCompressed">Whether the address is compressed or not.</param>
        public BitcoinAddress getAddr(string addrType, bool isCompressed = true, bool testnet = false) {
            return new BitcoinAddress(this, addrType, isCompressed, testnet);
        }

        /// <summary>
        /// Verifies a message in <see cref="string"/> format.
        /// </summary>
        public bool verify(string message, Signature s) {
            BigInteger messageInt = new BigInteger(Encoding.UTF8.GetBytes(message), true, true);
            return verify(messageInt, s);
        }

        /// <summary>
        /// Verifies a message in <see cref="BigInteger"/> format.
        /// </summary>
        public bool verify(BigInteger z, Signature s) {
            if(compressed_ == null && uncompressed_ == null) {
                throw new Exception("No public key found");
            }
            Point256 pubKeyPoint = compressed_ != null ? Point256.Parse(compressed_) : Point256.Parse(uncompressed_);
            return pubKeyPoint.verify(z, s);
        }

        public static PublicKey Parse(string pubKeyStr) {
            byte[] pubKeyBytes = BigInteger.Parse(pubKeyStr, System.Globalization.NumberStyles.HexNumber).ToByteArray(true, true);
            return new PublicKey(pubKeyBytes);
        }

        public override string ToString() {
            return Byte.bytesToString(this.compressed_);
        }

    }
}
