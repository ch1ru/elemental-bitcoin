using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    class AddrMessage : GenericMessage {

        NetAddress[] addresses_;

        public AddrMessage(NetAddress[] addresses) {
            addresses_ = addresses;
            command_ = Encoding.UTF8.GetBytes("addr");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {

            if(addresses_.Length > 1000) {
                throw new Exception("Over maximum of 1000 addresses");
            }
            byte[] result = Helper.encodeVarInt(addresses_.Length);
            foreach(NetAddress addr in addresses_) {
                result = Byte.join(result, addr.serialise());
            }
            return result;
        }

        public static new AddrMessage Parse(Stream s) {
            int numOfAddr = Helper.getVarIntLength(s);
            NetAddress[] addresses = new NetAddress[numOfAddr];
            for(int i = 0; i < numOfAddr; i++) {
                byte[] addrBytes = new byte[30];
                s.Read(addrBytes, 0, 30);
                NetAddress addr = new NetAddress(addrBytes);
                addresses[i] = addr;
            }
            return new AddrMessage(addresses);
        }
    }
}
