using System;
using System.Numerics;


namespace LBitcoin.Ecc {

    /// <summary>
    /// Generic point class, creates a point on an elliptic curve bounded by a prime field.
    /// </summary>
    public class Point {
        
        protected static BigInteger? pointAtInf = null;
        protected FieldElement x_, y_, a_, b_;

        /// <summary>
        /// Constructor. A point on an elliptic curve of prime order.
        /// </summary>
        /// <param name="x">x coordinate as <see cref="FieldElement"/></param>.
        /// <param name="y">y coordinate as <see cref="FieldElement"/></param>.
        /// <param name="a">Value for A as <see cref="FieldElement"/></param>.
        /// <param name="b">Value for B as <see cref="FieldElement"/></param>.
        public Point(FieldElement x, FieldElement y, FieldElement a, FieldElement b) {
            FieldElement leftSide = y ^ 2;
            FieldElement rightSide = (x ^ 3) + (a * x) + b;

            /*if prime is null*/
            if (x.Prime == 0 || y.Prime == 0 || a.Prime == 0 || b.Prime == 0) {
                throw new Exception("PRIME IS NULL");
            }
            /*if not on the curve*/
            if (leftSide != rightSide) {
                throw new Exception("NOT ON THE CURVE");
            }

            x_ = x;
            y_ = y;
            a_ = a;
            b_ = b;

        }

        /*Called from the inheriter class Point256*/
        protected Point(Point other) {
            this.x_ = other.x_;
            this.y_ = other.y_;
            this.a_ = other.a_;
            this.b_ = other.b_;
        }


        public BigInteger X { get { return x_.Num; } }

        public BigInteger Y { get{ return y_.Num; } }

        static public bool operator ==(Point first, Point second) {
            return (
                first.x_.Num == second.x_.Num &&
                first.y_.Num == second.y_.Num &&
                first.a_.Num == second.a_.Num && 
                first.b_.Num == second.b_.Num);
        }

        public override bool Equals(Object other) {
            return this == (Point)other;
        }

        static public bool operator !=(Point first, Point second) {
            return !(first == second);
        }

        /// <summary>
        /// Performs point addition within a prime field.
        /// </summary>
        /// <param name="first">First point as <see cref="Point"/></param>.
        /// <param name="second">Second point as <see cref="System.Numerics.BigInteger"/></param>.
        /// <returns></returns>
        static public Point operator +(Point first, Point second) {

            BigInteger prime = first.x_.Prime;
            FieldElement x3 = null;
            FieldElement y3 = null;

            if (first.a_ != second.a_ || first.b_ != second.b_) {
                throw new Exception("Points do not use same equation");
            }
            
            if (first.x_.Num == pointAtInf) { //identity of second
                return second;
            }
            else if (second.x_.Num == pointAtInf) { //identity of first
                return first;
            }
            /*xs are not equal*/
            else if (first.x_.Num != second.x_.Num) {
                FieldElement quotient = second.y_ - first.y_;
                FieldElement divisor = second.x_ - first.x_;
                FieldElement m = quotient / divisor;
                x3 = (m ^ 2) - first.x_ - second.x_;
                y3 = (m * (first.x_ - x3)) - first.y_;
            }
            /*if first and second point are equal*/
            else if (first.x_ == second.x_ && first.y_ == second.y_) {
    
                /*use differentiation to get the gradient*/
                FieldElement dividend = ((first.x_ ^ 2) * 3) + first.a_;
                FieldElement divisor = first.y_ * 2;
                FieldElement m = dividend / divisor;
                x3 = (m ^ 2) - (first.x_ * 2);
                y3 = (m * (first.x_ - x3)) - first.y_;
            }
            return new Point(x3, y3, first.a_, first.b_);
        }

        
        /// <summary>
        /// Performs point multiplication within a prime field.
        /// </summary>
        /// <param name="first">Point to mulpiply as <see cref="Point"/>.</param>
        /// <param name="coef">Scalar as <see cref="System.Numerics.BigInteger"/>.</param>
        static public Point operator *(Point first, BigInteger coef) {
            coef = coef % Secp256k1.N; //in cyclic group of order N
            if (coef == pointAtInf) { //if point at infinity
                throw new Exception("Reached point at infinity");
            }
            coef--; //function will return 1 addion more
            Point curr = first;
            Point result = curr;
            /*Now do binary expansion*/
            while (coef > 0) {
                if ((coef & 1) == 1) {
                    result = result + curr;
                }
                curr = curr + curr;
                coef = coef >> 1;
            }

        return result;
        }

        /// <summary>
        /// Prints the coordinates and A and B values.
        /// </summary>
        public void Print() {
            Console.WriteLine("X-coord: " + x_.Num);
            Console.WriteLine("Y-coord: " + y_.Num);
            Console.WriteLine("a: " + a_.Num);
            Console.WriteLine("b: " + b_.Num);
        }
    }
}
