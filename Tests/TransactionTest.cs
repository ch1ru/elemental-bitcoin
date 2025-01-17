﻿using System.Numerics;
using System.IO;
using System.Diagnostics;
using LBitcoin.Ecc;

namespace LBitcoin.Tests {
    public static class TransactionTest {

        public static void RunAll() {
            test_ParseVersion();
            test_ParseInputs();
            test_ParseOutputs();
            test_ParseLocktime();
            test_Serialise();
            test_input_value();
            test_inputPubkey();
            test_fee();
            test_sigHash();
            test_p2pkh();
            test_p2sh();
            test_p2wpkh();
            test_p2wpkh_p2sh();
            test_p2wsh();
            test_sign_input();
            test_isCoinbase();
            test_coinbaseHeight();
            Debug.WriteLine("Transaction tests PASSED");
        }

        public static void test_ParseVersion() {
            byte[] rawTx = BigInteger.Parse("0100000001813f79011acb80925dfe69b3def355fe914bd1d96a3f5f71bf8303c6a989c7d1000000006b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278afeffffff02a135ef01000000001976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac99c39800000000001976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac19430600", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawTx);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(tx.Version == 1);
        }

        public static void test_ParseInputs() {
            byte[] rawTx = BigInteger.Parse("0100000001813f79011acb80925dfe69b3def355fe914bd1d96a3f5f71bf8303c6a989c7d1000000006b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278afeffffff02a135ef01000000001976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac99c39800000000001976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac19430600", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawTx);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(tx.TxIns.Length == 1);
            byte[] want = BigInteger.Parse("d1c789a9c60383bf715f3f6ad9d14b91fe55f3deb369fe5d9280cb1a01793f81", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Debug.Assert(Byte.bytesToString(tx.TxIns[0].GetPrevTxid()) == Byte.bytesToString(want)); //fails due to endianess
            Debug.Assert(tx.TxIns[0].GetPrevIndex() == 0);
            string want2 = "6b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278a";
            Debug.Assert(Byte.bytesToString(tx.TxIns[0].scriptSig_.Serialise()) == want2);
            Debug.Assert(tx.TxIns[0].GetSequence() == 0xfffffffe);
        }

        public static void test_ParseOutputs() {
            byte[] rawTx = BigInteger.Parse("0100000001813f79011acb80925dfe69b3def355fe914bd1d96a3f5f71bf8303c6a989c7d1000000006b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278afeffffff02a135ef01000000001976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac99c39800000000001976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac19430600", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawTx);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(tx.outputs_.Length == 2);
            ulong want = 32454049;
            Debug.Assert(tx.outputs_[0].Amount() == want);
            string want2 = "1976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac";
            Debug.Assert(Byte.bytesToString(tx.outputs_[0].ScriptPubKey.Serialise()) == want2);
            want = 10011545;
            Debug.Assert(tx.outputs_[1].Amount() == want);
            want2 = "1976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac";
            Debug.Assert(Byte.bytesToString(tx.outputs_[1].ScriptPubKey.Serialise()) == want2);

        }

        public static void test_ParseLocktime() {
            byte[] rawTx = BigInteger.Parse("0100000001813f79011acb80925dfe69b3def355fe914bd1d96a3f5f71bf8303c6a989c7d1000000006b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278afeffffff02a135ef01000000001976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac99c39800000000001976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac19430600", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawTx);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(tx.Locktime == 410393);
        }

        public static void test_Serialise() {
            string rawTxStr = "0100000001813f79011acb80925dfe69b3def355fe914bd1d96a3f5f71bf8303c6a989c7d1000000006b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278afeffffff02a135ef01000000001976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac99c39800000000001976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac19430600";
            byte[] rawTx = BigInteger.Parse(rawTxStr, System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawTx);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(Byte.bytesToString(tx.Serialise()) == rawTxStr);
        }

        public static void test_input_value() {
            byte[] txHash = BigInteger.Parse("d1c789a9c60383bf715f3f6ad9d14b91fe55f3deb369fe5d9280cb1a01793f81", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            int index = 0;
            ulong want = 42505594;
            TxIn txIn = new TxIn(txHash, index);
            Debug.Assert(txIn.Amount() == want);
        }

        public static void test_inputPubkey() {
            byte[] txHash = BigInteger.Parse("d1c789a9c60383bf715f3f6ad9d14b91fe55f3deb369fe5d9280cb1a01793f81", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            int index = 0;
            string want = "1976a914a802fc56c704ce87c42d7c92eb75e7896bdc41ae88ac";
            TxIn txIn = new TxIn(txHash, index);
            Debug.Assert(Byte.bytesToString(txIn.ScriptPubKey().Serialise()) == want);
        }

        public static void test_fee() {
            byte[] txBytes = BigInteger.Parse("0100000001813f79011acb80925dfe69b3def355fe914bd1d96a3f5f71bf8303c6a989c7d1000000006b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278afeffffff02a135ef01000000001976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac99c39800000000001976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac19430600", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(txBytes);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(tx.Fee() == 40000);

            txBytes = BigInteger.Parse("010000000456919960ac691763688d3d3bcea9ad6ecaf875df5339e148a1fc61c6ed7a069e010000006a47304402204585bcdef85e6b1c6af5c2669d4830ff86e42dd205c0e089bc2a821657e951c002201024a10366077f87d6bce1f7100ad8cfa8a064b39d4e8fe4ea13a7b71aa8180f012102f0da57e85eec2934a82a585ea337ce2f4998b50ae699dd79f5880e253dafafb7feffffffeb8f51f4038dc17e6313cf831d4f02281c2a468bde0fafd37f1bf882729e7fd3000000006a47304402207899531a52d59a6de200179928ca900254a36b8dff8bb75f5f5d71b1cdc26125022008b422690b8461cb52c3cc30330b23d574351872b7c361e9aae3649071c1a7160121035d5c93d9ac96881f19ba1f686f15f009ded7c62efe85a872e6a19b43c15a2937feffffff567bf40595119d1bb8a3037c356efd56170b64cbcc160fb028fa10704b45d775000000006a47304402204c7c7818424c7f7911da6cddc59655a70af1cb5eaf17c69dadbfc74ffa0b662f02207599e08bc8023693ad4e9527dc42c34210f7a7d1d1ddfc8492b654a11e7620a0012102158b46fbdff65d0172b7989aec8850aa0dae49abfb84c81ae6e5b251a58ace5cfeffffffd63a5e6c16e620f86f375925b21cabaf736c779f88fd04dcad51d26690f7f345010000006a47304402200633ea0d3314bea0d95b3cd8dadb2ef79ea8331ffe1e61f762c0f6daea0fabde022029f23b3e9c30f080446150b23852028751635dcee2be669c2a1686a4b5edf304012103ffd6f4a67e94aba353a00882e563ff2722eb4cff0ad6006e86ee20dfe7520d55feffffff0251430f00000000001976a914ab0c0b2e98b1ab6dbf67d4750b0a56244948a87988ac005a6202000000001976a9143c82d7df364eb6c75be8c80df2b3eda8db57397088ac46430600", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            s = new MemoryStream(txBytes);
            tx = Transaction.Parse(s);
            Debug.Assert(tx.Fee() == 140500);
        }

        public static void test_sigHash() {
            /*legacy*/
            string txid = "452c629d67e41baec3ac6f04fe744b4b9617f8f859c63b3002f8684e7a4fee03";
            Transaction tx = TxFetcher.Fetch(txid);
            BigInteger want = BigInteger.Parse("27e0c5994dec7824e56dec6b2fcb342eb7cdb0d0957c2fce9882f715e85d81a6", System.Globalization.NumberStyles.HexNumber);
            Debug.Assert(tx.sigHash(0) == want);
            /*segwit*/
            txid = "bfd5d009ba083caa0ab49e2e6301e487690fea41b20b2e17dae92c4f7d60165e";
            tx = TxFetcher.Fetch(txid);
            want = BigInteger.Parse("17174620482529247160313241192122825540703212391130841682361125259764187024751", System.Globalization.NumberStyles.Integer);
            Debug.Assert(tx.sigHashBip143(0) == want);
        }

        public static void test_p2pkh() {
            string txid = "452c629d67e41baec3ac6f04fe744b4b9617f8f859c63b3002f8684e7a4fee03";
            Transaction tx = TxFetcher.Fetch(txid);
            Debug.Assert(tx.Verify());
            txid = "5418099cc755cb9dd3ebc6cf1a7888ad53a1a3beb5a025bce89eb1bf7f1650a2";
            tx = TxFetcher.Fetch(txid, testnet: true);
            Debug.Assert(tx.Verify());
        }

        public static void test_p2sh() {
            string txid = "46df1a9484d0a81d03ce0ee543ab6e1a23ed06175c104a178268fad381216c2b";
            Transaction tx = TxFetcher.Fetch(txid);
            Debug.Assert(tx.Verify());
        }

        public static void test_p2wpkh() {
            string txid = "d869f854e1f8788bcff294cc83b280942a8c728de71eb709a2c29d10bfe21b7c";
            Transaction tx = TxFetcher.Fetch(txid, testnet: true);
            Debug.Assert(tx.Verify());
        }

        public static void test_p2wpkh_p2sh() {
            string txid = "c586389e5e4b3acb9d6c8be1c19ae8ab2795397633176f5a6442a261bbdefc3a";
            Transaction tx = TxFetcher.Fetch(txid);
            Debug.Assert(tx.Verify());
        }

        public static void test_p2wsh() {
            //find test example
        }

        public static void test_p2wsh_p2sh() {
            string txid = "24809132036b7ce6eec04e53b3be41140a37f0768f36967615955e869b38371d";
            Transaction tx = TxFetcher.Fetch(txid);
            Debug.Assert(tx.Verify());
        }

        public static void test_sign_input() {
            var privateKey = new PrivateKey(8675309);
            byte[] rawBytes = BigInteger.Parse("010000000199a24308080ab26e6fb65c4eccfadf76749bb5bfa8cb08f291320b3c21e56f0d0d00000000ffffffff02408af701000000001976a914d52ad7ca9b3d096a38e752c2018e6fbc40cdf26f88ac80969800000000001976a914507b27411ccf7f16f10297de6cef3f291623eddf88ac00000000", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawBytes);
            Transaction tx = Transaction.Parse(s, testnet: true);

            Debug.Assert(tx.SignInput(0, privateKey));
            string want = "010000000199a24308080ab26e6fb65c4eccfadf76749bb5bfa8cb08f291320b3c21e56f0d0d0000006b4830450221008ed46aa2cf12d6d81065bfabe903670165b538f65ee9a3385e6327d80c66d3b502203124f804410527497329ec4715e18558082d489b218677bd029e7fa306a72236012103935581e52c354cd2f484fe8ed83af7a3097005b2f9c60bff71d35bd795f54b67ffffffff02408af701000000001976a914d52ad7ca9b3d096a38e752c2018e6fbc40cdf26f88ac80969800000000001976a914507b27411ccf7f16f10297de6cef3f291623eddf88ac00000000";
            Debug.Assert(Byte.bytesToString(tx.Serialise()) == want);

        }

        public static void test_isCoinbase() {
            byte[] rawTx = BigInteger.Parse("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff5e03d71b07254d696e656420627920416e74506f6f6c20626a31312f4542312f4144362f43205914293101fabe6d6d678e2c8c34afc36896e7d9402824ed38e856676ee94bfdb0c6c4bcd8b2e5666a0400000000000000c7270000a5e00e00ffffffff01faf20b58000000001976a914338c84849423992471bffb1a54a8d9b1d69dc28a88ac00000000", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawTx);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(tx.IsCoinbase());
        }

        public static void test_coinbaseHeight() {
            byte[] rawTx = BigInteger.Parse("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff5e03d71b07254d696e656420627920416e74506f6f6c20626a31312f4542312f4144362f43205914293101fabe6d6d678e2c8c34afc36896e7d9402824ed38e856676ee94bfdb0c6c4bcd8b2e5666a0400000000000000c7270000a5e00e00ffffffff01faf20b58000000001976a914338c84849423992471bffb1a54a8d9b1d69dc28a88ac00000000", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            Stream s = new MemoryStream(rawTx);
            Transaction tx = Transaction.Parse(s);
            Debug.Assert(tx.CoinbaseHeight() == 465879);

            rawTx = BigInteger.Parse("0100000001813f79011acb80925dfe69b3def355fe914bd1d96a3f5f71bf8303c6a989c7d1000000006b483045022100ed81ff192e75a3fd2304004dcadb746fa5e24c5031ccfcf21320b0277457c98f02207a986d955c6e0cb35d446a89d3f56100f4d7f67801c31967743a9c8e10615bed01210349fc4e631e3624a545de3f89f5d8684c7b8138bd94bdd531d2e213bf016b278afeffffff02a135ef01000000001976a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac99c39800000000001976a9141c4bc762dd5423e332166702cb75f40df79fea1288ac19430600", System.Globalization.NumberStyles.HexNumber).ToByteArray(false, true);
            s = new MemoryStream(rawTx);
            tx = Transaction.Parse(s);
            Debug.Assert(tx.CoinbaseHeight() == 0);
        }
    }
}
