using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    public class GetHeadersMessage : GenericMessage {

        int numOfHashes_;
        int version_;
        byte[][] startBlock_;
        byte[] endBlock_;

        public GetHeadersMessage(int version = 70015,
            int numOfHashes = 1,
            byte[][] startBlock = null,
            byte[] endBlock = null) {

            version_ = version;
            numOfHashes_ = numOfHashes;
            if (startBlock == null) {
                throw new Exception("Start Block is required");
            }
            startBlock_ = startBlock;
            if (endBlock == null) {
                endBlock_ = new byte[32];
            } else {
                endBlock_ = endBlock;
            }

            command_ = Encoding.UTF8.GetBytes("getheaders");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {

            byte[] version = BitConverter.GetBytes(version_);
            byte[] numOfHashes = Helper.encodeVarInt(numOfHashes_);
            byte[] startBlock = new byte[32];
            byte[] endBlock = new byte[32];

            foreach (byte[] hash in startBlock_) {
                Array.Reverse(hash);
                startBlock = Byte.join(startBlock, hash);
            }
            endBlock = endBlock_;
            Array.Reverse(endBlock);

            byte[] result = Byte.join(version, numOfHashes);
            result = Byte.join(result, startBlock);
            result = Byte.join(result, endBlock);
            return result;
        }
    }
}
