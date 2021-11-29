using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    class SendHeadersMessage : GenericMessage {

        public SendHeadersMessage() {
            command_ = Encoding.UTF8.GetBytes("sendheaders");
            payload_ = null;
        }
    }
}
