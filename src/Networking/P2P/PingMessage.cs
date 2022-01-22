using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    class PingMessage : GenericMessage {

        byte[] nonce_;

        public PingMessage(byte[] nonce) {
            nonce_ = nonce;
            command_ = Encoding.UTF8.GetBytes("ping");
            payload_ = this.serialise();
        }

        public static PingMessage parse(Stream s) {
            byte[] nonce = new byte[8];
            s.Read(nonce, 0, 8);
            return new PingMessage(nonce);
        }

        public override byte[] serialise() {
            return nonce_;
        }
    }
}
