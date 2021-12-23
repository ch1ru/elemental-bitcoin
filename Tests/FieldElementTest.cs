using System.Diagnostics;
using LBitcoin.Ecc;

namespace LBitcoin.Tests {
    static class FieldElementTest {

        public static void RunAll() {
            test_ne();
            test_add();
            test_div();
            test_mul();
            test_sub();
            test_pow();
            Debug.WriteLine("FieldElement tests PASSED");
        }

        public static void test_ne() {
            FieldElement a = new FieldElement(2, 31);
            FieldElement b = new FieldElement(2, 31);
            FieldElement c = new FieldElement(15, 31);
            Debug.Assert(a == b);
            Debug.Assert(a != c);
        }

        public static void test_add() {
            FieldElement a = new FieldElement (2, 31);
            FieldElement b = new FieldElement(15, 31);
            Debug.Assert(a + b == new FieldElement(17, 31));
            a = new FieldElement(17, 31);
            b = new FieldElement(21, 31);
            Debug.Assert(a + b == new FieldElement (7, 31));
        }

        public static void test_sub() {
            FieldElement a = new FieldElement (29, 31);
            FieldElement b = new FieldElement (4, 31);
            Debug.Assert(a - b == new FieldElement(25, 31));
            a = new FieldElement(15, 31);
            b = new FieldElement(30, 31);
            Debug.Assert(a - b == new FieldElement(16, 31));
        }

        public static void test_mul() {
            FieldElement a = new FieldElement(24, 31);
            FieldElement b = new FieldElement (19, 31);
            Debug.Assert(a * b == new FieldElement(22, 31));
        }

        public static void test_pow() {
            FieldElement a = new FieldElement(17, 31);
            Debug.Assert((a ^ 3) == new FieldElement(15, 31));
            a = new FieldElement(5, 31);
            FieldElement b = new FieldElement (18, 31);
            Debug.Assert((a ^ 5) * b == new FieldElement(16, 31));
        }

        public static void test_div() {
            FieldElement a = new FieldElement (3, 31);
            FieldElement b = new FieldElement (24, 31);
            Debug.Assert((a / b) == new FieldElement(4, 31));
            a = new FieldElement (17, 31);
            //Debug.Assert((a ^ -3) == new FieldElement (29, 31)); //FIXME: cannot perform on negative exponents
            a = new FieldElement (4, 31);
            b = new FieldElement (11, 31);
            //Debug.Assert((a ^ -4) * b == new FieldElement (13, 31));
        }
    }
}
