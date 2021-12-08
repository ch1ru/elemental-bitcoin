using System;
using System.Numerics;
using System.Collections.Generic;
using LBitcoin.Ecc;
using LBitcoin.Tests;
using LBitcoin.Networking;
using LBitcoin.Networking.P2P;
using System.Net;
using System.IO;


namespace LBitcoin
{
    class Program
    {
		public static void Main() {
			/*
			//Secp256k1 curve = new Secp256k1();
			BigInteger k1 = csrng.genKey();
			PrivateKey pk1 = new PrivateKey(k1);
			Console.WriteLine("private key {0}", pk1);
			PublicKey pub1 = pk1.pubKey();
			BigInteger k2 = csrng.genKey();
			PrivateKey pk2 = new PrivateKey(k2);
			PublicKey pub2 = pk2.pubKey();
			BitcoinAddress addr1 = new BitcoinAddress(pub1, AddressType.nativeSegwit);
			Console.WriteLine("Address is {0}", addr1);
			BigInteger z1 = csrng.genKey();
			Signature sig1 = pk1.sign(z1);

			Script scriptsig = new Script();
			Script singlescriptsig = new Script();
			Script scriptpk = Script.p2wpkh(Hash.hash160(pub1.Compressed));
			Script script1 = singlescriptsig + scriptpk;
			List<byte[]> wit = new List<byte[]>();
			wit.Add(sig1.DerEncode());
			wit.Add(pub1.Compressed);
			if (script1.evaluate(z1, wit)) Console.WriteLine("Evaluated!");


			/*
			scriptsig.Add(opcodes.OP_0);
			scriptsig.Add(sig1.DerEncode());
			byte[] redeemscript = BigInteger.Parse("51" + Byte.bytesToString(new byte[] { Convert.ToByte(pub1.Compressed.Length) }) + Byte.bytesToString(pub1.Compressed) + Byte.bytesToString(new byte[] { Convert.ToByte(pub2.Compressed.Length) }) + Byte.bytesToString(pub2.Compressed) + "52ae", NumberStyles.HexNumber).ToByteArray(false, true);
			scriptsig.Add(redeemscript);

			Script scriptPubKey = Script.p2sh(Hash.hash160(redeemscript));
			Console.WriteLine(scriptPubKey.address());

			Script script = scriptsig + scriptPubKey;
			
			if(script.evaluate(z1, witness: null)) {
				Console.WriteLine("Evaluated!");
            }
			else {
				Console.WriteLine("Not evaluated");
            }
			*/

			/*
            FieldElementTest.RunAll();
			Sha256FieldTest.RunAll();
            PointTest.RunAll();
			Point256Test.RunAll();
            BlockTest.RunAll();
			BlockTest.RunAll();
            MerkleTreeTest.RunAll();
            MerkleBlockTest.RunAll();
            BloomTest.RunAll();
            //TransactionTest.RunAll();
            ScriptTest.RunAll();
			HDTest.RunAll();
			OpTest.RunAll();
			

			/*Create a wordlist, default is english*/
			/*To Change the language, overload with the language string e.g. Wordlist("spanish")*/




			//Transaction tx = txFetcher.fetch("452c629d67e41baec3ac6f04fe744b4b9617f8f859c63b3002f8684e7a4fee03");
			/*
			TxIn[] inputs = new TxIn[] { new TxIn(Encoding.UTF8.GetBytes("d1c789a9c60383bf715f3f6ad9d14b91fe55f3deb369fe5d9280cb1a01793f81"), 0, null, 0xfffffffe) };
			TxOut[] outputs = new TxOut[2];
			byte[] scriptpk0bytes = BigInteger.Parse("76a914bc3b654dca7e56b04dca18f2566cdaf02e8d9ada88ac", NumberStyles.HexNumber).ToByteArray(false, true);
			byte[] scriptpk1bytes = BigInteger.Parse("76a9141c4bc762dd5423e332166702cb75f40df79fea1288ac", NumberStyles.HexNumber).ToByteArray(false, true);
			Stream scriptpk0 = new MemoryStream(scriptpk0bytes);
			Stream scriptpk1 = new MemoryStream(scriptpk1bytes);
			outputs[0] = new TxOut(0x0000000001ef35a1, Script.Parse(scriptpk0));
			outputs[1] = new TxOut(0x000000000098c399, Script.Parse(scriptpk1));
			Transaction tx = new Transaction(1, inputs, outputs, 0x00064319);*/
			//tx.getData();

			//if (tx.verify()) Console.WriteLine("Verified!");
			//HDTest.RunAll();
			/*
			var privateKey = new PrivateKey(5);
			Signature sig = privateKey.sign(5);
			PublicKey pubkey = privateKey.pubKey();
			if(pubkey.verify(77, sig)) {
				Console.WriteLine("VALID!!!");
            }
			else {
				Console.WriteLine("NOT VALID!!!");
            }

			*/
			/*
			SimpleNode node = new SimpleNode(IPAddress.Loopback, 8333, false, true);
			node.handshake();
			_ = node.send(new VersionMessage(70014, null, 0, null, null, null, "1234567890", 100, false));
			
			byte[] bs = new byte[] { 0x80,0x11,0x01,0x00,0x08,0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x23,0x49,0xa7,0x61,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x08,0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x5e,0x6f,0x21,0x39,0x3d,0xa6,0x22,0x74,0x10,0x2f,0x53,0x61,0x74,0x6f,0x73,0x68,0x69,0x3a,0x32,0x32,0x2e,0x30,0x2e,0x30,0x2f,0xc6,0x51,0x05,0x00,0x01,0xf9,0xbe,0xb4,0xd9,0x76,0x65,0x72,0x61,0x63,0x6b,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x5d,0xf6,0xe0,0xe2 };
			VersionMessage m = VersionMessage.parse(new MemoryStream(bs));
			Console.WriteLine(Byte.bytesToString(m.serialise()));
			Console.WriteLine(m);*/
			Bip32Test.RunAll();
			MnemonicTest.RunAll();
			Bip84Test.RunAll();
			Bip49Test.RunAll();

			byte[] entropy = new byte[] { 0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80,0x80 };
			Mnemonic mnemonic = new Mnemonic(new Wordlist(), entropy, "TREZOR");
			Console.WriteLine(Byte.bytesToString(mnemonic.ToSeed("TREZOR")));
			HDPrivateKey root = new HDPrivateKey(mnemonic, "TREZOR");
			Console.WriteLine(root);


			string m = "あいこくしん あいさつ あいだ あおぞら あかちゃん あきる あけがた あける あこがれる あさい あさひ あしあと";
			Mnemonic m2 = new Mnemonic(m, "TREZOR");
			HDPrivateKey xpk = new HDPrivateKey(mnemonic, "TREZOR");
			Console.WriteLine(xpk);

		}
	}
}
