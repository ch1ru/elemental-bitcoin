using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    class NotFoundMessage : InvMessage {

        public NotFoundMessage(List<(int, byte[])> data) : base(data) {
            command_ = Encoding.UTF8.GetBytes("notfound");
            payload_ = base.serialise();
        }
    }
}
