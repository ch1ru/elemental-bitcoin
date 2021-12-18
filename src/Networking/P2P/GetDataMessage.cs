using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    class GetDataMessage : InvMessage {

        public GetDataMessage(List<(int, byte[])> data) : base(data) {
            command_ = Encoding.UTF8.GetBytes("getdata");
            payload_ = base.serialise();
        }
    }
}
