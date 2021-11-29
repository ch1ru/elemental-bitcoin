using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LBitcoin.Networking.P2P;

namespace LBitcoin.Networking {

    class SimpleNode {
        
        UInt16 port_;
        IPAddress addr_;
        bool logging_;
        Socket sock_ = null;
        List<NetworkEnvelope> receivedPackets_;
        byte[] BUFFER_;

        /*Network parameters*/
        int version_;
        bool testnet_;

        public SimpleNode(IPAddress addr = null, UInt16 port = 0, bool testnet = false, bool logging = false) {

            if(port == 0) {
                if(testnet) {
                    port_ = 18333;
                }
                else {
                    port_ = 8333;
                }
            }
            else {
                if(port > 0 && port <= 65535) {
                    port_ = port;
                }
                else {
                    throw new Exception("Invalid port number");
                }
            }

            if(addr == null) {
                var name = "seed.bitcoinstats.com";
                IPHostEntry host = Dns.GetHostEntry(name);
                var addresses = host.AddressList;
                addr_ = addresses[0];
            }
            else {
                addr_ = addr;
            }

            testnet_ = testnet;
            logging_ = logging;
            BUFFER_ = new byte[1024];
        }

        public bool handshake() {
            VersionMessage versionSender = new VersionMessage();
            VerackMessage verackSender = new VerackMessage();
            VersionMessage versionResponse = null;
            try {
                send(versionSender);
                bool ack = false;
                foreach(NetworkEnvelope envelope in receivedPackets_) {
                    if(envelope.Command == "version") {
                        versionResponse = VersionMessage.parse(new MemoryStream(envelope.Payload));
                    }
                    if(envelope.Command == "verack") {
                        /*Our message has been acknoledged*/
                        ack = true;
                    }
                }
                if(versionResponse == null || !ack) {
                    Console.WriteLine("Did not receive valid response");
                    return false;
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }

            version_ = Math.Min(versionSender.Version, versionResponse.Version);

            return true;
        }

        public async Task send(GenericMessage message) {
            NetworkEnvelope envelope = new NetworkEnvelope(
                message.CommandBytes,
                message.Payload,
                this.testnet_);
            

            try {
                /*create socket*/
                var endpoint = new IPEndPoint(addr_, port_);
                var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(endpoint);
                var networkStream = new NetworkStream(socket, true);

                if (logging_) {
                    Console.WriteLine("Connected to endpoint: {0}", endpoint.ToString());
                    Console.WriteLine("Sending Message:");
                    Console.WriteLine(Byte.bytesToString(envelope.serialise()));
                }

                await SendAsync<Task>(networkStream, envelope).ConfigureAwait(false);
                receivedPackets_ = await ReceiveAsync<NetworkEnvelope>(networkStream).ConfigureAwait(false);
                
                if(logging_) {
                    Console.WriteLine("Received message(s):");
                    foreach(var packet in receivedPackets_) {
                        Console.WriteLine(Byte.bytesToString(packet.serialise()));
                    }
                }
            }
            catch(Exception e) {
                throw new Exception(e.Message);
            }
        }

        static async Task SendAsync<T>(NetworkStream networkStream, NetworkEnvelope message, bool testnet = false) {
            var (header, payload) = Encode<T>(message, testnet);
            await networkStream.WriteAsync(header, 0, header.Length);
            await networkStream.WriteAsync(payload, 0, payload.Length);
        }

        static async Task< List<NetworkEnvelope> > ReceiveAsync<T>(NetworkStream networkStream) {

            List<NetworkEnvelope> packets = new List<NetworkEnvelope>();
            while(networkStream.DataAvailable) {
                var headerBytes = await ReadAsync(networkStream, 24);
                int bodyLength = BitConverter.ToInt32(headerBytes[16..20]);
                byte[] commandBytes = headerBytes[4..16];
                byte[] bodyBytes = await ReadAsync(networkStream, bodyLength);
                packets.Add(Decode<T>(bodyBytes, commandBytes));
            }
            
            return packets;
        }

        static (byte[] header, byte[] payload) Encode<T>(NetworkEnvelope envelope, bool testnet = false) {
            return (envelope.getHeaderBytes(), envelope.Payload);
        }

        static NetworkEnvelope Decode<T>(byte[] payload, byte[] command) {
            NetworkEnvelope networkMsg = new NetworkEnvelope(command, payload);
            return networkMsg;
        }

        static async Task<byte[]> ReadAsync(NetworkStream networkStream, int bytesToRead) {
            var buffer = new byte[bytesToRead];
            var bytesRead = 0;
            while (bytesRead < bytesToRead) {
                var bytesReceived = await networkStream.ReadAsync(buffer, bytesRead, (bytesToRead - bytesRead)).ConfigureAwait(false);
                if (bytesReceived == 0) {
                    throw new Exception("Socket Closed");
                }
                bytesRead += bytesReceived;
            }
            return buffer;
        }
    }
}
