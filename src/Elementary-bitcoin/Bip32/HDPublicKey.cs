using System;
using System.Numerics;
using LBitcoin.Ecc;

namespace LBitcoin {
    class HDPublicKey : PublicKey {

        byte[] Chaincode_;
        bool testnet_;
        int Depth_;
        byte[] Fingerprint_;
        uint Index_;

        public HDPublicKey(PublicKey publicKey, byte[] chaincode, int depth, 
            byte[] parentFingerprint, uint index = 0, bool testnet = false) : base(publicKey) {

            Chaincode_ = chaincode;
            Depth_ = depth;
            Fingerprint_ = parentFingerprint;
            Index_ = index;
            testnet_ = testnet;
        }

        public override string ToString() {
            return Base58Check.Base58CheckEncoding.Encode(this.Serialise());
        }

        public byte[] Serialise() {
            /*version*/
            byte[] version = new byte[4];
            if(testnet_) {
                version = new byte[] { 0x04, 0x35, 0x87, 0xCF };
            }
            else {
                version = new byte[] { 0x04, 0x88, 0xB2, 0x1E };
            }
            /*depth*/
            byte[] result = Byte.appendByte(version, Convert.ToByte(Depth_));
            /*parent fingerprint*/
            result = Byte.join(result, Fingerprint_);
            /*index*/
            byte[] childNumBytes = BitConverter.GetBytes(Index_);
            Array.Reverse(childNumBytes);
            result = Byte.join(result, childNumBytes);
            /*chaincode*/
            result = Byte.join(result, Chaincode_);
            /*pub key*/
            result = Byte.join(result, base.Compressed);

            byte[] checksum = Hash.hash256(result)[0..4];
            return result;
        }

        public new HDPublicKey Parse(string extPub) {
            byte[] bytes = Base58Check.Base58CheckEncoding.Decode(extPub);
            bool testnet;
            if (bytes[1] == 0x35 && bytes[2] == 0x87 && bytes[3] == 0xCF) {
                testnet = true;
            } else if (bytes[1] == 0x88 && bytes[2] == 0xB2 && bytes[3] == 0x1E) {
                testnet = false;
            } else {
                throw new Exception("Invalid address version bytes");
            }

            int depth = Convert.ToInt32(bytes[4]);
            byte[] parentFingerprint = bytes[5..9];
            uint index = BitConverter.ToUInt32(bytes[9..13]);
            byte[] chaincode = bytes[13..45];
            PublicKey publicKey = new PublicKey(bytes[45..78]);

            return new HDPublicKey(publicKey, chaincode, depth,
                parentFingerprint, index, testnet: testnet);
        }


        public HDPublicKey childAt(string path) {
            HDPath derivation = new HDPath(path);
            return childAt(derivation);
        }

        public HDPublicKey childAt(HDPath path) {

            HDPublicKey childKey = this; //set child to parent initially

            foreach (var level in path.hierarchies_) {
                childKey = childKey.childAt(level.child_);
            }

            return childKey;
        }

        public HDPublicKey childAt(uint index) {

            if(index >= HDPrivateKey.BIP32_HARDENED) { //hardened address
                throw new Exception("Public key cannot derrive from hardened parent public key");
            }

            /*HMAC*/
            byte[] indexBytes = BitConverter.GetBytes(index);
            Array.Reverse(indexBytes);
            byte[] hmac = Hash.HMACSHA512Encode(Byte.join(base.Compressed, indexBytes), Chaincode_);
            byte[] pre = hmac[0..32];
            byte[] chaincode = hmac[32..64];
            byte[] fingerprint = Hash.hash160(base.Compressed)[0..4];

            /*left bits are multiplied by base point, then added to parent coordinate*/
            Secp256k1 curve = new Secp256k1();
            Point256 basePoint = curve.getGeneratorPoint();
            BigInteger scalar = new BigInteger(pre, true, true);
            Point256 parent = Point256.Parse(base.Compressed);
            Point p = (basePoint * scalar) + parent;
            PublicKey childKey = new PublicKey(p);

            return new HDPublicKey(childKey, chaincode, Depth_ + 1, fingerprint, index, testnet_);
        }

        public byte[] chaincode { get { return Chaincode_; } }

        public byte[] Fingerprint { get { return Fingerprint_; } }

        public bool Testnet { get { return testnet_; } }

        public int Depth { get { return Depth_; } }

        public uint Index { get { return Index_; } }
    }
}
