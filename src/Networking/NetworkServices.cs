using System;
using System.Collections;

namespace LBitcoin.Networking {

    /// <summary>
    /// Services the node supports, sent in the version message.
    /// </summary>
    public class NetworkServices {

        public static int NETWORK = 1;
        public static int GETUTXO = 2;
        public static int BLOOM = 4;
        public static int WITNESS = 8;
        public static int NETWORK_LIMITED = 1024;

        protected byte[] services_;

        public byte[] GetServices() {
            return services_;
        }

        public bool IsNetwork() {
            return CheckBit(services_, 1);
        }

        public bool IsGetUtxo() {
            return CheckBit(services_, 2);
        }

        public bool IsBloom() {
            return CheckBit(services_, 3);
        }

        public bool IsWitness() {
            return CheckBit(services_, 4);
        }

        public bool IsNetworkLimited() {
            return CheckBit(services_, 10);
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

        bool CheckBit(byte[] services, int index) {
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
            string network = IsNetwork() ? "TRUE" : "FALSE";
            string getUtxo = IsGetUtxo() ? "TRUE" : "FALSE";
            string bloom = IsBloom() ? "TRUE" : "FALSE";
            string witness = IsWitness() ? "TRUE" : "FALSE";
            string limited = IsNetworkLimited() ? "TRUE" : "FALSE";

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
