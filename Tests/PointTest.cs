using System;
using System.Diagnostics;
using System.Numerics;
using LBitcoin.Ecc;

namespace LBitcoin.Tests {
    static class PointTest {

        public static void RunAll() {
            test_onCurve();
            test_add();
            test_rmul();
            Debug.WriteLine("Point tests PASSED");
        }


        public static void test_onCurve() {
            BigInteger prime = 223;
            FieldElement a = new FieldElement(0, prime);
            FieldElement b = new FieldElement(7, prime);
            int[] validXCoords = { 192, 105, 17, 56, 1, 193 };
            int[] invalidXCoords = { 200, 42, 119, 99 };

            for(int i = 0; i < 4; i+=2) { //valid points
                FieldElement x = new FieldElement(validXCoords[i], prime);
                FieldElement y = new FieldElement(validXCoords[i+1], prime);
                Point p = new Point(x, y, a, b); //this should not result in an error
            }

            for (int i = 0; i < 2; i+=2) { //invalid points
                FieldElement x = new FieldElement(invalidXCoords[i], prime);
                FieldElement y = new FieldElement(invalidXCoords[i+1], prime);
                try {
                    Point p = new Point(x, y, a, b); //this should result in an error
                }
                catch(Exception e) {
                    continue;
                }
                Debug.Fail("invalid point added");
            }

        }

        public static void test_add() {
            BigInteger prime = 223;
            FieldElement a = new FieldElement(0, prime);
            FieldElement b = new FieldElement(7, prime);

            int[][] sets = {
                new int[] { 192, 105, 17, 56, 170, 142 },
                new int[] { 47, 71, 117, 141, 60, 139 },
                new int[] { 143, 98, 76, 66, 47, 71 }
            };

            for(int i = 0; i < sets.Length; i++) {
                Point p1 = new Point(new FieldElement(sets[i][0], prime), new FieldElement(sets[i][1], prime), a, b);
                Point p2 = new Point(new FieldElement(sets[i][2], prime), new FieldElement(sets[i][3], prime), a, b);
                Point p3 = new Point(new FieldElement(sets[i][4], prime), new FieldElement(sets[i][5], prime), a, b);
                Debug.Assert(p1 + p2 == p3);
            }
        }

        public static void test_rmul() {
            BigInteger prime = 223;
            FieldElement a = new FieldElement(0, prime);
            FieldElement b = new FieldElement(7, prime);

            int[][] sets = {
                new int[] { 2, 192, 105, 49, 71},
                new int[] { 2, 143, 98, 64, 168 },
                new int[] { 2, 47, 71, 36, 111 },
                new int[] { 4, 47, 71, 194, 51 },
                new int[] { 8, 47, 71, 116, 55 }
                //add new int[] { 21, 47, 71, null, None }
            };

            for(int i = 0; i < sets.Length; i++) {
                Point p1 = new Point(new FieldElement(sets[i][1], prime), new FieldElement(sets[i][2], prime), a, b);
                Point p2 = new Point(new FieldElement(sets[i][3], prime), new FieldElement(sets[i][4], prime), a, b);
                BigInteger s = sets[i][0];
                Debug.Assert((p1 * s) == p2);
            }
        }

    }
}
