using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    public class GetBlocksMessage : GetHeadersMessage {

        public GetBlocksMessage(
            int version = 70015,
            int numOfHashes = 1,
            byte[][] startBlock = null,
            byte[] endBlock = null) : base(version, numOfHashes, startBlock, endBlock) {

            command_ = Encoding.UTF8.GetBytes("getblocks");
            payload_ = base.serialise();
        }
    }
}
