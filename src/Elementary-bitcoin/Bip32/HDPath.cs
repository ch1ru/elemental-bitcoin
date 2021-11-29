using System;
using System.Linq;
using System.Numerics;

namespace LBitcoin {

    struct HDNode {

        public int index_;
        public uint child_;
        public bool isHardened_;

        public HDNode(int index, bool isHardened = false) {

            index_ = index;
            isHardened_ = isHardened;

            /*hardened children are the last 2^31 bits*/
            child_ = isHardened ? (uint) index + (uint) BigInteger.Pow(2, 31) : (uint) index;

        }
    }

    class HDPath {

        public HDNode[] hierarchies_;
        string derivationScheme_;

       public HDPath(string path) {
            //example m/44'/0/1 or m/84'/0'/1'/0/1

            var levels = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            HDNode[] hierarchies = new HDNode[levels.Length - 1];
            if(levels[1] == "84'" || levels[1] == "49'" || levels[1] == "44'") {
                if(levels.Length <= 6) {
                    derivationScheme_ = levels[0];
                }
                else {
                    throw new Exception("Not recognised as derivation scheme");
                }
            }
            else {
                /*deprecated*/
                derivationScheme_ = "32";
            }
            

            for(int i = 1; i < levels.Length; i++) {
                if (levels[i].ElementAt(levels[i].Length - 1) == '\'') { //hardened
                    int index = Convert.ToInt32(levels[i].Substring(0, levels[i].Length - 1));
                    hierarchies[i-1] = new HDNode(index, true);
                }
                else {
                    int index = Convert.ToInt32(levels[i]);
                    hierarchies[i-1] = new HDNode(index);
                }
            }

            hierarchies_ = hierarchies;
       }

        public bool isBip84() {
            return derivationScheme_ == "84'";
        }

        public bool isBip49() {
            return derivationScheme_ == "49'";
        }

        public bool isBip44() {
            return derivationScheme_ == "44'";
        }

        public bool isBip32() {
            return derivationScheme_ == "32";
        }
    }
}
