using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    class BlockMessage : GenericMessage {

        Block block_;

        public BlockMessage(Block block) {
            block_ = block;
            command_ = Encoding.UTF8.GetBytes("block");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {
            byte[] result = block_.getHeader();
            int txCount = block_.TxHashes.Count;
            result = Byte.join(result, BitConverter.GetBytes(txCount));
            if (block_.Transactions.Count < 1) { //at least 1 (coinbase) tx
                throw new Exception("Insufficient transactions in block");
            }
            foreach (Transaction tx in block_.Transactions) {
                result = Byte.join(result, tx.Serialise());
            }
            return result;
        }

        public static new BlockMessage Parse(Stream s) {
            Block block = Block.Parse(s); //create tx header
            int numOfTxs = Helper.getVarIntLength(s);
            List<Transaction> txs = new List<Transaction>();
            for (int i = 0; i < numOfTxs; i++) { //add txs
                txs.Add(Transaction.Parse(s));
            }
            block.fillTransactions(txs);
            return new BlockMessage(block);
        }
    }
}
