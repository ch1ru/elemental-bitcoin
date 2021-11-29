using System;
using System.Text;
using System.IO;

namespace LBitcoin {
    class TxOut {
        UInt64 amount_;
        const UInt64 MAXCOINS = 2100000000000000;
        Script scriptPubKey_; //change this to script type

        public TxOut(UInt64 amount, Script scriptPubKey) {
            if (amount <= MAXCOINS) {
                this.amount_ = amount;
            } else {
                throw new Exception("Amount too large");
            }
            this.scriptPubKey_ = scriptPubKey;
        }

        public byte[] Serialise() {
            byte[] amount_bytes = Byte.intToLittleEndian(Convert.ToInt64(amount_), 8);
            byte[] result = Byte.join(amount_bytes, this.scriptPubKey_.Serialise()); //change this to Serialise in script class
            return result;
        }

        public static TxOut Parse(Stream s) {
            /********amount********/
            byte[] amount = new byte[8];
            s.Read(amount, 0, 8);
            UInt64 amountLong = BitConverter.ToUInt64(amount);
            /********scriptpubkey********/
            int scriptPubKeyLength = Helper.getVarIntLength(s);
            byte[] length = Helper.encodeVarInt(scriptPubKeyLength);
            byte[] scriptPubKey = new byte[scriptPubKeyLength];
            s.Read(scriptPubKey, 0, scriptPubKeyLength);
            Stream scriptPubKeyStream = new MemoryStream(Byte.join(length, scriptPubKey));
            TxOut output = new TxOut(amountLong, Script.Parse(scriptPubKeyStream));
            return output;
        }

        public void getData() {
            Console.WriteLine("\t\tAmount: {0}", this.amount_);
            Console.WriteLine("\t\tScriptPukKey: {0}", Byte.bytesToString(this.scriptPubKey_.Serialise()));
            Console.Write("\t\tScript type: ");

            if (scriptPubKey_.isP2PKH()) {
                Console.WriteLine("Pay-to-pubkey-hash");
            } 
            else if (scriptPubKey_.isP2SH()) {
                Console.WriteLine("Pay-to-script-hash");
            } 
            else if (scriptPubKey_.isP2wpkh()) {
                Console.WriteLine("Segwit pay-to-pubkey-hash");
            } 
            else if (scriptPubKey_.isP2wsh()) {
                Console.WriteLine("Segwit pay-to-script-hash");
            }
            else if(scriptPubKey_.isOpReturn()) {
                Console.WriteLine("OP Return");
                Console.WriteLine("OP Return Data: {0}", GetOPReturnData());
            }
            else {
                Console.WriteLine("Unknown");
            }
        }

        public UInt64 Amount() {
            return this.amount_;
        }

        public Script ScriptPubKey { get { return scriptPubKey_; } }

        public string GetOPReturnData() {
            if(scriptPubKey_.Commands[0][0] != 0x6a) {
                return "";
            }
            int length = Convert.ToInt32(scriptPubKey_.Commands[1][0]);
            byte[] message = new byte[length];
            for(int i = 2; i < length; i++) {
                message[i - 2] = scriptPubKey_.Commands[2][i];
            }
            Console.WriteLine("op return bytes {0}", Byte.bytesToString(message));
            return Encoding.UTF8.GetString(message);
        }
    }
}
