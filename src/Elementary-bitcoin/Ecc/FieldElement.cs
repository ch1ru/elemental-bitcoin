using System;
using System.Numerics;


namespace LBitcoin.Ecc {
    class FieldElement {

        protected BigInteger num_ = BigInteger.Zero;
        protected BigInteger prime_ = 0;


        protected FieldElement(FieldElement other) {
            this.num_ = other.num_;
            this.prime_ = other.prime_;
        }


        public FieldElement(BigInteger num, BigInteger prime) {
            this.num_ = num;
            this.prime_ = prime;
        }


        public BigInteger Num { get { return num_; } }

        public BigInteger Prime { get { return prime_; } }

        public static bool operator ==(FieldElement first, FieldElement second) {
                return (first.num_ == second.num_ && first.prime_ == second.prime_);
        }

        public static bool operator !=(FieldElement first, FieldElement second) {
            return !(first == second);
        }

        public override bool Equals(Object other) {
            return this == (FieldElement)other;
        }

        public static FieldElement operator+(FieldElement first, FieldElement second) {
            if (first.prime_ != second.prime_) {
                throw new Exception("Prime numbers are not equal");
            }

            BigInteger num = mod((first.num_ + second.num_), first.prime_);
            FieldElement third = new FieldElement(num, first.prime_);
            return third;
        }

        public static FieldElement operator -(FieldElement first, FieldElement second) {
            if (first.prime_ != second.prime_) {
                throw new Exception("Prime numbers are not equal");
            }

            BigInteger num = mod((first.num_ - second.num_), first.prime_);
            FieldElement third = new FieldElement(num, first.prime_);
            return third;
        }

        public static FieldElement operator *(FieldElement first, FieldElement second) {
            if (first.prime_ != second.prime_) {
                throw new Exception("Prime numbers are not equal");
            }

            BigInteger num = mod((first.num_ * second.num_), first.prime_);
            FieldElement third = new FieldElement(num, first.prime_);
            return third;
        }

        public static FieldElement operator ^(FieldElement baseNum, BigInteger exponent) {
            BigInteger prime = baseNum.Prime;
            BigInteger n = exponent % (prime - 1);
            BigInteger num = BigInteger.ModPow(baseNum.Num, n, prime);
            FieldElement result = new FieldElement(num, prime);
            return result;
        }

        public static FieldElement operator /(FieldElement dividend, FieldElement divisor) {
            if (dividend.prime_ != divisor.prime_) {
                throw new Exception("Prime numbers are not equal");
            }

            BigInteger exponent = dividend.prime_ - 2;
            return dividend * (divisor ^ exponent);
        }

        public static FieldElement operator *(FieldElement first, BigInteger coefficient) {
            BigInteger num = first.num_ * coefficient;
            BigInteger num_mod = mod(num, first.prime_);
            FieldElement third = new FieldElement(num_mod, first.prime_);
            return third;
        }

        /// <summary>
        /// Exponentiates a value <paramref name="x"/> to the power <paramref name="n"/>.
        /// </summary>
        /// <param name="x">A <see cref="System.Numerics.BigInteger"/> base num.</param>
        /// <param name="n">The <see cref="System.Numerics.BigInteger"/> exponent.</param>
        /// <returns>Exponent as BigInteger</returns>
        public static BigInteger Pow(BigInteger x, BigInteger n) { 
          BigInteger result = 1;

          while (n > 0){
            if ((n & 1) == 1){
              result *= x;
            }
            n >>=1 ;
            x *= x;
          }

          return result;
        }
        

        /// <summary>
        /// Performs modulo operation of <paramref name="n"/> on <paramref name="a"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns>Result of modulo operation as <see cref="System.Numerics.BigInteger"/></returns>
        public static BigInteger mod(BigInteger a, BigInteger n) {
            BigInteger result = a % n;
            if ((result < 0 && n > 0) || (result > 0 && n < 0)) {
                result += n;
            }
            return result;
        }

        /// <summary>
        /// Performs square root
        /// </summary>
        /// <returns><see cref="FieldElement"/> square root</returns>
        public FieldElement Sqrt() {
            return this ^ ((prime_ + 1) / 4);
        }
    }
}
