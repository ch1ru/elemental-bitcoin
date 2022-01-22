using System.Text;
using System.Numerics;

namespace LBitcoin.Ecc {

    /// <summary>
    /// Creates a point on an elliptic curve of order <see cref="Secp256k1.N"/>
    /// </summary>
    class Point256 : Point {

        /// <summary>
        /// Constructor. Creates a point on an elliptic curve of order <see cref="Secp256k1.N"/> and
        /// prime <see cref="Sha256Field.P"/>.
        /// </summary>
        /// <param name="x">x coordinate as <see cref="Sha256Field"/></param>
        /// <param name="y">y coordinate as <see cref="Sha256Field"/></param>
        public Point256(Sha256Field x, Sha256Field y) 
            : base(x, y, new Sha256Field(0), new Sha256Field(7)) {

            x_ = new Sha256Field(x);
            y_ = new Sha256Field(y);
            a_ = new Sha256Field(0);
            b_ = new Sha256Field(7);
        }

        public Point256(Point p) : base(p) {
            x_ = new Sha256Field(p.x);
            y_ = new Sha256Field(p.y);
            a_ = new Sha256Field(0);
            b_ = new Sha256Field(7);
        }

        /// <summary>
        /// Parses an SEC formated public key and converts it into <see cref="Point256"/> object.
        /// </summary>
        /// <param name="sec">Public key in bytes</param>
        /// <returns><see cref="Point256"/></returns>
        static public Point256 Parse(byte[] sec) {
            byte[] x_bytes = new byte[32];
            for (int i = 1; i < 33; i++) {
                x_bytes[i - 1] = sec[i];
            }
            BigInteger x = new BigInteger(x_bytes, true, true);
            Sha256Field x_sha256 = new Sha256Field(x);
            Sha256Field a = new Sha256Field(0);
            Sha256Field b = new Sha256Field(7);
            if (sec[0] == 0x04) { //uncompressed
                byte[] y_bytes = new byte[32];
                for (int i = 33; i < 65; i++) {
                    y_bytes[i - 33] = sec[i];
                }
                BigInteger y = new BigInteger(y_bytes, true, true);
                Sha256Field y_sha256 = new Sha256Field(y);
                Point256 finalPoint = new Point256(x_sha256, y_sha256);
                return finalPoint;
            } else { //compressed
                bool isEven = (sec[0] == 0x02);
                FieldElement alpha = (x_sha256 ^ 3) + b; //right side of the equation y^2 = x^3 + 7
                FieldElement beta = alpha.Sqrt(); //sqrt +/- x^3 + 7
                Sha256Field evenBeta, oddBeta;

                if (beta.Num.IsEven) { //even y
                    evenBeta = new Sha256Field(beta);
                    oddBeta = new Sha256Field(Sha256Field.P - beta.Num);
                } else { //odd (p-y)
                    evenBeta = new Sha256Field(Sha256Field.P - beta.Num);
                    oddBeta = new Sha256Field(beta);
                }

                if (isEven) {
                    Point256 finalPoint = new Point256(x_sha256, evenBeta);
                    return finalPoint;
                } else {
                    Point256 finalPoint = new Point256(x_sha256, oddBeta);
                    return finalPoint;
                }
            }
        }


        /// <summary>
        /// Verifies a message string from signature
        /// </summary>
        /// <param name="message">The string message</param>
        /// <param name="sig">The <see cref="Signature"/> to verify</param>
        /// <returns><see cref="bool"/></returns>
        public bool verify(string message, Signature sig) {
            BigInteger mesageInt = new BigInteger(Encoding.UTF8.GetBytes(message), true, true);
            return verify(mesageInt, sig);
        }

        /// <summary>
        /// Verifies a numeric message from signature
        /// </summary>
        /// <param name="z">Message as <see cref="System.Numerics.BigInteger"/></param>
        /// <param name="sig">The <see cref="Signature"/> to verify</param>
        /// <returns><see cref="bool"/></returns>
        public bool verify(BigInteger z, Signature sig) {
            Secp256k1 curve = new Secp256k1();
            Point256 self = new Point256(new Sha256Field(x_), new Sha256Field(y_));
            BigInteger s_inv = BigInteger.ModPow(sig.s, Secp256k1.N - 2, Secp256k1.N);
            BigInteger u = z * s_inv % Secp256k1.N;
            BigInteger v = sig.r * s_inv % Secp256k1.N;
            Point total = (curve.getGeneratorPoint() * u) + (self * v);
            return total.x == sig.r;
        }
    }
}
