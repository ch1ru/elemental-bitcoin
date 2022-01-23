using System;
using System.IO;

namespace LBitcoin.Networking {

    /// <summary>
    /// Standard bitcoin network message with header and payload. 
    /// </summary>
    public class GenericMessage {

        protected byte[] command_;
        protected byte[] payload_;
        protected bool testnet_;

        public GenericMessage(byte[] command, byte[] payload, bool testnet = false) {
            command_ = command;
            payload_ = payload;
            testnet_ = testnet;
        }

        protected GenericMessage() { }

        public byte[] CommandBytes { get { return command_; } }

        public byte[] Payload { get { return payload_; } }

        public bool Testnet { get { return testnet_; } }

        public virtual byte[] serialise() {
            byte[] magic = testnet_ ? NetworkEnvelope.TESTNET_NETWORK_MAGIC :
                NetworkEnvelope.NETWORK_MAGIC;

            byte[] result = Byte.join(magic, command_);
            result = Byte.join(result, BitConverter.GetBytes(payload_.Length));
            result = Byte.join(result, Hash.hash256(payload_));
            return result;
        }

        public static GenericMessage Parse(Stream s) {
            byte[] magic = new byte[4];
            s.Read(magic, 0, 4);
            bool testnet = magic == NetworkEnvelope.TESTNET_NETWORK_MAGIC ? true : false;
            byte[] command = new byte[12];
            s.Read(command, 0, 12);
            byte[] lengthBytes = new byte[4];
            s.Read(lengthBytes, 0, 4);
            int length = BitConverter.ToInt32(lengthBytes);
            byte[] checksum = new byte[4];
            s.Read(checksum, 0, 4);
            byte[] payload = new byte[length];
            s.Read(payload, 0, length);
            if(Byte.bytesToString(Hash.hash256(payload)[0..4]) != Byte.bytesToString(checksum)) {
                throw new Exception("Invalid checksum");
            }
            return new GenericMessage(command, payload, testnet);
        }
    }
}



