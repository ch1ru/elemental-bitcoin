using System;
using System.Net;

namespace LBitcoin.Networking {

    /// <summary>
    /// Network address class. 
    /// Specification can be found <see href="https://en.bitcoin.it/wiki/Protocol_documentation#Network_address">here</see>. 
    /// </summary>
    class NetAddress {

        protected UInt32 time_; //not present in version message
        private NetworkServices services_;
        protected IPAddress ipAddr_;
        protected Int16 port_;

        public NetAddress(byte[] bytes, bool version = false) {
            if (version) {
                services_ = new NetworkServices(bytes[0..8]);
                ipAddr_ = new IPAddress(bytes[8..24]);
                port_ = BitConverter.ToInt16(bytes[24..26]);
            } else {
                time_ = BitConverter.ToUInt32(bytes[0..4]);
                services_ = new NetworkServices(bytes[4..12]);
                ipAddr_ = new IPAddress(bytes[12..28]);
                port_ = BitConverter.ToInt16(bytes[28..30]);
            }
        }

        public NetAddress(IPAddress ipAddr, Int16 port, NetworkServices services, UInt32 time = 0) {
            time_ = time;
            ipAddr_ = ipAddr;
            port_ = port;
            services_ = services;
        }

        /*Default settings*/
        public NetAddress() {
            time_ = 0;
            byte[] emptyIp = { 0x00, 0x00, 0x00, 0x00 };
            ipAddr_ = new IPAddress(emptyIp);
            port_ = 8333;
            services_ = new NetworkServices();
        }

        public byte[] serialise(bool version = false) {
            byte[] portBytes = BitConverter.GetBytes(port_);
            byte[] result = Byte.join(services_.getServices(), ipAddr_.MapToIPv6().GetAddressBytes());
            result = Byte.join(result, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(port_)));

            if (version == false) {
                byte[] timeBytes = BitConverter.GetBytes(time_);
                result = Byte.join(timeBytes, result);
            }

            return result;
        }

        public override string ToString() {
            string ip = ipAddr_.ToString();
            string services = services_.ToString();
            return (
                "IP Address: " + ip +
                "\nPort: " + port_
                );
        }

        public IPAddress IpAddr { get { return ipAddr_; } }

        public Int16 Port { get { return port_; } }

        public NetworkServices NetworkServices { get { return services_; } }

        public UInt32 Time { get { return time_; } }
    }
}
