using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Numerics;
using System.Net;

namespace LBitcoin {
    class Transaction {

        int version_;
        public TxIn[] inputs_;
        public TxOut[] outputs_;
        uint locktime_;
        bool testnet_;
        bool segwit_;

        byte[] hash_ ;
        byte[] hashOutputs_;
        byte[] hashPrevouts_;
        byte[] hashSequence_;

        public static int SIGHASH_ALL = 1;
        public static int SIGHASH_NONE = 2;
        public static int SIGHASH_SINGLE = 3;

        public Transaction(int version, TxIn[] inputs, TxOut[] outputs, 
            uint locktime = 0, bool testnet = false, bool segwit = false) {
            version_ = version;
            inputs_ = inputs;
            outputs_ = outputs;
            locktime_ = locktime;
            testnet_ = testnet;
            segwit_ = segwit;
            hash_ = this.hash();
        }


        /*Serialises a transaction*/
        public byte[] Serialise() {
            return segwit_ ? this.SerialiseSegwit() : this.SerialiseLegacy();
        }

        /*Serialises a segwit transaction*/
        public byte[] SerialiseSegwit() {
            /*version*/
            byte[] versionBytes = new byte[] { Convert.ToByte(version_) };
            byte[] marker = new byte[] { 0x00, 0x01 };
            byte[] result = Byte.join(versionBytes, marker);
            result = Byte.join(result, Helper.encodeVarInt(inputs_.Length));
            foreach(TxIn input in inputs_) {
                result = Byte.join(result, input.Serialise());
            }
            result = Byte.join(result, Helper.encodeVarInt(outputs_.Length));
            foreach (TxOut output in outputs_) {
                result = Byte.join(result, output.Serialise());
            }
            foreach(TxIn input in inputs_) {
                byte[] numOfItems = new byte[] { Convert.ToByte(input.witness_.Count) };
                result = Byte.join(result, numOfItems);
                foreach(byte[] item in input.witness_) {
                    byte[] length = Byte.encodeVarInt(item.Length);
                    result = Byte.join(result, length);
                    result = Byte.join(result, item);
                }
            }
            result = Byte.join(result, BitConverter.GetBytes(locktime_));
            return result;
        }

        /*Serialises a legacy transaction*/
        public byte[] SerialiseLegacy() {
            byte[] numOfInputs = Byte.encodeVarInt(inputs_.Length);
            byte[] version_bytes = Byte.intToLittleEndian(version_);
            byte[] result = Byte.join(version_bytes, numOfInputs);
            foreach (TxIn input in inputs_) {
                result = Byte.join(result, input.Serialise());
            }
            byte[] numOfOutputs = Byte.encodeVarInt(outputs_.Length);
            result = Byte.join(result, numOfOutputs);
            foreach (TxOut output in outputs_) {
                result = Byte.join(result, output.Serialise());
            }
            uint locktime = locktime_;
            result = Byte.join(result, BitConverter.GetBytes(locktime));
            return result;
        }

        public static Transaction Parse(Stream s, bool testnet = false) {
            byte[] tmp = new byte[4];
            s.Read(tmp, 0, 4); //peel the first 4 version bytes
            byte[] segwit = new byte[1];
            s.Read(segwit, 0, 1);
            s.Seek(-5, SeekOrigin.Current);
            if(segwit[0] == 0x00) {
                return ParseSegwit(s, testnet: testnet);
            }
            else {
                return ParseLegacy(s, testnet: testnet);
            }
        }

        /*Parses a segwit tx stream*/
        public static Transaction ParseSegwit(Stream s, bool testnet = false) {
            /*Version*/
            byte[] versionBytes = new byte[4];
            s.Read(versionBytes, 0, 4);
            int version = BitConverter.ToInt32(versionBytes);
            /*check segwit*/
            byte[] segwitMarker = new byte[2];
            s.Read(segwitMarker, 0, 2);
            if(segwitMarker[0] != 0x00 || segwitMarker[1] != 0x01) {
                throw new Exception("Not a segwit transaction");
            }
            /*Inputs*/
            int numOfInputs = Helper.getVarIntLength(s);
            TxIn[] txins = new TxIn[numOfInputs];
            for (int i = 0; i < numOfInputs; i++) {
                txins[i] = TxIn.Parse(s); //pass the correct bytes
            }
            /*Outputs*/
            int numOfOutputs = Helper.getVarIntLength(s);
            TxOut[] txouts = new TxOut[numOfOutputs];
            for (int i = 0; i < numOfOutputs; i++) {
                txouts[i] = TxOut.Parse(s); //pass the correct bytes
            }
            /*witness*/
            foreach(TxIn txin in txins) {
                int numOfItems = Helper.getVarIntLength(s);
                List<byte[]> items = new List<byte[]>();
                for(int i = 0; i < numOfItems; i++) {
                    int itemLength = Helper.getVarIntLength(s);
                    if (itemLength == 0) {
                        items.Add(new byte[] { 0x00 });
                    }
                    else {
                        byte[] newItem = new byte[itemLength];
                        s.Read(newItem, 0, itemLength);
                        items.Add(newItem);
                    }
                }
                txin.witness_ = items;
            }
            /*locktime*/
            byte[] locktimeBytes = new byte[4];
            s.Read(locktimeBytes, 0, 4);
            uint locktime = BitConverter.ToUInt32(locktimeBytes);
            return new Transaction(
                version, txins, txouts, locktime, testnet: testnet, segwit: true);
        }

        /*Parses a legacy tx stream*/
        public static Transaction ParseLegacy(Stream s, bool testnet = false) {
            /*Version*/
            byte[] version_bytes = new byte[4];
            s.Read(version_bytes, 0, 4);
            int version = BitConverter.ToInt32(version_bytes);
            /*Inputs*/
            int numOfInputs = Helper.getVarIntLength(s);
            TxIn[] txins = new TxIn[numOfInputs];
            for (int i = 0; i < numOfInputs; i++) {
                txins[i] = TxIn.Parse(s); //pass the correct bytes
            }
            /*Outputs*/
            int numOfOutputs = Helper.getVarIntLength(s);
            TxOut[] txouts = new TxOut[numOfOutputs];
            for (int i = 0; i < numOfOutputs; i++) {
                txouts[i] = TxOut.Parse(s); //pass the correct bytes
            }
            /*locktime*/
            byte[] locktimeBytes = new byte[4];
            s.Read(locktimeBytes, 0, 4);
            uint locktime = BitConverter.ToUInt32(locktimeBytes);
            Transaction tx = new Transaction(
                version, txins, txouts, locktime, testnet: testnet, segwit: false);
            return tx;
        }

        public override string ToString() {
            return Byte.bytesToString(this.Serialise());
        }

        public void getData() {
            Console.WriteLine("=========================================================");
            Console.WriteLine("Version {0}", version_);
            if (segwit_) Console.WriteLine("Segwit: true");
                else Console.WriteLine("Segwit: false");
            Console.WriteLine("Number of inputs: {0}", inputs_.Length);
            for (int i = 0; i < inputs_.Length; i++) {
                Console.WriteLine("\tInput {0}", i);
                inputs_[i].getData();
            }
            Console.WriteLine();
            Console.WriteLine("Number of outputs: {0}", outputs_.Length);
            for (int i = 0; i < outputs_.Length; i++) {
                Console.WriteLine("\tOutput {0}", i);
                outputs_[i].getData();
            }
            Console.WriteLine("Locktime is {0}", locktime_);
            Console.WriteLine("Transaction fee {0}", this.fee());
            Console.WriteLine("=========================================================");
        }

        /*Returns the transaction hash*/
        byte[] hash() {
            byte[] SerialisedTx = this.SerialiseLegacy();
            byte[] sha256Digest = Hash.hash256(SerialisedTx);
            Array.Reverse(sha256Digest, 0, sha256Digest.Length);
            return sha256Digest;
        }

        public byte[] getHash() {
             return hash_; 
        }

        public string ID {
            get { return Byte.bytesToString(this.hash_); }
        }

        public int Version {
            get { return this.version_; }
        }

        public TxIn[] TxIns {
            get { return this.inputs_; }
        }

        public TxOut[] TxOuts {
            get { return this.outputs_; }
        }

        public uint Locktime {
            get { return this.locktime_; }
        }

        public bool isTestnet() {
            return testnet_;
        }

        public UInt64 fee(bool testnet = false) {
            UInt64 inputFeesTotal = 0, outputFeesTotal = 0;
            foreach(TxIn input in this.inputs_) {
                inputFeesTotal += input.Amount(testnet);
            }
            foreach(TxOut output in this.outputs_) {
                outputFeesTotal += output.Amount();
            }
            UInt64 fee = inputFeesTotal - outputFeesTotal;
            return fee;
        }

        

        byte[] hashPrevouts() {
            if(hashPrevouts_ == null) {
                byte[] allPrevouts = null;
                byte[] allSequence = null;
                foreach(TxIn input in inputs_) {
                    allPrevouts = Byte.join(allPrevouts, Byte.join(input.getPrevTxid(), 
                        BitConverter.GetBytes(input.getPrevIndex())));
                    allSequence = Byte.join(allSequence, BitConverter.GetBytes(input.getSequence()));
                }
                hashPrevouts_ = Hash.hash256(allPrevouts);
                hashSequence_ = Hash.hash256(allSequence);
            }
            return hashPrevouts_;
        }

        byte[] hashOutputs() {
            if (hashOutputs_ == null) {
                byte[] allOutputs = null;
                foreach (TxOut output in outputs_) {
                    allOutputs = Byte.join(allOutputs, output.Serialise());
                }
                hashOutputs_ = Hash.hash256(allOutputs);
            }
            return hashOutputs_;
        }

        byte[] hashSequence() {
            if(hashSequence_ == null) {
                hashPrevouts();
            }
            return hashSequence_;
        }

        public BigInteger sigHash(int inputIndex, Script redeemScript = null) {
            byte[] bytes = BitConverter.GetBytes(this.version_);
            bytes = Byte.join(bytes, Byte.encodeVarInt(inputs_.Length));
            for (int i = 0; i < this.inputs_.Length; i++) {
                Script scriptSig;
                if (i == inputIndex) {
                    if (redeemScript != null) {
                        scriptSig = redeemScript;
                    } else {
                        /*replace scriptsig with scriptpubkey*/
                        scriptSig = this.TxIns[i].scriptPubKey(testnet: testnet_);
                    }
                } else {
                    scriptSig = null;
                }
                bytes = Byte.join(bytes, new TxIn(
                    this.TxIns[i].getPrevTxid(),
                    this.TxIns[i].getPrevIndex(),
                    scriptSig,
                    this.TxIns[i].getSequence())
                    .Serialise());
            }
            bytes = Byte.join(bytes, Byte.encodeVarInt(outputs_.Length));

            foreach (TxOut txOut in outputs_) {
                bytes = Byte.join(bytes, txOut.Serialise());
            }
            bytes = Byte.join(bytes, BitConverter.GetBytes(this.locktime_));
            bytes = Byte.join(bytes, BitConverter.GetBytes(SIGHASH_ALL));
            byte[] sha256Digest = Hash.hash256(bytes);
            BigInteger z = new BigInteger(sha256Digest, true, true);
            return z;
        }

        public BigInteger sigHashBip143(int inputIndex, Script redeemScript = null, Script witnessScript = null) {
            TxIn txIn = inputs_[inputIndex];
            /*per bip143 spec*/
            byte[] s = BitConverter.GetBytes(version_);
            s = Byte.join(s, this.hashPrevouts());
            s = Byte.join(s, this.hashSequence());
            s = Byte.join(s, txIn.getPrevTxid());
            s = Byte.join(s, BitConverter.GetBytes(txIn.getPrevIndex()));
            byte[] scriptCode;

            if(witnessScript != null) {
                scriptCode = witnessScript.Serialise();
            }
            else if(redeemScript != null) {
                scriptCode = Script.p2pkh(redeemScript.Commands[1]).Serialise();
            }
            else {
                scriptCode = Script.p2pkh(txIn.scriptPubKey(testnet: testnet_).Commands[1]).Serialise();
            }

            s = Byte.join(s, scriptCode);
            s = Byte.join(s, Byte.intToLittleEndian((long)txIn.Amount(testnet: testnet_), 8));
            s = Byte.join(s, BitConverter.GetBytes(txIn.getSequence()));
            s = Byte.join(s, this.hashOutputs());
            s = Byte.join(s, BitConverter.GetBytes(locktime_));
            s = Byte.join(s, BitConverter.GetBytes(SIGHASH_ALL));
            return new BigInteger(Hash.hash256(s), true, true);
        }

        public bool signInput(int inputIndex, PrivateKey key) {
            BigInteger z = this.sigHash(inputIndex); //message to sign
            byte[] der = key.sign(z).DerEncode();
            byte[] sig = Byte.appendByte(der, Convert.ToByte(SIGHASH_ALL));
            byte[] sec = key.pubKey().Compressed;
            Script scriptSig = new Script();
            scriptSig.Add(sig);
            scriptSig.Add(sec);
            inputs_[inputIndex].scriptSig_ = scriptSig;
            return this.verifyInput(inputIndex);
        }

        public bool verifyInput(int inputIndex) {
            TxIn txIn = inputs_[inputIndex];
            Script scriptSig = txIn.getScriptSig();
            Script scriptPubKey = txIn.scriptPubKey(testnet: testnet_);
            Script redeemScript = null;
            List<byte[]> witness = new List<byte[]>();
            BigInteger z; //transaction hash to sign

            if (scriptPubKey.isP2SH()) {
                byte[] cmd = txIn.scriptSig_.Commands[txIn.scriptSig_.Commands.Count - 1];
                byte[] rawRedeem = Byte.join(Byte.encodeVarInt(cmd.Length), cmd);
                Stream stream = new MemoryStream(rawRedeem);
                redeemScript = Script.Parse(stream);

                if(redeemScript.isP2wpkh()) { //p2pkh-p2wsh
                    z = sigHashBip143(inputIndex, redeemScript);
                    witness = txIn.witness_;
                }
                else if(redeemScript.isP2wsh()) { //p2sh-p2wsh
                    cmd = txIn.witness_[txIn.witness_.Count - 1];
                    byte[] rawWitness = Byte.join(Helper.encodeVarInt(cmd.Length), cmd);
                    Stream s = new MemoryStream(rawWitness);
                    Script witnessScript = Script.Parse(s);
                    z = this.sigHashBip143(inputIndex, witnessScript);
                    witness = txIn.witness_;
                }
                else { //p2sh
                    z = sigHash(inputIndex, redeemScript);
                    witness = null;
                }
            }
            else {
                if(scriptPubKey.isP2wpkh()) {
                    z = sigHashBip143(inputIndex);
                    witness = txIn.witness_;
                }
                else { //p2pkh
                    z = sigHash(inputIndex, redeemScript);
                    witness = null;
                }
            }
            
            Script combined = scriptSig + scriptPubKey;
            return combined.evaluate(z, witness);
        }

        public bool verify() {
            /*Check we're not printing money*/
            if(fee(testnet: testnet_) < 0) {
                return false;
            }
            /*Check each input has a valid scriptsig*/
            for(int i = 0; i < TxIns.Length; i++) {
                if(!this.verifyInput(i)) {
                    return false;
                }
            }
            return true;
        }

        public void AddMessage(string message) {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            if (messageBytes.Length > 80) throw new Exception("Message is longer than 80 bytes max");
            messageBytes = Byte.prependByte(messageBytes, Convert.ToByte(messageBytes.Length));
            messageBytes = Byte.prependByte(messageBytes, 0x6a);
            Stream s = new MemoryStream(messageBytes);
            TxOut opReturnOut = new TxOut(0, Script.Parse(s));
            TxOut[] newOutputs = new TxOut[outputs_.Length + 1];
            newOutputs[0] = opReturnOut;
            Array.Copy(outputs_, 0, newOutputs, 1, outputs_.Length);
        }

        public bool isCoinbase() {
            /*1 input, txid all 0x00s and sequence ffffffff*/
            if (inputs_.Length > 1 || inputs_[0].getSequence() != 0xffffffff) {
                return false;
            }
            foreach(byte b in inputs_[0].getPrevTxid()) {
                if(b != 0x00) {
                    return false;
                }
            }
            return true;
        }

        public int coinbaseHeight() {
            if(this.isCoinbase() == false) {
                return 0;
            }
            byte[] height = inputs_[0].scriptSig_.Commands[0];
            while (height.Length < 4) height = Byte.appendByte(height, 0x00); //add padding
            return BitConverter.ToInt32(height);
        }
    }


    class txFetcher {

        string[] cache = new string[] { };

        static string getURL(string txid, bool testnet = false) {
            return testnet ?
                "https://blockstream.info/testnet/api/tx/" + txid + "/hex" :
                "https://blockstream.info/api/tx/" + txid + "/hex";
        }

        /*Returns a transaction from txid. txid must be in big endian format*/
        public static Transaction fetch(string txid, bool testnet = false, bool fresh = true) {
            if (fresh) { //or txid not in cache
                byte[] rawTx = new byte[] { };
                string url = getURL(txid, testnet: testnet);
                var response = new WebClient().DownloadString(url);
                BigInteger rawTxInt = BigInteger.Parse(response, System.Globalization.NumberStyles.HexNumber);
                rawTx = rawTxInt.ToByteArray(true, true);

                //Parse raw data into a transaction
                Stream txStream = new MemoryStream(rawTx);
                Transaction tx = Transaction.Parse(txStream, testnet: testnet);
                //verify we have the right data
                if(tx.ID != txid) {
                    //throw new Exception("Transaction hash does not match");
                }
                //add to cache
                return tx;
            }
            return null; //return value of txid
        }
    }
}
