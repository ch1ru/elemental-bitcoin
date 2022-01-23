using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    public class PongMessage : GenericMessage {

        byte[] nonce_;

        public PongMessage(byte[] nonce) {
            nonce_ = nonce;
            command_ = Encoding.UTF8.GetBytes("pong");
            payload_ = this.serialise();
        }

        public static PongMessage Parse(Stream s) {
            byte[] nonce = new byte[8];
            s.Read(nonce, 0, 8);
            return new PongMessage(nonce);
        }

        public override byte[] serialise() {
            return nonce_;
        }
    }
}
