using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    class GetAddrMessage : GenericMessage {

        public GetAddrMessage() {
            command_ = Encoding.UTF8.GetBytes("getaddr");
            payload_ = null;
        }
    }
}
