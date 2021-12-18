using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    class HeadersMessage : GenericMessage {

        Block[] blockHeaders_;

        public HeadersMessage(Block[] blocks) {
            blockHeaders_ = blocks;
            payload_ = this.serialise();
            command_ = Encoding.UTF8.GetBytes("headers");
        }

        public override byte[] serialise() {
            byte[] result = null;
            foreach (Block block in blockHeaders_) {
                result = Byte.join(result, block.Serialise());
            }
            return result;
        }

        public static new HeadersMessage Parse(Stream s) {
            /*varint is num of block headers*/
            int numOfHeaders = Helper.getVarIntLength(s);
            Block[] blockHeaders = new Block[numOfHeaders];
            for (int i = 0; i < numOfHeaders; i++) {
                blockHeaders[i] = Block.Parse(s);
                /*next varint is num of txs*/
                int numOfTxs = Helper.getVarIntLength(s);
                if (numOfTxs != 0) {
                    throw new Exception("Number of Transactions not zero");
                }
            }
            return new HeadersMessage(blockHeaders);
        }
    }
}
