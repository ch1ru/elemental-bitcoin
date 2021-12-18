using System;
using System.Numerics;
using System.Collections.Generic;
using LBitcoin.Ecc;
using LBitcoin.Tests;
using LBitcoin.Networking;
using LBitcoin.Networking.P2P;
using System.Net;
using System.IO;
using System.Text;


namespace LBitcoin
{
    class Program
    {
		public static void Main() {

			FieldElementTest.RunAll();
			PointTest.RunAll();
			S256Test.RunAll();
			BlockTest.RunAll();
			BlockTest.RunAll();
			MerkleTreeTest.RunAll();
			MerkleBlockTest.RunAll();
			BloomTest.RunAll();
			TransactionTest.RunAll();
			ScriptTest.RunAll();
			Bip32Test.RunAll();
			Bip49Test.RunAll();
			Bip84Test.RunAll();
			OpTest.RunAll();


		}
	}
}
