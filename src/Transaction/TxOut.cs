using System;
using System.Text;
using System.IO;

namespace LBitcoin {

    /// <summary>
    /// Transaction output class.
    /// </summary>
    public class TxOut {
        UInt64 amount_;
        const UInt64 MAXCOINS = 2100000000000000;
        Script scriptPubKey_; //change this to script type

        /// <summary>
        /// Constructor. Creates transaction output.
        /// </summary>
        /// <param name="amount">Value of transaction in satoshis.</param>
        /// <param name="scriptPubKey">scriptpubkey <see cref="Script"/></param>
        public TxOut(UInt64 amount, Script scriptPubKey) {
            if (amount <= MAXCOINS) {
                this.amount_ = amount;
            } else {
                throw new Exception("Amount too large");
            }
            this.scriptPubKey_ = scriptPubKey;
        }

        /// <summary>
        /// Serialises the transaction output.
        /// </summary>
        public byte[] Serialise() {
            byte[] amount_bytes = Byte.intToLittleEndian(Convert.ToInt64(amount_), 8);
            byte[] result = Byte.join(amount_bytes, this.scriptPubKey_.Serialise());
            return result;
        }

        /// <summary>
        /// Parses a transaction output from stream.
        /// </summary>
        /// <returns><see cref="TxOut"/> object.</returns>
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

        /// <summary>
        /// Prints transaction output data to console.
        /// </summary>
        public void GetData() {
            Console.WriteLine("\t\tAmount: {0}", this.amount_);
            Console.WriteLine("\t\tScriptPukKey: {0}", Byte.bytesToString(this.scriptPubKey_.Serialise()));
            Console.Write("\t\tScript type: ");

            if (scriptPubKey_.IsP2PKH()) {
                Console.WriteLine("Pay-to-pubkey-hash");
            } 
            else if (scriptPubKey_.IsP2SH()) {
                Console.WriteLine("Pay-to-script-hash");
            } 
            else if (scriptPubKey_.IsP2wpkh()) {
                Console.WriteLine("Segwit pay-to-pubkey-hash");
            } 
            else if (scriptPubKey_.IsP2wsh()) {
                Console.WriteLine("Segwit pay-to-script-hash");
            }
            else if(scriptPubKey_.IsOpReturn()) {
                Console.WriteLine("OP Return");
                Console.WriteLine("OP Return Data: {0}", GetOPReturnData());
            }
            else {
                Console.WriteLine("Unknown");
            }
        }

        /// <summary>
        /// Gets the transaction value of the output.
        /// </summary>
        /// <returns>Value in satoshis.</returns>
        public UInt64 Amount() {
            return this.amount_;
        }

        public Script ScriptPubKey { get { return scriptPubKey_; } }

        /// <summary>
        /// Get the OP_Return data in the script
        /// </summary>
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
