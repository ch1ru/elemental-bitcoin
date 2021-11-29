using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    class VerackMessage : GenericMessage {

        public VerackMessage() {

            command_ = Encoding.UTF8.GetBytes("verack");
            payload_ = null;
        }

        public override byte[] serialise() {
            return Encoding.ASCII.GetBytes("");
        }
    }
}
