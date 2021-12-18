using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    class TxMessage : GenericMessage {

        Transaction tx_;

        public TxMessage(Transaction tx) {
            tx_ = tx;
            command_ = Encoding.UTF8.GetBytes("tx");
            payload_ = tx_.Serialise();
        }

        public static new TxMessage Parse(Stream s) {
            return new TxMessage(Transaction.Parse(s));
        }
    }
}
