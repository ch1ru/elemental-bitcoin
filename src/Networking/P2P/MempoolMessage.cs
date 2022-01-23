using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    public class MempoolMessage : GenericMessage {

        public MempoolMessage() {
            command_ = Encoding.UTF8.GetBytes("mempool");
            payload_ = null;
        }
    }
}
