using System.Numerics;

namespace LBitcoin.Ecc {
    class Sha256Field : FieldElement {
        
        public static BigInteger P = FieldElement.Pow(2, 256) - FieldElement.Pow(2, 32) - 977;

        /// <summary>
        /// Creates an integer value in the field of <see cref="P"/>
        /// </summary>
        /// <param name="num"></param>
        public Sha256Field(BigInteger num) : base(num, P) {
            num_ = num;
            prime_ = P;
        }

        /// <summary>
        /// Creates an integer value in the field of <see cref="P"/>
        /// </summary>
        /// <param name="element"></param>
        public Sha256Field(FieldElement element) : base(element) {
            num_ = element.Num;
            prime_ = P;
        }
    }
}
