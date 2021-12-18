using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

namespace LBitcoin.Tests {
    static class OpTest {

        public static void RunAll() {
            //complete tests
            test_op_hash160();
            Debug.WriteLine("OP tests PASSED");
        }

        public static void test_op_hash160() {
            byte[] test = Encoding.UTF8.GetBytes("hello world");
            byte[] h160 = Hash.hash160(test);
            Debug.Assert(Byte.bytesToString(h160) == "d7d5ee7824ff93f94c3055af9382c86c68b5ca92");
        }
    }
}
