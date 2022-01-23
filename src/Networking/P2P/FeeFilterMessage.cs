using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    public class FeeFilterMessage : GenericMessage {

        UInt64 dustAmount_;

        public FeeFilterMessage(UInt64 dustAmount) {
            dustAmount_ = dustAmount;
            command_ = Encoding.UTF8.GetBytes("feefilter");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {
            return BitConverter.GetBytes(dustAmount_);
        }

        public static new FeeFilterMessage Parse(Stream s) {
            byte[] dustAmountBytes = new byte[8];
            s.Read(dustAmountBytes, 0, 8);
            UInt64 dustAmount = BitConverter.ToUInt64(dustAmountBytes);
            return new FeeFilterMessage(dustAmount);
        }
    }
}
