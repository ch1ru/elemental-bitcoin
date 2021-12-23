using System;
using System.Numerics;

namespace LBitcoin.Ecc {

    /// <summary>
    /// Elliptic curve signature.
    /// See <see href="https://cryptobook.nakov.com/digital-signatures/ecdsa-sign-verify-messages">here</see> for more info.
    /// </summary>
    class Signature {

        BigInteger r_, s_;

        /// <summary>
        /// Constructor. Takes an r and s value.
        /// </summary>
        /// <param name="r">R value.</param>
        /// <param name="s">S value.</param>
        public Signature(BigInteger r, BigInteger s) {
            this.r_ = r;
            this.s_ = s;
        }

        public override string ToString() {
            string r = Byte.bytesToString(r_.ToByteArray(false, true));
            string s = Byte.bytesToString(s_.ToByteArray(false, true));
            return "Signature (" + r + ", " + s + ")";
        }

        /// <summary>
        /// Encodes signature in bytes.
        /// </summary>
        public byte[] DerEncode() {
            bool isBigEndian = true;
            bool isSigned = true;
            byte[] r_bytes = r_.ToByteArray(isSigned, isBigEndian);
            byte[] s_bytes = s_.ToByteArray(isSigned, isBigEndian);
            if (Convert.ToInt32(r_bytes[0]) >= Convert.ToInt32(0x80)) {
                r_bytes = Byte.prependByte(r_bytes, 0x00);
            }
            if (Convert.ToInt32(s_bytes[0]) >= Convert.ToInt32(0x80)) {
                s_bytes = Byte.prependByte(s_bytes, 0x00);
            }

            byte r_length = Convert.ToByte(r_bytes.Length);
            byte s_length = Convert.ToByte(s_bytes.Length);
            byte[] der = new byte[4];
            der[0] = 0x30; //start byte
            int sigLength = Convert.ToInt32(r_length) + Convert.ToInt32(s_length) + 4; //length of sig from this byte
            der[1] = Convert.ToByte(sigLength);
            der[2] = 0x02; //marker byte
            der[3] = r_length; //length of r
            der = Byte.join(der, r_bytes);
            der = Byte.appendByte(der, 0x02); //marker byte
            der = Byte.appendByte(der, s_length);
            der = Byte.join(der, s_bytes);
            return der;
        }

        /// <summary>
        /// Creates a signature by parsing a byte array.
        /// </summary>
        /// <param name="sigBytes">The DER encoded signature.</param>
        public static Signature Parse(byte[] sigBytes) {
            int counter = 2;
            if(sigBytes[counter] != 0x02) { //r marker
                throw new Exception("Cannot find r");
            }
            counter = 3;
            int r_length = sigBytes[counter];
            if(sigBytes[4] == 0x00) {
                counter++;
                r_length--;
            }
            byte[] rBytes = new byte[r_length];
            counter++;
            for (int i = counter; i < counter + r_length; i++) {
                rBytes[i - counter] = sigBytes[i];
            }
            counter = r_length + counter;
            
            if (sigBytes[counter] != 0x02) {
                throw new Exception("Cannot find s");
            }
            counter++;
            int s_length = sigBytes[counter];
            if (sigBytes[counter+1] == 0x00) {
                counter++;
                r_length--;
            }
            byte[] sBytes = new byte[s_length];
            counter++;
            for (int i = counter; i < counter + s_length; i++) {
                sBytes[i - counter] = sigBytes[i];
            }
            BigInteger r = new BigInteger(rBytes, true, true);
            BigInteger s = new BigInteger(sBytes, true, true);
            Signature sig = new Signature(r, s);
            return sig;
        }


        public BigInteger r { get { return r_; } }

        public BigInteger s { get { return s_; } }
    }
}
