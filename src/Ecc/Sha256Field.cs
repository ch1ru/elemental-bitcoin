using System.Numerics;

namespace LBitcoin.Ecc {

    /// <summary>
    /// A finite field of prime order P.
    /// See <see href="https://en.bitcoin.it/wiki/Secp256k1">Secp256k1</see> for more info
    /// </summary>
    class Sha256Field : FieldElement {
        
        public static BigInteger P = FieldElement.Pow(2, 256) - FieldElement.Pow(2, 32) - 977;

        /// <summary>
        /// Constructor. Creates an integer value in the field of <see cref="P"/>.
        /// </summary>
        public Sha256Field(BigInteger num) : base(num, P) {
            num_ = num;
            prime_ = P;
        }

        /// <summary>
        /// Constructor. Creates an integer value in the field of <see cref="P"/>.
        /// </summary>
        public Sha256Field(FieldElement element) : base(element) {
            num_ = element.Num;
            prime_ = P;
        }
    }
}
