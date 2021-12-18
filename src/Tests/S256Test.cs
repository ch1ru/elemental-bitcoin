using System;
using System.Collections.Generic;
using System.Text;
using LBitcoin;
using System.Numerics;
using LBitcoin.Ecc;
using System.Diagnostics;

namespace LBitcoin.Tests {
    class S256Test {

        public static void RunAll() {
            //FIXME some tests fail
            test_order();
            test_testPub();
            test_verify();
            Debug.WriteLine("Sha256 Tests PASSED");
        }

        public static void test_order() {
            Secp256k1 curve = new Secp256k1();
            Point p = curve.getGeneratorPoint() * Secp256k1.N;
            Debug.Assert(p.x == null);
        }

        public static void test_testPub() {
            var points = new (BigInteger secret, BigInteger x, BigInteger y)[4];
            points[0] = (7, BigInteger.Parse("5cbdf0646e5db4eaa398f365f2ea7a0e3d419b7e0330e39ce92bddedcac4f9bc", System.Globalization.NumberStyles.HexNumber), BigInteger.Parse("6aebca40ba255960a3178d6d861a54dba813d0b813fde7b5a5082628087264da", System.Globalization.NumberStyles.HexNumber));
            points[1] = (1485, BigInteger.Parse("c982196a7466fbbbb0e27a940b6af926c1a74d5ad07128c82824a11b5398afda", System.Globalization.NumberStyles.HexNumber), BigInteger.Parse("7a91f9eae64438afb9ce6448a1c133db2d8fb9254e4546b6f001637d50901f55", System.Globalization.NumberStyles.HexNumber));
            points[2] = (BigInteger.Pow(2, 128), BigInteger.Parse("8f68b9d2f63b5f339239c1ad981f162ee88c5678723ea3351b7b444c9ec4c0da", System.Globalization.NumberStyles.HexNumber), BigInteger.Parse("662a9f2dba063986de1d90c2b6be215dbbea2cfe95510bfdf23cbf79501fff82", System.Globalization.NumberStyles.HexNumber));
            points[3] = (BigInteger.Pow(2,240), BigInteger.Parse("9577ff57c8234558f293df502ca4f09cbc65a6572c842b39b366f21717945116", System.Globalization.NumberStyles.HexNumber), BigInteger.Parse("10b49c67fa9365ad7b90dab070be339a1daf9052373ec30ffae4f72d5e66d053", System.Globalization.NumberStyles.HexNumber));
        
        
            foreach((BigInteger secret, BigInteger x, BigInteger y) point in points) {
                Secp256k1 curve = new Secp256k1();
                Point256 p = new Point256(new Sha256Field(point.x), new Sha256Field(point.y));
                Debug.Assert(curve.getGeneratorPoint() * Secp256k1.N == p);
            }
        }

        public static void test_verify() {
            Point256 point = new Point256(
                new Sha256Field(BigInteger.Parse("887387e452b8eacc4acfde10d9aaf7f6d9a0f975aabb10d006e4da568744d06c", System.Globalization.NumberStyles.HexNumber)), 
                new Sha256Field(BigInteger.Parse("61de6d95231cd89026e286df3b6ae4a894a3378e393e93a0f45b666329a0ae34", System.Globalization.NumberStyles.HexNumber)));

            BigInteger z = BigInteger.Parse("ec208baa0fc1c19f708a9ca96fdeff3ac3f230bb4a7ba4aede4942ad003c0f60", System.Globalization.NumberStyles.HexNumber);
            BigInteger r = BigInteger.Parse("ac8d1c87e51d0d441be8b3dd5b05c8795b48875dffe00b7ffcfac23010d3a395", System.Globalization.NumberStyles.HexNumber);
            BigInteger s = BigInteger.Parse("68342ceff8935ededd102dd876ffd6ba72d6a427a3edb13d26eb0781cb423c4", System.Globalization.NumberStyles.HexNumber);
            Debug.Assert(point.verify(z, new Signature(r, s)));

            z = BigInteger.Parse("7c076ff316692a3d7eb3c3bb0f8b1488cf72e1afcd929e29307032997a838a3d", System.Globalization.NumberStyles.HexNumber);
            r = BigInteger.Parse("eff69ef2b1bd93a66ed5219add4fb51e11a840f404876325a1e8ffe0529a2c", System.Globalization.NumberStyles.HexNumber);
            s = BigInteger.Parse("c7207fee197d27c618aea621406f6bf5ef6fca38681d82b2f06fddbdce6feab6", System.Globalization.NumberStyles.HexNumber);
            Debug.Assert(point.verify(z, new Signature(r, s)));
        }
    }
}