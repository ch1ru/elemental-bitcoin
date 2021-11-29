using System;
using System.Numerics;
using LBitcoin.Ecc;

namespace LBitcoin {

    class HDPrivateKey : PrivateKey {

        int Depth_;
        byte[] Fingerprint_;
        uint Index_;
        byte[] Chaincode_;
        bool IsHardened_;
        bool Testnet_;

        public static readonly uint BIP32_HARDENED = 0x80000000u;

        public HDPrivateKey(PrivateKey privateKey, byte[] chaincode, int depth,
            byte[] parentFingerprint, uint index = 0, bool isHardened = false, bool testnet = false)
            : base(privateKey) { 

            if (privateKey.ToBytes().Length != 32) {
                throw new Exception("Value not a valid private key");
            }

            Chaincode_ = chaincode;
            Depth_ = depth;
            Fingerprint_ = parentFingerprint;
            Index_ = isHardened ? index | BIP32_HARDENED : index;
            IsHardened_ = isHardened;
            Testnet_ = testnet;
        }

        /*Creates an xpriv at master node*/
        public HDPrivateKey(byte[] secret, byte[] chaincode, bool testnet = false) : base(secret) {
            if (secret.Length != 32) {
                throw new Exception("Secret too small");
            }
            if(chaincode.Length != 32) {
                throw new Exception("Chain code too small");
            }

            Chaincode_ = chaincode; //right bits
            Fingerprint_ = new byte[] { 0x00, 0x00, 0x00, 0x00 }; //master key
            Depth_ = 0x00;
            Index_ = 0x00000000;
            Testnet_ = testnet;
            IsHardened_ = false;
        }

        /*Creates an xpriv from mnemonic*/
        public HDPrivateKey(Mnemonic mnemonic, bool testnet = false) : 
            this(mnemonic.Seed[0..32], mnemonic.Seed[32..64], testnet) {
        }

        /*Creates an xpriv from mnemonic*/
        public HDPrivateKey(string mnemonicPhrase, string password = null, bool testnet = false) :
            this(new Mnemonic(mnemonicPhrase, password).Seed[0..32], 
                new Mnemonic(mnemonicPhrase, password).Seed[32..64], testnet) {
        }


        /*Creates a master public key by removing the private key*/
        public HDPublicKey Neuter() {
            return new HDPublicKey(
                base.pubKey(), Chaincode_, Depth_, Fingerprint_, Index_, Testnet_);
        }


        /*Derives Xpub according to BIP32*/
        public HDPublicKey XPub(uint index, bool isHardened = false) {
            uint childNum = isHardened ? BIP32_HARDENED | index : index;
            return this.ChildAt(index, isHardened).Neuter();
        }

        public HDPublicKey XPub(HDPath path = null) {

            if (path == null) return this.Neuter();

            HDPrivateKey childKey = this; //set child to parent initially

            foreach (var level in path.hierarchies_) {
                childKey = childKey.ChildAt(level.child_, level.isHardened_);
            }

            return childKey.Neuter();
        }

        public HDPublicKey YPub() {
            return null;
        }

        public HDPublicKey ZPub() {
            return null;
        }

        byte[] CalculateFingerprint() {
            byte[] h160 = Hash.hash160(pubKey().Compressed);
            return h160[0..4];
        }


        public HDPrivateKey ChildAt(string derPath) {
            HDPath derivation = new HDPath(derPath);
            return ChildAt(derivation);
        }

        public HDPrivateKey ChildAt(HDPath derPath) {

            HDPrivateKey childKey = this; //set child to parent initially

            foreach (var level in derPath.hierarchies_) {
                childKey = childKey.ChildAt(level.child_, level.isHardened_);
            }

            return childKey;
        }

        public HDPrivateKey ChildAt(uint index, bool isHardened = false) {

            /*non hardened keys between 0 ~ 2^31 - 1, hardened between 2^31 ~ 2^32 - 1*/
            /*Note: A hardened key at index less than 2^31 will automatically assume it starts at 2^31*/
            if (index >= BIP32_HARDENED && !isHardened) {
                throw new Exception("Index too large for non-hardened key");
            }
            uint childNum = isHardened ? index | BIP32_HARDENED : index;

            /*perform hmac*/
            byte[] hmac;
            byte[] childNumBytes = BitConverter.GetBytes(childNum);
            Array.Reverse(childNumBytes);

            if (isHardened) {
                byte[] pkBytes = Byte.prependByte(ToBytes(), 0x00);
                byte[] data = Byte.join(pkBytes, childNumBytes);
                hmac = Hash.HMACSHA512Encode(data, Chaincode_);
            } 
            else {
                byte[] data = Byte.join(pubKey().Compressed, childNumBytes);
                hmac = Hash.HMACSHA512Encode(data, Chaincode_);
            }

            /*Properties of child*/
            byte[] pre = hmac[0..32];
            byte[] chaincode = hmac[32..64];
            byte[] fingerprint = Hash.hash160(pubKey().Compressed)[0..4];
            int depth = this.Depth_ + 1;

            /*Perform addition of the parent and child in the field of N*/
            FieldElement parent = new FieldElement(new BigInteger(base.ToBytes(), true, true), Secp256k1.N);
            FieldElement child = new FieldElement(new BigInteger(pre, true, true), Secp256k1.N);
            FieldElement sum = parent + child;
            PrivateKey privateKey = new PrivateKey(sum.Num);
            HDPrivateKey childKey = new HDPrivateKey(
                privateKey, chaincode, depth, fingerprint, childNum, this.Testnet_);
            return childKey;
        }

        public override string ToString() {
            return Base58Check.Base58CheckEncoding.Encode(this.Serialise());
        }


        public byte[] Serialise() {
            /*version*/
            byte[] version;
            if (Testnet_) {
                version = new byte[] { 0x04, 0x35, 0x83, 0x94 };
            } else {
                version = new byte[] { 0x04, 0x88, 0xAD, 0xE4 };
            }
            /*Depth*/
            byte[] result = Byte.appendByte(version, Convert.ToByte(Depth_));
            /*fingerprint*/
            result = Byte.join(result, Fingerprint_);
            /*index*/
            byte[] childNumBytes = BitConverter.GetBytes(Index_);
            Array.Reverse(childNumBytes);
            result = Byte.join(result, childNumBytes);
            /*chaincode*/
            result = Byte.join(result, Chaincode_);
            /*private key*/
            byte[] pkBytes = Byte.prependByte(base.ToBytes(), 0x00);
            result = Byte.join(result, pkBytes);
            return result;
        }

        public HDPrivateKey Parse(string xpriv) {
            byte[] bytes = Base58Check.Base58CheckEncoding.Decode(xpriv);
            bool testnet;
            if (bytes[1] == 0x35 && bytes[2] == 0x83 && bytes[3] == 0x94) {
                testnet = true;
            } else if (bytes[1] == 0x88 && bytes[2] == 0xAD && bytes[3] == 0xE4) {
                testnet = false;
            } else {
                throw new Exception("Invalid address version bytes");
            }

            int depth = Convert.ToInt32(bytes[4]);
            byte[] parentFingerprint = bytes[5..9];
            uint index = BitConverter.ToUInt32(bytes[9..13]);
            byte[] chaincode = bytes[13..45];
            PrivateKey privateKey = new PrivateKey(bytes[45..78]);

            return new HDPrivateKey(privateKey, chaincode, depth,
                parentFingerprint, index, testnet: testnet);
        }

        public int Depth { get { return Depth_; } }

        public byte[] Fingerprint { get { return Fingerprint_; } }

        public uint Index { get { return Index_; } }

        public byte[] Chaincode { get { return Chaincode_; } }

        public bool IsHardened { get { return IsHardened_; } }

        public bool Testnet { get { return Testnet_; } }
    }
}
