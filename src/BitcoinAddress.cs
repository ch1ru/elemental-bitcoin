using System;
using Bech32_Csharp;
using LBitcoin.Ecc;

namespace LBitcoin {


    public readonly struct AddressType {
        public const string legacy = "legacy";
        public const string nativeSegwit = "native segwit";
        public const string p2wsh = "p2wsh";
        public const string legacyScript = "legacy script";
    }

    /// <summary>
    /// Bitcoin bech32 & base58 address class.
    /// </summary>
    class BitcoinAddress {

        public string address_ = "";
        byte[] hash_;

        /// <summary>
        /// Constructor. Creates an address from elliptic curve point. Default is native segwit.
        /// </summary>
        /// <param name="p">Elliptic curve point.</param>
        /// <param name="type">Type of address.</param>
        public BitcoinAddress(Point p, string type = AddressType.nativeSegwit, bool testnet = false, 
            bool isCompressed = true) : this(isCompressed ? Hash.hash160(new PublicKey(p).Compressed) 
                : Hash.hash160(new PublicKey(p).Uncompressed), type, testnet: testnet) {

            PublicKey pub = new PublicKey(p);

            hash_ = isCompressed ? Hash.hash160(pub.Compressed) :
                Hash.hash160(pub.Uncompressed);

        }
            
        /// <summary>
        /// Constructor. Creates bitcoin address from public key. Default is native segwit.
        /// </summary>
        /// <param name="pub">Public key.</param>
        /// <param name="type">Address type.</param>
        public BitcoinAddress(PublicKey pub, string type = AddressType.nativeSegwit, 
            bool testnet = false, bool isCompressed = true)
            : this(isCompressed ? Hash.hash160(pub.Compressed) 
                  : Hash.hash160(pub.Uncompressed), type, testnet: testnet) {

            hash_ = isCompressed ? Hash.hash160(pub.Compressed) :
                Hash.hash160(pub.Uncompressed);
        }

        /// <summary>
        /// Constructor. Creates address from string. Automatically detects type.
        /// </summary>
        /// <param name="addr">Address in <see cref="string"/> format.</param>
        public BitcoinAddress(string addr) {

            address_ = addr;
            if(addr[0] == '3') { //p2sh mainnet
                hash_ = this.getHash(segwit: false, type: 1, testnet: false);
            }
            else if(addr[0] == '2') { //p2sh testnet
                hash_ = this.getHash(segwit: false, type: 1, testnet: true);
            }
            else if(addr[0] == '1') { //p2pkh mainnet legacy
                hash_ = this.getHash(segwit: false, type: 0, testnet: false);
            }
            else if(addr[0..2] == "bc") {
                //p2wpkh or p2wsh mainnet
                if (addr.Length > 50) {
                    hash_ = this.getHash(segwit: true, type: 1, testnet: false);
                }
                else {
                    hash_ = this.getHash(segwit: true, type: 0, testnet: false);
                }
            }
            else if(addr[0] == 'm' || addr[0] == 'n') { // p2pkh testnet legacy
                hash_ = this.getHash(segwit: false, type: 0, testnet: true);
            }
            else if(addr[0..2] == "tb") { //p2wpkh testnet
                hash_ = this.getHash(segwit: true, type: 0, testnet: true);
            }
            else {
                throw new Exception("Unrecognised address format");
            }
        }

        /// <summary>
        /// Constructor. Creates bitcoin address from hash. Default is native segwit.
        /// </summary>
        /// <param name="hash">Hash of public key as byte array.</param>
        public BitcoinAddress(
            byte[] hash, string type = AddressType.nativeSegwit, bool testnet = false) {

            hash_ = hash;

            switch (type) {
                case AddressType.nativeSegwit:
                    address_ = Converter.EncodeBech32(0x00, hash_, isP2PKH: true, mainnet: !testnet);
                    break;
                case AddressType.p2wsh:
                    address_ = Converter.EncodeBech32(0x00, hash_, isP2PKH: false, mainnet: !testnet);
                    break;
                case AddressType.legacyScript:
                    encodeAddressLegacy(hash_, isP2PKH: false, testnet: testnet);
                    break;
                case AddressType.legacy:
                    encodeAddressLegacy(hash_, isP2PKH: true, testnet: testnet);
                    break;
                default:
                    throw new Exception("Unrecognised address format");
            }
        }

        public override string ToString() {
            return address_;
        }

        /// <summary>
        /// Creates legacy address (starting with 1).
        /// </summary>
        /// <param name="hash">Hash of public key.</param>
        /// <returns>Address as <see cref="string"/>.</returns>
        public static string encodeAddressLegacy(byte[] hash, bool isP2PKH = true, bool testnet = false) {
            byte prefix = 0x00;
            
            if (testnet) {
                prefix = isP2PKH ?  (byte)0x6f : (byte)0xc4;
            }
            else {
                prefix = isP2PKH ? (byte)0x00 : (byte)0x05;
            }

            byte[] bytes = Byte.prependByte(hash, prefix);
            string b58check = Base58Check.Base58CheckEncoding.Encode(bytes); //encode in base58 check
            return b58check;
        }

        /// <summary>
        /// Gets the hash the address is comprised of.
        /// </summary>
        public byte[] getHash(bool segwit, byte type, bool testnet = false) {
            byte[] hash = new byte[] { };
            if(segwit) {
                bool mainnet = !testnet;
                hash = Converter.DecodeBech32(address_, out _, out type, out mainnet);
            }
            else {
                hash = Base58Check.Base58CheckEncoding.Decode(address_);
            }

            if(hash.Length == 21) { //includes version byte
                return hash[1..21];
            }
            return hash;
        }
    }
}
