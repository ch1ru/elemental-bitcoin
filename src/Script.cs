using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using Bech32_Csharp;
using LBitcoin.Ecc;

namespace LBitcoin {

    /// <summary>
    /// Script-based operations such as performing evaluations and locking/unlocking scripts. 
    /// </summary>
    class Script {

        List<byte[]> cmds_ = new List<byte[]>();
        Stack<byte[]> stack_ = new Stack<byte[]>();

        public List<byte[]> Commands { get { return cmds_; } }

        public static bool isCommand(byte[] testCmd) {
            if (testCmd.Length == 1 && (testCmd[0] > 0x4d || testCmd[0] == 0x00)) {
                return true;
            }
            return false;
        }

        public static bool isData(byte[] testData) {
            return !isCommand(testData);
        }

        public void printStack() {
            foreach (byte[] element in stack_) {
                Console.WriteLine(Byte.bytesToString(element));
            }
        }

        public override string ToString() {
            string result = "";
            foreach (byte[] cmd in cmds_) {
                if (isCommand(cmd)) {
                    /*get command name*/
                    try {
                        var opcode = (opcodes)cmd[0];
                        result += opcode.ToString() + "\n";
                    } catch (Exception e) { //cmd not in dictionary
                        throw new Exception("Could not find command name");
                    }
                } else {
                    result += Byte.bytesToString(cmd) + "\n";
                }
            }
            return result;
        }

        /// <summary>
        /// Add data to the script.
        /// </summary>
        /// <param name="data">Opcode or operand in bytes.</param>
        public void Add(byte[] data) {
            cmds_.Add(data);
        }

        /// <summary>
        /// Add single byte to the script.
        /// </summary>
        /// <param name="data">Opcode or operand as byte.</param>
        public void Add(byte data) {
            byte[] dataArray = new byte[1];
            dataArray[0] = data;
            cmds_.Add(dataArray);
        }

        /// <summary>
        /// Add data to the script.
        /// </summary>
        /// <param name="data">Opcode or operand in <see cref="opcodes"/>.</param>
        public void Add(opcodes opcode) {
            Add((byte)opcode);
        }


        /// <summary>
        /// Creates a locking script for pay-to-pubkeyhash.
        /// </summary>
        public static Script p2pkh(byte[] h160) {
            Script scriptPubKey = new Script();
            scriptPubKey.Add(opcodes.OP_DUP);
            scriptPubKey.Add(opcodes.OP_HASH160);
            scriptPubKey.Add(h160);
            scriptPubKey.Add(opcodes.OP_EQUALVERIFY);
            scriptPubKey.Add(opcodes.OP_CHECKSIG);
            return scriptPubKey;
        }

        /// <summary>
        /// Creates a locking script for pay-to-segwig-pubkeyhash.
        /// </summary>
        public static Script p2wpkh(byte[] h160) {
            Script script = new Script();
            script.Add(opcodes.OP_0);
            script.Add(h160);
            return script;
        }

        /// <summary>
        /// Creates a locking script for segwit pay-to-script-hash
        /// </summary>
        /// <param name="h256">The hash of the redeem script.</param>
        /// <returns></returns>
        public static Script p2wsh(byte[] h256) {
            Script scriptPubKey = new Script();
            scriptPubKey.Add(opcodes.OP_0);
            scriptPubKey.Add(h256);
            return scriptPubKey;
        }

        /// <summary>
        /// Creates a locking script for segwit pay-to-script-hash
        /// </summary>
        /// <param name="script">The redeem script as <see cref="Script"/>.</param>
        /// <returns></returns>
        public static Script p2wsh(Script script) {
            byte[] scriptBytes = new byte[] { };
            foreach (byte[] cmd in script.cmds_) {
                scriptBytes = Byte.join(scriptBytes, cmd);
            }
            return p2wsh(Hash.sha256(scriptBytes));
        }

        /// <summary>
        /// Creates a locking script for pay-to-script-hash.
        /// </summary>
        /// <param name="scriptHash">Script in bytes.</param>
        public static Script p2sh(byte[] scriptHash) {
            Script scriptPubKey = new Script();
            scriptPubKey.Add(0xa9); //hash160
            scriptPubKey.Add(scriptHash);
            scriptPubKey.Add(0x87); //equal
            return scriptPubKey;
        }

        /// <summary>
        /// Creates a locking script for pay-to-script-hash.
        /// </summary>
        /// <param name="script">Script as <see cref="Script"/></param>
        public static Script p2sh(Script script) {
            byte[] scriptBytes = new byte[] { };
            foreach (byte[] cmd in script.cmds_) {
                scriptBytes = Byte.join(scriptBytes, cmd);
            }
            return p2sh(Hash.hash160(scriptBytes));
        }


        /// <summary>
        /// (Deprecated) Creates a locking script for pay-to-pubkey.
        /// </summary>
        public static Script p2pk(byte[] sec) {
            byte[] checkSig = new byte[1];
            checkSig[0] = 0xac;
            Script scriptPubKey = new Script();
            scriptPubKey.Add(sec);
            scriptPubKey.Add(checkSig);
            return scriptPubKey;
        }

        /// <summary>
        /// Creates m of n multisig redeem script.
        /// </summary>
        /// <param name="n">Total number of keys.</param>
        /// <param name="m">Minimum number of keys needed to sign.</param>
        /// <param name="pubs">Public keys.</param>
        public static Script createMultisigRedeemScript(int n, int m, PublicKey[] pubs) {
            Script multisigRedeemScript = new Script();

            multisigRedeemScript.Add(Op.encodeNum(n));
            foreach (PublicKey pub in pubs) {
                byte length = Convert.ToByte(pub.Compressed.Length);
                byte[] bytes = Byte.prependByte(pub.Compressed, length);
                multisigRedeemScript.Add(bytes);
            }
            multisigRedeemScript.Add(Op.encodeNum(m));
            return multisigRedeemScript;
        }

        /// <summary>
        /// Gets the corresponding bitcoin address from the current object script.
        /// </summary>
        public BitcoinAddress address(bool testnet = false) {

            if (this.isP2PKH()) {
                byte[] h160 = cmds_[2];
                string addr = BitcoinAddress.encodeAddressLegacy(h160, testnet: testnet);
                return new BitcoinAddress(addr);
            } else if (this.isP2SH()) {
                byte[] h160 = cmds_[1];
                string addr = BitcoinAddress.encodeAddressLegacy(h160, isP2PKH: false, testnet: testnet);
                return new BitcoinAddress(addr);
            } else if (this.isP2wpkh()) {
                byte[] h160 = cmds_[1];
                string addr = Converter.EncodeBech32(0x00, h160, isP2PKH: true, mainnet: !testnet);
                return new BitcoinAddress(addr);
            } else if (this.isP2wsh()) {
                byte[] s256 = cmds_[1];
                string addr = Converter.EncodeBech32(0x00, s256, isP2PKH: false, mainnet: !testnet);
                return new BitcoinAddress(addr);
            } else if (this.isP2PK()) { //deprecated
                byte[] pubKey = cmds_[1];
                string addr = BitcoinAddress.encodeAddressLegacy(pubKey, testnet: testnet);
                return new BitcoinAddress(addr);
            }
            throw new Exception("Unknown scriptpubkey");
        }

        public static Script operator +(Script first, Script second) {
            Script combined = new Script();
            /*Scripts can be null*/
            if (first.cmds_ == null) {
                return second;
            }
            if (second.cmds_ == null) {
                return first;
            }
            List<byte[]> firstCmds = first.cmds_;
            List<byte[]> secondCmds = second.cmds_;
            foreach (byte[] array in firstCmds) {
                combined.Add(array);
            }
            foreach (byte[] array in secondCmds) {
                combined.Add(array);
            }
            return combined;
        }

        /// <summary>
        /// Evaluates a script to find if a utxo can be redeemed by the provided signature or script.
        /// </summary>
        /// <param name="z">BigInteger message which denotes the hash of a transaction.</param>
        /// <param name="witness">Witness script for use in segwit witness program.</param>
        /// <returns><see cref="bool"/> value the script has evaluated to.</returns>
        public bool evaluate(BigInteger z, List<byte[]> witness) {
            BitcoinStack stack = new BitcoinStack(stack_);
            Stack<byte[]> altstack = new Stack<byte[]>();
            while (cmds_.Count() > 0) {
                byte[] cmd = cmds_[0];
                cmds_.RemoveAt(0);
                try {
                    if (cmd.Length == 1 && (cmd[0] > 0x4b) || (cmd[0] == 0x00 && cmd.Length == 1)) { //command

                        if (cmd[0] == 0xac || cmd[0] == 0xad || cmd[0] == 0xae || cmd[0] == 0xaf) {
                            if (!stack.execCommand((opcodes)cmd[0], z)) return false;
                        } else if (cmd[0] == 0x6b || cmd[0] == 0x6c) {
                            if (!stack.execCommand((opcodes)cmd[0], ref altstack)) return false;
                        } else {
                            if (!stack.execCommand((opcodes)cmd[0])) return false;
                        }
                    } else {

                        stack.Push(cmd);
                        /*Check for bip16 (p2sh) and bip141 (segwit) patterns, 
                          * if found, push commands for execution*/

                        if (cmds_.Count == 3 && cmds_[0][0] == 0xa9 &&
                            cmds_[1].Length == 20 && cmds_[2][0] == 0x87) { //p2sh (bip16)
                            if (!execp2sh(ref stack, cmd)) return false;
                        }
                        if (stack.Count == 2 && stack[1][0] == 0x00 && stack[0].Length == 20) { //p2wpkh
                            if (!execp2wpkh(ref stack, witness)) return false;
                        }
                        if (stack.Count == 2 && stack[1][0] == 0x00 && cmds_[0].Length == 32) { //p2wsh
                            if (!execp2wsh(ref stack, witness)) return false;
                        }


                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            this.stack_ = stack.Stack;

            /*evaluate*/
            if (stack.Count == 0) {
                return false;
            } else {
                if (stack.Pop() == null) {
                    return false;
                }
            }
            return true;
        }

        bool execp2sh(ref BitcoinStack stack, byte[] cmd) {
            cmds_.RemoveAt(0); //op_hash160
            byte[] scriptHash = cmds_[0];
            cmds_.RemoveAt(0); //script hash
            cmds_.RemoveAt(0); //op_equal
            if (!stack.execCommand(opcodes.OP_HASH160)) return false; //hash top item in script
            stack.Push(scriptHash);
            if (!stack.execCommand(opcodes.OP_EQUAL)) return false; //check hashes are equal
            if (!stack.execCommand(opcodes.OP_VERIFY)) return false; //verify by checking 1 on stack

            /*at this point, the top element on the stack (cmd) is the redeem script*/
            byte[] redeemScriptBytes = Byte.join(Byte.encodeVarInt(cmd.Length), cmd);
            Stream s = new MemoryStream(redeemScriptBytes);
            Script redeemScript = Script.Parse(s);
            cmds_ = redeemScript.cmds_;

            return true;
        }

        bool execp2wpkh(ref BitcoinStack stack, List<byte[]> witness) {

            /*Run witness program for segwit v0*/
            /*This is for p2wpkh*/
            if (witness == null) {
                Console.WriteLine("Witness is null");
                return false;
            }

            byte[] h160 = stack.Pop();
            stack.Pop();
            if (witness[1].Length != 33) return false; //pubkey must be compressed
            cmds_.AddRange(witness);
            cmds_.AddRange(p2pkh(h160).cmds_);

            return true;
        }

        bool execp2wsh(ref BitcoinStack stack, List<byte[]> witness) {

            /*Run witness program for script*/
            /*This is for p2wsh*/
            byte[] s256 = stack.Pop();
            stack.Pop();
            byte[] witnessScript = witness[witness.Count - 1];

            for (int i = 0; i < witness.Count - 1; i++) {
                cmds_.Add(witness[i]);
            }

            if (Byte.bytesToString(s256) != Byte.bytesToString(Hash.sha256(witnessScript))) {
                Console.WriteLine("Bad sha256 {0} vs {1}", Byte.bytesToString(s256),
                Byte.bytesToString(Hash.sha256(witnessScript)));
                return false;
            }

            byte[] length = Helper.encodeVarInt(witnessScript.Length);
            byte[] parseScript = Byte.join(length, witnessScript);
            Stream s = new MemoryStream(parseScript);
            List<byte[]> witnessScriptCmds = Script.Parse(s).cmds_;

            foreach (byte[] element in witnessScriptCmds) {
                if (element.Length == 64 && element[0] == 0x04) { //looking for uncompressed pubkey
                    return false; //segwit must use compressed keys
                }
            }
            cmds_.AddRange(witnessScriptCmds);

            return true;
        }

        byte[] RawSerialise() {
            List<byte[]> result = new List<byte[]>();
            foreach (byte[] command in cmds_) {
                if (command.Length == 1) {
                    result.Add(command);
                } else {
                    int length = command.Length;
                    if (length < 75) {
                        byte[] tmp = new byte[1];
                        tmp[0] = Convert.ToByte(length);
                        result.Add(tmp);
                    } else if (length > 75 && length < 256) {
                        result.Add(Byte.intToLittleEndian(76, 1));
                        result.Add(Byte.intToLittleEndian(length, 1));
                    } else if (length >= 256 && length < 520) {
                        result.Add(Byte.intToLittleEndian(77, 1));
                        result.Add(Byte.intToLittleEndian(length, 2));
                    } else {
                        throw new Exception("Too long for command");
                    }
                    result.Add(command);
                }
            }
            List<byte> bytes = new List<byte>();
            foreach (byte[] item in result) {
                foreach (byte b in item) {
                    bytes.Add(b);
                }
            }
            return bytes.ToArray();
        }

        /// <summary>
        /// Serialises a bitcoin script.
        /// </summary>
        public byte[] Serialise() {
            byte[] result = this.RawSerialise();
            int total = result.Length;
            return Byte.join(Byte.encodeVarInt(total), result);
        }

        /// <summary>
        /// Parses a bitcoin script from stream.
        /// </summary>
        /// <returns>Bitcoin <see cref="Script"/>.</returns>
        public static Script Parse(Stream s) {
            int length = Helper.getVarIntLength(s);
            Script newScript = new Script();
            //skip var int bytes
            int count = 0;
            while (count < length) {
                byte[] current = new byte[1];
                s.Read(current, 0, 1);
                count += 1;
                byte current_byte = current[0];
                if (current_byte >= 0x01 && current_byte <= 0x4b) { //read n bytes
                    int n = Convert.ToInt32(current_byte);
                    byte[] tmp = new byte[n];
                    s.Read(tmp, 0, n);
                    newScript.Add(tmp);
                    count += n;
                } else if (current_byte == 0x4c) { //pushdata1
                    byte[] lengthByte = new byte[1];
                    s.Read(lengthByte, 0, 1);
                    int dataLength = Convert.ToInt32(lengthByte[0]);
                    byte[] tmp = new byte[dataLength];
                    s.Read(tmp, 0, dataLength);
                    newScript.Add(tmp);
                    count += dataLength + 1;
                } else if (current_byte == 0x4d) { //pushdata2
                    byte[] lengthByte = new byte[2];
                    s.Read(lengthByte, 0, 2);
                    int dataLength = BitConverter.ToInt32(lengthByte);
                    byte[] tmp = new byte[dataLength];
                    s.Read(tmp, 0, dataLength);
                    newScript.Add(tmp);
                    count += dataLength + 2;
                } else { //command
                    byte[] op_code = new byte[1];
                    op_code[0] = current_byte;
                    newScript.Add(op_code);
                }
            }

            return newScript;
        }

        /// <summary>
        /// Tests if the scriptpubkey is pay-to-pubkey-hash.
        /// </summary>
        public bool isP2PKH() {
            return (cmds_.Count == 5 &&
                cmds_[0][0] == 0x76 &&
                cmds_[1][0] == 0xa9 &&
                cmds_[2].Length == 20 &&
                cmds_[3][0] == 0x88 &&
                cmds_[4][0] == 0xac);
        }

        /// <summary>
        /// Tests if the scriptpubkey is pay-to-script-hash.
        /// </summary>
        public bool isP2SH() {
            return (
                cmds_.Count == 3 &&
                cmds_[0][0] == 0xa9 &&
                cmds_[1].Length == 20 &&
                cmds_[2][0] == 0x87);
        }

        /// <summary>
        /// Tests if the scriptpubkey is pay-to-witness-pubkey-hash.
        /// </summary>
        public bool isP2wpkh() {
            return (
                cmds_.Count == 2 &&
                cmds_[0][0] == 0x00 &&
                cmds_[1].Length == 20);
        }

        /// <summary>
        /// Tests if the scriptpubkey is pay-to-witness-script-hash.
        /// </summary>
        public bool isP2wsh() {
            return (
                cmds_.Count == 2 &&
                cmds_[0][0] == 0x00 &&
                cmds_[1].Length == 32);
        }

        /// <summary>
        /// Test if there is an OP_Return message.
        /// </summary>
        public bool isOpReturn() {
            return cmds_[0][0] == 0x6a;
        }

        /// <summary>
        /// (Deprecated) Tests if the scriptpubkey is pay-to-pubkey.
        /// </summary>
        public bool isP2PK() {
            return (
                cmds_.Count == 3 &&
                cmds_[1].Length == 33 || cmds_[1].Length == 64 &&
                cmds_[2][0] == 0xac);
        }
    }
}
