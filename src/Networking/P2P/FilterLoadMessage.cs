using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    public class FilterLoadMessage : GenericMessage {

        BloomFilter bloomFilter_;
        uint flag_;

        public FilterLoadMessage(BloomFilter bloomFilter, uint flag = 1) {
            bloomFilter_ = bloomFilter;
            flag_ = flag;
            command_ = Encoding.UTF8.GetBytes("filterload");
            payload_ = bloomFilter.FilterLoad(flag).Payload;
        }


        public static new FilterLoadMessage Parse(Stream s) {

            int size = Helper.getVarIntLength(s);
            byte[] filter = new byte[size];
            for(int i = 0; i < size; i++) {
                byte[] singleByte = new byte[1];
                s.Read(singleByte, 0, 1);
                filter = Byte.appendByte(filter, singleByte[0]);
            }
            byte[] functionCountBytes = new byte[4];
            s.Read(functionCountBytes, 0, 4);
            uint functionCount = BitConverter.ToUInt32(functionCountBytes);
            byte[] tweakBytes = new byte[4];
            s.Read(tweakBytes, 0, 4);
            uint tweak = BitConverter.ToUInt32(tweakBytes);
            byte[] flagBytes = new byte[1];
            s.Read(flagBytes, 0, 1);
            uint flag = Convert.ToUInt32(flagBytes[0]);
            BloomFilter bloomFilter = new BloomFilter((UInt32)size, functionCount, tweak, filter);
            return new FilterLoadMessage(bloomFilter, flag);
        }
        
    }

    public class FilterAddMessage : GenericMessage {

        byte[] data_;

        public FilterAddMessage(byte[] data) {
            data_ = data;
            command_ = Encoding.UTF8.GetBytes("filteradd");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {
            if(data_.Length > 520) {
                throw new Exception("Item is too large");
            }
            byte[] size = Helper.encodeVarInt(data_.Length);
            return Byte.join(size, data_);
        }

        public static new FilterAddMessage Parse(Stream s) {
            int length = Helper.getVarIntLength(s);
            byte[] data = new byte[length];
            for(int i = 0; i < length; i++) {
                byte[] dataByte = new byte[1];
                s.Read(dataByte, 0, 1);
                data = Byte.appendByte(data, dataByte[0]);
            }
            return new FilterAddMessage(data);
        }
    }

    public class FilterClearMessage : GenericMessage {

        public FilterClearMessage() {
            command_ = Encoding.UTF8.GetBytes("filterclear");
            payload_ = null;
        }
    }
}
