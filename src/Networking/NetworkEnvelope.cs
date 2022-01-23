using System;
using System.Text;
using System.IO;

namespace LBitcoin.Networking {

    /// <summary>
    /// Creates a network wrapper for the bitcoin message. 
    /// For more info see <see href="https://en.bitcoin.it/wiki/Protocol_documentation#Message_structure">message structure</see>.
    /// </summary>
    public class NetworkEnvelope {

        byte[] magic_;
        byte[] command_;
        byte[] payloadLength_;
        byte[] checksum_;
        byte[] payload_;

        public static byte[] NETWORK_MAGIC = { 0xf9, 0xbe, 0xb4, 0xd9 };
        public static byte[] TESTNET_NETWORK_MAGIC = { 0x0b, 0x11, 0x09, 0x07 };

        public byte[] Magic { get { return magic_; } }

        public string Command { get { return Encoding.UTF8.GetString(command_); } }

        public int PayloadLength { get { return BitConverter.ToInt32(payloadLength_); } }

        public byte[] Checksum { get { return checksum_; } }

        public byte[] Payload { get { return payload_; } }

        public byte[] GetHeaderBytes() {
            byte[] header = Byte.join(this.magic_, Encoding.UTF8.GetBytes(this.Command));
            header = Byte.join(header, payloadLength_);
            header = Byte.join(header, checksum_);
            return header;
        }

        public NetworkEnvelope(byte[] command, byte[] payload = null, bool testnet = false) {
            int paddingSize = 12 - command.Length;
            byte[] padding = new byte[paddingSize];
            for (int i = 0; i < paddingSize; i++) {
                padding[i] = 0x00;
            }
            command_ = Byte.join(command, padding);
            payload_ = payload;
            if (testnet) {
                magic_ = TESTNET_NETWORK_MAGIC;
            } 
            else {
                magic_ = NETWORK_MAGIC;
            }

            /*Sometimes the payload is empty*/
            if (payload == null) {
                checksum_ = new byte[] { 0x5D, 0xF6, 0xE0, 0xE2 };
            } 
            else {
                checksum_ = Hash.hash256(payload)[0..4];
            }

            payloadLength_ = payload == null ? null : BitConverter.GetBytes(payload.Length);
        }

        public static NetworkEnvelope Parse(Stream s, bool testnet = false) {
            byte[] magic = new byte[4];
            s.Read(magic, 0, 4);
            byte[] command = new byte[12];
            s.Read(command, 0, 12);
            byte[] payloadLength = new byte[4];
            s.Read(payloadLength, 0, 4);
            byte[] payload = new byte[BitConverter.ToInt32(payloadLength)];
            s.Read(payload, 0, BitConverter.ToInt32(payloadLength));
            NetworkEnvelope netMessage = new NetworkEnvelope(command, payload, testnet);
            return netMessage;
        }

        public byte[] Serialise() {
            byte[] tmp = Byte.join(magic_, command_);
            tmp = Byte.join(tmp, payloadLength_);
            tmp = Byte.join(tmp, checksum_);
            return Byte.join(tmp, payload_);
        }

        public override bool Equals(object obj) {
            byte[] test = Encoding.UTF8.GetBytes((string)obj);
            if(test.Length > command_.Length) {
                return false; //would result in overflow
            }
            for (int i = 0; i < test.Length; i++) {
                if(test[i] != this.command_[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}
    
