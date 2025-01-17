﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Numerics;
using System.Net;
using LBitcoin.Ecc;

namespace LBitcoin {

    /// <summary>
    /// Bitcoin transaction class for defining inputs and outputs and adding spending conditions.
    /// For background info see <see href="https://en.bitcoin.it/wiki/Transaction">here</see>.
    /// </summary>
    public class Transaction {

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

        enum SigHash : uint {
            ALL = 1,
            NONE = 2,
            SINGLE = 3,
            ANYONE_CAN_PAY = 0x80
        }

        /// <summary>
        /// Constructor. Creates standard transaction from version, inputs, outputs and locktime.
        /// </summary>
        public Transaction(int version, TxIn[] inputs, TxOut[] outputs, 
            uint locktime = 0, bool testnet = false, bool segwit = false) {
            version_ = version;
            inputs_ = inputs;
            outputs_ = outputs;
            locktime_ = locktime;
            testnet_ = testnet;
            segwit_ = segwit;
            hash_ = this.CalculateHash();
        }


        /// <summary>
        /// Serialises a transaction.
        /// </summary>
        public byte[] Serialise() {
            return segwit_ ? this.SerialiseSegwit() : this.SerialiseLegacy();
        }

        /// <summary>
        /// Serialises a segwit transaction.
        /// </summary>
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

        /// <summary>
        /// Serialises a legacy transaction.
        /// </summary>
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

        /// <summary>
        /// Parses transaction stream.
        /// </summary>
        /// <returns><see cref="Transaction"/></returns>
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

        /// <summary>
        /// Parses a segwit transaction stream.
        /// </summary>
        /// <returns><see cref="Transaction"/></returns>
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

        /// <summary>
        /// Parses a legacy tx stream.
        /// </summary>
        /// <returns><see cref="Transaction"/></returns>
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

        /// <summary>
        /// Prints data about the transaction to console.
        /// </summary>
        public void GetData() {
            Console.WriteLine("=========================================================");
            Console.WriteLine("Version {0}", version_);
            if (segwit_) Console.WriteLine("Segwit: true");
                else Console.WriteLine("Segwit: false");
            Console.WriteLine("Number of inputs: {0}", inputs_.Length);
            for (int i = 0; i < inputs_.Length; i++) {
                Console.WriteLine("\tInput {0}", i);
                inputs_[i].GetData();
            }
            Console.WriteLine();
            Console.WriteLine("Number of outputs: {0}", outputs_.Length);
            for (int i = 0; i < outputs_.Length; i++) {
                Console.WriteLine("\tOutput {0}", i);
                outputs_[i].GetData();
            }
            Console.WriteLine("Locktime is {0}", locktime_);
            Console.WriteLine("Transaction fee {0}", this.Fee(testnet: testnet_));
            Console.WriteLine("=========================================================");
        }

        /// <summary>
        /// Returns the transaction hash.
        /// </summary>
        byte[] CalculateHash() {
            byte[] SerialisedTx = this.SerialiseLegacy();
            byte[] sha256Digest = Hash.hash256(SerialisedTx);
            Array.Reverse(sha256Digest, 0, sha256Digest.Length);
            return sha256Digest;
        }

        /// <summary>
        /// Calculates the network fee.
        /// </summary>
        /// <returns>Fee in satoshis.</returns>
        public UInt64 Fee(bool testnet = false) {
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

        /// <summary>
        /// Calculates hash prevouts as part of <see href="https://github.com/bitcoin/bips/blob/master/bip-0143.mediawiki">bip143</see>.
        /// </summary>
        byte[] HashPrevouts() {
            if(hashPrevouts_ == null) {
                byte[] allPrevouts = null;
                byte[] allSequence = null;
                foreach(TxIn input in inputs_) {
                    byte[] littleEndianTxid = new byte[32];
                    Array.Copy(input.GetPrevTxid(), littleEndianTxid, 32);
                    Array.Reverse(littleEndianTxid);
                    allPrevouts = Byte.join(allPrevouts, Byte.join(littleEndianTxid, 
                        BitConverter.GetBytes(input.GetPrevIndex())));
                    allSequence = Byte.join(allSequence, BitConverter.GetBytes(input.GetSequence()));
                }
                hashPrevouts_ = Hash.hash256(allPrevouts);
                hashSequence_ = Hash.hash256(allSequence);
            }
            return hashPrevouts_;
        }

        /// <summary>
        /// Calculates hash outputs as part of <see href="https://github.com/bitcoin/bips/blob/master/bip-0143.mediawiki">bip143</see>.
        /// </summary>
        public byte[] HashOutputs() {
            if (hashOutputs_ == null) {
                byte[] allOutputs = null;
                foreach (TxOut output in outputs_) {
                    allOutputs = Byte.join(allOutputs, output.Serialise());
                }
                hashOutputs_ = Hash.hash256(allOutputs);
            }
            return hashOutputs_;
        }

        /// <summary>
        /// Calculates hash sequence as part of <see href="https://github.com/bitcoin/bips/blob/master/bip-0143.mediawiki">bip143</see>.
        /// </summary>
        byte[] HashSequence() {
            if(hashSequence_ == null) {
                HashPrevouts(); //hash sequence takes place here
            }
            return hashSequence_;
        }

        /// <summary>
        /// Calculates the message to sign (z) which is the hash of the transaction. 
        /// Legacy only.
        /// </summary>
        /// <returns>Message as <see cref="BigInteger"/>.</returns>
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
                        scriptSig = this.TxIns[i].ScriptPubKey(testnet: testnet_);
                    }
                } else {
                    scriptSig = null;
                }
                bytes = Byte.join(bytes, new TxIn(
                    this.TxIns[i].GetPrevTxid(),
                    this.TxIns[i].GetPrevIndex(),
                    scriptSig,
                    this.TxIns[i].GetSequence())
                    .Serialise());
            }
            bytes = Byte.join(bytes, Byte.encodeVarInt(outputs_.Length));

            foreach (TxOut txOut in outputs_) {
                bytes = Byte.join(bytes, txOut.Serialise());
            }
            bytes = Byte.join(bytes, BitConverter.GetBytes(this.locktime_));
            bytes = Byte.join(bytes, BitConverter.GetBytes(((uint)SigHash.ALL)));
            byte[] sha256Digest = Hash.hash256(bytes);
            BigInteger z = new BigInteger(sha256Digest, true, true);
            return z;
        }

        /// <summary>
        /// Calculates the message to sign (z) which is the hash of the transaction. 
        /// Uses bip143 (segwit).
        /// </summary>
        /// <returns>Message as <see cref="BigInteger"/></returns>
        public BigInteger sigHashBip143(int inputIndex, Script redeemScript = null, Script witnessScript = null) {
            TxIn txIn = inputs_[inputIndex];
            /*per bip143 spec*/
            byte[] s = BitConverter.GetBytes(version_);
            s = Byte.join(s, this.HashPrevouts());
            s = Byte.join(s, this.HashSequence());
            s = Byte.join(s, Byte.ConvertEndianess(txIn.GetPrevTxid()));
            s = Byte.join(s, BitConverter.GetBytes(txIn.GetPrevIndex()));
            byte[] scriptCode;

            if(witnessScript != null) {
                scriptCode = witnessScript.Serialise();
            }
            else if(redeemScript != null) {
                scriptCode = Script.P2PKH(redeemScript.Commands[1]).Serialise();
            }
            else {
                scriptCode = Script.P2PKH(txIn.ScriptPubKey(testnet: testnet_).Commands[1]).Serialise();
            }

            s = Byte.join(s, scriptCode);
            s = Byte.join(s, Byte.intToLittleEndian((long)txIn.Amount(testnet: testnet_), 8));
            s = Byte.join(s, BitConverter.GetBytes(txIn.GetSequence()));
            s = Byte.join(s, this.HashOutputs());
            s = Byte.join(s, BitConverter.GetBytes(locktime_));
            s = Byte.join(s, BitConverter.GetBytes(((uint)SigHash.ALL)));
            return new BigInteger(Hash.hash256(s), true, true);
        }

        /// <summary>
        /// Sign an input.
        /// </summary>
        /// <param name="inputIndex">The transaction index to sign.</param>
        /// <param name="key">A <see cref="PrivateKey"/></param>
        /// <returns></returns>
        public bool SignInput(int inputIndex, PrivateKey key) {
            BigInteger z = this.sigHash(inputIndex); //message to sign
            byte[] der = key.sign(z).DerEncode();
            byte[] sig = Byte.appendByte(der, Convert.ToByte(((uint)SigHash.ALL)));
            byte[] sec = key.pubKey().Compressed;
            Script scriptSig = new Script();
            scriptSig.Add(sig);
            scriptSig.Add(sec);
            inputs_[inputIndex].scriptSig_ = scriptSig;
            return this.VerifyInput(inputIndex);
        }

        /// <summary>
        /// Verify a particular transaction input.
        /// <param name="inputIndex">The transaction index.</param>
        /// </summary>
        public bool VerifyInput(int inputIndex) {
            TxIn txIn = inputs_[inputIndex];
            Script scriptSig = txIn.GetScriptSig();
            Script scriptPubKey = txIn.ScriptPubKey(testnet: testnet_);
            Script redeemScript = null;
            List<byte[]> witness = new List<byte[]>();
            BigInteger z; //transaction hash to sign

            if (scriptPubKey.IsP2SH()) {
                byte[] cmd = txIn.scriptSig_.Commands[txIn.scriptSig_.Commands.Count - 1];
                byte[] rawRedeem = Byte.join(Byte.encodeVarInt(cmd.Length), cmd);
                Stream stream = new MemoryStream(rawRedeem);
                redeemScript = Script.Parse(stream);

                if(redeemScript.IsP2wpkh()) { //p2pkh-p2wsh
                    z = sigHashBip143(inputIndex, redeemScript);
                    witness = txIn.witness_;
                }
                else if(redeemScript.IsP2wsh()) { //p2sh-p2wsh
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
                if(scriptPubKey.IsP2wpkh()) {
                    z = sigHashBip143(inputIndex);
                    witness = txIn.witness_;
                }
                else { //p2pkh
                    z = sigHash(inputIndex, redeemScript);
                    witness = null;
                }
            }
            
            Script combined = scriptSig + scriptPubKey;
            return combined.Evaluate(z, witness);
        }

        /// <summary>
        /// Verify all transaction inputs.
        /// </summary>
        /// <returns></returns>
        public bool Verify() {
            /*Check we're not printing money*/
            if(Fee(testnet: testnet_) < 0) {
                return false;
            }
            /*Check each input has a valid scriptsig*/
            for(int i = 0; i < TxIns.Length; i++) {
                if(!this.VerifyInput(i)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds OP_Return message.
        /// </summary>
        /// <param name="message">Message you want to add</param>
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

        /// <summary>
        /// Whether the transaction is the coinbase.
        /// </summary>
        /// <returns></returns>
        public bool IsCoinbase() {
            /*1 input, txid all 0x00s and sequence ffffffff*/
            if (inputs_.Length > 1 || inputs_[0].GetSequence() != 0xffffffff) {
                return false;
            }
            foreach(byte b in inputs_[0].GetPrevTxid()) {
                if(b != 0x00) {
                    return false;
                }
            }
            return true;
        }

        public int CoinbaseHeight() {
            if(this.IsCoinbase() == false) {
                return 0;
            }
            byte[] height = inputs_[0].scriptSig_.Commands[0];
            while (height.Length < 4) height = Byte.appendByte(height, 0x00); //add padding
            return BitConverter.ToInt32(height);
        }

        public byte[] GetHash() { return hash_; }

        public string ID { get { return Byte.bytesToString(this.hash_); } }

        public int Version { get { return this.version_; } }

        public TxIn[] TxIns { get { return this.inputs_; } }

        public TxOut[] TxOuts { get { return this.outputs_; } }

        public uint Locktime { get { return this.locktime_; } }

        public bool IsTestnet() {
            return testnet_;
        }
    }


    public class TxFetcher {

        string[] cache = new string[] { };

        static string GetURL(string txid, bool testnet = false) {
            return testnet ?
                "https://blockstream.info/testnet/api/tx/" + txid + "/hex" :
                "https://blockstream.info/api/tx/" + txid + "/hex";
        }

        /// <summary>
        /// Fetches a transaction from txid.
        /// </summary>
        /// <param name="txid">Txid in big endian format</param>
        public static Transaction Fetch(string txid, bool testnet = false, bool fresh = true) {
            if (fresh) { //or txid not in cache
                byte[] rawTx = new byte[] { };
                string url = GetURL(txid, testnet: testnet);
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
