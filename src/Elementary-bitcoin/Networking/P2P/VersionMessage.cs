using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LBitcoin.Networking.P2P {
    class VersionMessage : GenericMessage {


        int version_, latestBlock_;
        TimeSpan timestamp_;
        NetworkServices services_;
        NetAddress receiver_;
        NetAddress sender_;
        byte[] nonce_ = new byte[8];
        string userAgent_;
        bool relay_;


        public int Version { get { return version_; } }

        public int Height { get { return latestBlock_; } }

        public Int64 Timestamp { get { return (Int64)timestamp_.TotalSeconds; } }

        public NetworkServices Services { get { return services_; } }

        public NetAddress SenderNetAddress { get { return sender_; } }

        public NetAddress ReceiverNetAddress { get { return receiver_; } }

        public byte[] Nonce { get { return nonce_; } }

        public string UserAgent { get { return userAgent_; } }

        public bool Relay { get { return relay_; } }


        public VersionMessage(int version = 70015, NetworkServices services = null, Int64 timestamp = 0,
            NetAddress receiver = null,
            NetAddress sender = null,
            byte[] nonce = null, string userAgent = "/ElementaryBitcoin:0.01/",
            int latestBlock = 0, bool relay = false) {

            version_ = version;

            /*Services*/
            if (services == null) {
                services_ = new NetworkServices();
            } else {
                services_ = services;
            }

            /*Timestamp*/
            if (timestamp == 0) {
                /*calculate unix timestamp*/
                timestamp_ = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));
            } else {
                timestamp_ = TimeSpan.FromSeconds(timestamp);
            }

            /*Network addresses*/
            if (receiver != null) {
                receiver_ = receiver;
            } else {
                receiver_ = new NetAddress();
            }

            if (sender != null) {
                sender_ = sender;
            } else {
                sender_ = new NetAddress();
            }

            /*user agent*/
            userAgent_ = userAgent;

            /*latest block*/
            latestBlock_ = latestBlock;

            /*relay*/
            relay_ = relay;

            /*nonce*/
            if (nonce == null) {
                Random r = new Random();
                int rand1 = r.Next(0, int.MaxValue);
                int rand2 = r.Next(0, int.MaxValue);
                nonce_ = Byte.join(BitConverter.GetBytes(rand1), BitConverter.GetBytes(rand2));
            } else {
                nonce_ = nonce;
            }

            command_ = Encoding.UTF8.GetBytes("version");
            payload_ = this.serialise();
        }

        public override byte[] serialise() {

            byte[] tmp = Byte.join(BitConverter.GetBytes(version_), services_.getServices());
            tmp = Byte.join(tmp, BitConverter.GetBytes((Int64)timestamp_.TotalSeconds));
            tmp = Byte.join(tmp, sender_.serialise(version: true));
            tmp = Byte.join(tmp, receiver_.serialise(version: true));
            tmp = Byte.join(tmp, nonce_);
            tmp = Byte.join(tmp, Helper.encodeVarStr(userAgent_));
            tmp = Byte.join(tmp, BitConverter.GetBytes(latestBlock_));

            byte relayByte = 0x00;
            if (relay_) {
                relayByte = 0x01;
            }
            return Byte.appendByte(tmp, relayByte);
        }

        /// <summary>
        /// Parses the Payload of a version message
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static VersionMessage parse(Stream s) {
            /*Version*/
            byte[] version = new byte[4];
            s.Read(version, 0, 4);
            /*Services*/
            byte[] services = new byte[8];
            s.Read(services, 0, 8);
            /*Timestamp*/
            byte[] timestamp = new byte[8];
            s.Read(timestamp, 0, 8);
            /*receiver address*/
            byte[] receivingAddrBytes = new byte[26];
            s.Read(receivingAddrBytes, 0, 26);
            NetAddress receivingAddr = new NetAddress(receivingAddrBytes, version: true);
            /*sender address*/
            byte[] sourceAddrBytes = new byte[26];
            s.Read(sourceAddrBytes, 0, 26);
            NetAddress sourceAddr = new NetAddress(sourceAddrBytes, version: true);
            /*nonce*/
            byte[] nonce = new byte[8];
            s.Read(nonce, 0, 8);
            /*user agent*/
            int userAgentlength = Helper.getVarIntLength(s);
            byte[] userAgent = new byte[userAgentlength];
            s.Read(userAgent, 0, userAgentlength);
            /*Latest block*/
            byte[] latestBlock = new byte[4];
            s.Read(latestBlock, 0, 4);
            /*relay*/
            byte[] relayByte = new byte[1];
            s.Read(relayByte, 0, 1);
            bool relay = true;
            if (relayByte[0] == 0x00) {
                relay = false;
            }

            return new VersionMessage(
                BitConverter.ToInt32(version),
                new NetworkServices(services),
                BitConverter.ToInt64(timestamp),
                receivingAddr,
                sourceAddr,
                nonce,
                Encoding.ASCII.GetString(userAgent),
                BitConverter.ToInt32(latestBlock),
                relay);
        }

        public override string ToString() {
            string relay = relay_ ? "TRUE" : "FALSE";
            return (
                "Version: " + version_ +
                "\nServices " + services_.ToString() +
                "\nTimestamp " + timestamp_.ToString() +
                "\nSender: \n\t" + sender_.ToString() +
                "\nReceiver: \n\t" + receiver_.ToString() +
                "\nNonce: " + Byte.bytesToString(nonce_) +
                "\nUser Agent: " + userAgent_ +
                "\nHeight: " + latestBlock_ +
                "\nRelay: " + relay
                );
        }
    }
}
