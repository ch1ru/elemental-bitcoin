using System;
using System.Numerics;
using LBitcoin.Ecc;
using System.Text;

namespace LBitcoin {

    /// <summary>
    /// Extended private key for use in HD wallets.
    /// </summary>
    class HDPrivateKey : PrivateKey {

        int Depth_;
        byte[] Fingerprint_;
        uint Index_;
        byte[] Chaincode_;
        bool IsHardened_;
        bool Testnet_;
        uint Type_;

        public static readonly uint BIP32_HARDENED = 0x80000000u;
        static readonly byte[] BIP32_KEY = Encoding.UTF8.GetBytes("Bitcoin seed");


        public HDPrivateKey(PrivateKey privateKey, byte[] chaincode, int depth,
            byte[] parentFingerprint, uint index = 0, bool isHardened = false, bool testnet = false, uint type = 0)
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
            if(type == 0 || type == 1 || type == 2) {
                Type_ = type;
            }
            else {
                throw new Exception("Invalid type id");
            }
        }

        /// <summary>
        /// Constructor. Creates xpriv at master node.
        /// </summary>
        public HDPrivateKey(byte[] secret, byte[] chaincode, bool testnet = false, uint type = 0) : base(secret) {
            if (secret.Length != 32) {
                throw new Exception("Secret too small");
            }
            if(chaincode.Length != 32) {
                throw new Exception("Chain code too small");
            }

            Chaincode_ = chaincode;
            Fingerprint_ = new byte[] { 0x00, 0x00, 0x00, 0x00 }; //master key
            Depth_ = 0x00;
            Index_ = 0x00000000;
            Testnet_ = testnet;
            IsHardened_ = false;
            Type_ = type;
        }

        /// <summary>
        /// Constructor. Creates xpriv from mnemonic.
        /// </summary>
        /// <param name="mnemonic">Mnemonic as <see cref="Mnemonic"/>.</param>
        public HDPrivateKey(Mnemonic mnemonic, string passphrase = "", bool testnet = false, uint type = 0) : 
            this(Hash.HMACSHA512Encode(mnemonic.ToSeed(passphrase), BIP32_KEY)[0..32],
                Hash.HMACSHA512Encode(mnemonic.ToSeed(passphrase), BIP32_KEY)[32..64], 
            testnet, type) {
        }

        /// <summary>
        /// Constructor. Creates an xpriv from mnemonic string.
        /// </summary>
        /// <param name="mnemonicPhrase">Mnemonic as string.</param>
        public HDPrivateKey(string mnemonicPhrase, string passphrase = null, bool testnet = false, uint type = 0) 
            : this(Hash.HMACSHA512Encode(new Mnemonic(mnemonicPhrase, passphrase).ToSeed(passphrase), BIP32_KEY)[0..32],
                Hash.HMACSHA512Encode(new Mnemonic(mnemonicPhrase, passphrase).ToSeed(passphrase), BIP32_KEY)[0..32], 
                testnet, type) {
        }


        /// <summary>
        /// Creates a master public key by removing the private key.
        /// </summary>
        /// <returns>The <see cref="HDPublicKey"/> xpub.</returns>
        public HDPublicKey Neuter() {
            return new HDPublicKey(
                base.pubKey(), Chaincode_, Depth_, Fingerprint_, Index_, Testnet_, Type_);
        }


        /// <summary>
        /// Derives Xpub according to BIP32.
        /// </summary>
        /// <param name="index">Index of pubkey.</param>
        /// <param name="isHardened">Whether the key is hardened or not.</param>
        /// <param name="type">The type of <see cref="HDPublicKey"/>
        /// 0 = xpub, 1 = ypub, 2 = zpub.</param>
        /// <returns></returns>
        public HDPublicKey XPub(uint index, bool isHardened = false, uint type = 0) {
            uint childNum = isHardened ? BIP32_HARDENED | index : index;
            return this.ChildAt(index, isHardened).Neuter();
        }

        public HDPublicKey XPub(HDPath path = null, uint type = 0) {

            if (path == null) return this.Neuter();

            HDPrivateKey childKey = this; //set child to parent initially

            foreach (var level in path.hierarchies_) {
                childKey = childKey.ChildAt(level.child_, level.isHardened_);
            }

            return childKey.Neuter();
        }

        /// <summary>
        /// Derives Ypub according to BIP49.
        /// </summary>
        public HDPublicKey YPub(uint index, bool isHardened = false) {
            return XPub(index, isHardened, type: 1);
        }

        public HDPublicKey YPub(HDPath path = null) {
            return XPub(path, type: 1);
        }

        /// <summary>
        /// Derives Xpub according to BIP84.
        /// </summary>
        public HDPublicKey ZPub(uint index, bool isHardened = false) {
            return XPub(index, isHardened, type: 2);
        }

        public HDPublicKey ZPub(HDPath path = null) {

            return XPub(path, type: 2);
        }

        byte[] CalculateFingerprint() {
            byte[] h160 = Hash.hash160(pubKey().Compressed);
            return h160[0..4];
        }

        /// <summary>
        /// Derives a child private key from the parent.
        /// </summary>
        /// <param name="derPath">Derivation path as string.</param>
        public HDPrivateKey ChildAt(string derPath) {
            HDPath derivation = new HDPath(derPath);
            return ChildAt(derivation);
        }

        /// <summary>
        /// Derives a child private key from the parent.
        /// </summary>
        /// <param name="derPath">Derivation path as <see cref="HDPath"/></param>
        public HDPrivateKey ChildAt(HDPath derPath) {

            HDPrivateKey childKey = this; //set child to parent initially

            foreach (var level in derPath.hierarchies_) {
                childKey = childKey.ChildAt(level.child_, level.isHardened_);
            }

            return childKey;
        }

        /// <summary>
        /// Derives a child private key from the parent.
        /// </summary>
        /// <param name="index">Index of child.</param>
        /// <param name="isHardened">If the child is derived from a hardened key.</param>
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
                privateKey, chaincode, depth, fingerprint, childNum, isHardened, this.Testnet_, this.Type_);
            return childKey;
        }

        public override string ToString() {
            return Base58Check.Base58CheckEncoding.Encode(this.Serialise());
        }

        /// <summary>
        /// Serialises a HD private key.
        /// </summary>
        public byte[] Serialise() {
            /*version*/
            byte[] version = new byte[4];
            /*Bip32 xpriv*/
            if(Type_ == 0) {
                if (Testnet_) {
                    version = new byte[] { 0x04, 0x35, 0x83, 0x94 };
                } else {
                    version = new byte[] { 0x04, 0x88, 0xAD, 0xE4 };
                }
            }
            /*Bip49 ypriv*/
            else if(Type_ == 1) {
                if (Testnet_) {
                    version = new byte[] { 0x04, 0x4a, 0x4e, 0x28 };
                } else {
                    version = new byte[] { 0x04, 0x9d, 0x78, 0x78 };
                }
            }
            /*Bip84 zpriv*/
            else if(Type_ == 2) {
                if (Testnet_) {
                    version = new byte[] { 0x04, 0x5f, 0x18, 0xbc };
                } else {
                    version = new byte[] { 0x04, 0xb2, 0x43, 0x0c };
                }
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

        /// <summary>
        /// Parses a HD private key.
        /// </summary>
        /// <param name="xpriv">xpriv as string.</param>
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
