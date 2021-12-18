using System;
using System.Collections;

namespace LBitcoin.Networking {

    /// <summary>
    /// Services the node supports, sent in the version message.
    /// </summary>
    class NetworkServices {

        public static int NETWORK = 1;
        public static int GETUTXO = 2;
        public static int BLOOM = 4;
        public static int WITNESS = 8;
        public static int NETWORK_LIMITED = 1024;

        protected byte[] services_;

        public byte[] getServices() {
            return services_;
        }

        public bool isNetwork() {
            return checkBit(services_, 1);
        }

        public bool isGetUtxo() {
            return checkBit(services_, 2);
        }

        public bool isBloom() {
            return checkBit(services_, 3);
        }

        public bool isWitness() {
            return checkBit(services_, 4);
        }

        public bool isNetworkLimited() {
            return checkBit(services_, 10);
        }


        public NetworkServices(byte[] services) {
            if (services.Length == 8) {
                services_ = services;
            } else {
                throw new Exception("service bytes aren't correct length");
            }
        }

        public NetworkServices(
            bool network = false,
            bool getUtxo = false,
            bool bloom = false,
            bool witness = false,
            bool networkLimited = false) {

            bool[] boolArray = new bool[11];
            boolArray[0] = network; 
            boolArray[1] = getUtxo;
            boolArray[2] = bloom; 
            boolArray[3] = witness;
            boolArray[10] = networkLimited;
            BitArray bitArray = new BitArray(boolArray);
            Int64 servicesInt = Helper.getIntFromBitArray(bitArray);
            services_ = BitConverter.GetBytes(servicesInt);
        }

        bool checkBit(byte[] services, int index) {
            BitArray servicesBitArr = new BitArray(services);
            bool[] mask = new bool[servicesBitArr.Length];
            mask[index] = true;
            BitArray bitMask = new BitArray(mask);
            BitArray result = bitMask.And(servicesBitArr);
            if(result[index]) {
                return true;
            }
            return false;
        }

        public override string ToString() {
            string network = isNetwork() ? "TRUE" : "FALSE";
            string getUtxo = isGetUtxo() ? "TRUE" : "FALSE";
            string bloom = isBloom() ? "TRUE" : "FALSE";
            string witness = isWitness() ? "TRUE" : "FALSE";
            string limited = isNetworkLimited() ? "TRUE" : "FALSE";

            return (
                "Network: " + network +
                "\nGet UTXO: " + getUtxo +
                "\nBloom: " + bloom +
                "\nWitness " + witness +
                "\nNetwork limited: " + limited
                );
        }
    }
}
