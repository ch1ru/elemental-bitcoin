using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    public class SendHeadersMessage : GenericMessage {

        public SendHeadersMessage() {
            command_ = Encoding.UTF8.GetBytes("sendheaders");
            payload_ = null;
        }
    }
}
