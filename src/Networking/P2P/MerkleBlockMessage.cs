using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    class MerkleBlockMessage : GenericMessage {

        MerkleBlock mrklBlock_;

        public MerkleBlockMessage(MerkleBlock mrklBlock) {
            mrklBlock_ = mrklBlock;
            command_ = Encoding.UTF8.GetBytes("merkleblock");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {
            /*header*/
            byte[] result = mrklBlock_.getHeader();
            /*num of txs, including ones that don't fit the filter*/
            result = Byte.join(result, BitConverter.GetBytes(mrklBlock_.TotalTxs));
            /*hashes*/
            byte[] numOFHashes = Helper.encodeVarInt(mrklBlock_.TxHashes.Count);
            result = Byte.join(result, numOFHashes);
            foreach(byte[] hash in mrklBlock_.TxHashes) {
                result = Byte.join(result, hash);
            }
            /*Flag bytes*/
            byte[] numOfFlagBytes = Byte.join(result, Helper.encodeVarInt(mrklBlock_.Flags.Length));
            result = Byte.join(result, mrklBlock_.Flags);
            return result;
        }
    }
}
