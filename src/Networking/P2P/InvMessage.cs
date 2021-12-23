using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBitcoin.Networking.P2P {
    class InvMessage : GenericMessage {

        List<(int, byte[])> data_;

        public InvMessage(List<(int, byte[])> data) {
            data_ = data;
            command_ = Encoding.UTF8.GetBytes("inv");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {
            var result = Helper.encodeVarInt(data_.Count);
            foreach ((int type, byte[] id) item in data_) {
                result = Byte.join(result, BitConverter.GetBytes(item.type));
                byte[] idLittleEndian = new byte[item.id.Length];
                for (int i = 1; i <= item.id.Length; i++) {
                    idLittleEndian[item.id.Length - i] = item.id[i];
                }
                result = Byte.join(result, idLittleEndian);
            }
            return result;
        }
    }
}
