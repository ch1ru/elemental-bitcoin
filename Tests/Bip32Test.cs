using System;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace LBitcoin.Tests {
    static class Bip32Test {

        public static void RunAll() {
            test_vector_1();
            test_vector_2();
            test_vector_3();
            Debug.WriteLine("HD tests PASSED");
        }

        public static void test_vector_1() {
            /*Master node*/
            byte[] entropy = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            Mnemonic mnemonic = new Mnemonic(new Wordlist(), entropy, "Bitcoin seed");
            byte[] hmac = Hash.HMACSHA512Encode(entropy, Encoding.UTF8.GetBytes("Bitcoin seed"));
            HDPrivateKey root = new HDPrivateKey(hmac[0..32], hmac[32..64]);
            Debug.Assert(root.ToString() == "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi");
            Debug.Assert(root.Neuter().ToString() == "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8");
            /*Chain 0H*/
            Debug.Assert(root.ChildAt("m/0'").ToString() == "xprv9uHRZZhk6KAJC1avXpDAp4MDc3sQKNxDiPvvkX8Br5ngLNv1TxvUxt4cV1rGL5hj6KCesnDYUhd7oWgT11eZG7XnxHrnYeSvkzY7d2bhkJ7");
            Debug.Assert(root.ChildAt("m/0'").Neuter().ToString() == "xpub68Gmy5EdvgibQVfPdqkBBCHxA5htiqg55crXYuXoQRKfDBFA1WEjWgP6LHhwBZeNK1VTsfTFUHCdrfp1bgwQ9xv5ski8PX9rL2dZXvgGDnw");
            /*Chain 0H/1*/
            Debug.Assert(root.ChildAt("m/0'/1").ToString() == "xprv9wTYmMFdV23N2TdNG573QoEsfRrWKQgWeibmLntzniatZvR9BmLnvSxqu53Kw1UmYPxLgboyZQaXwTCg8MSY3H2EU4pWcQDnRnrVA1xe8fs");
            Debug.Assert(root.ChildAt("m/0'/1").Neuter().ToString() == "xpub6ASuArnXKPbfEwhqN6e3mwBcDTgzisQN1wXN9BJcM47sSikHjJf3UFHKkNAWbWMiGj7Wf5uMash7SyYq527Hqck2AxYysAA7xmALppuCkwQ");
            /*Chain 0H/1/2H/2*/
            Debug.Assert(root.ChildAt("m/0'/1/2'/2").ToString() == "xprvA2JDeKCSNNZky6uBCviVfJSKyQ1mDYahRjijr5idH2WwLsEd4Hsb2Tyh8RfQMuPh7f7RtyzTtdrbdqqsunu5Mm3wDvUAKRHSC34sJ7in334");
            Debug.Assert(root.ChildAt("m/0'/1/2'/2").Neuter().ToString() == "xpub6FHa3pjLCk84BayeJxFW2SP4XRrFd1JYnxeLeU8EqN3vDfZmbqBqaGJAyiLjTAwm6ZLRQUMv1ZACTj37sR62cfN7fe5JnJ7dh8zL4fiyLHV");
            /*Chain 0H/1/2H/2/1000000000*/
            Debug.Assert(root.ChildAt("m/0'/1/2'/2/1000000000").ToString() == "xprvA41z7zogVVwxVSgdKUHDy1SKmdb533PjDz7J6N6mV6uS3ze1ai8FHa8kmHScGpWmj4WggLyQjgPie1rFSruoUihUZREPSL39UNdE3BBDu76");
            Debug.Assert(root.ChildAt("m/0'/1/2'/2/1000000000").Neuter().ToString() == "xpub6H1LXWLaKsWFhvm6RVpEL9P4KfRZSW7abD2ttkWP3SSQvnyA8FSVqNTEcYFgJS2UaFcxupHiYkro49S8yGasTvXEYBVPamhGW6cFJodrTHy");
        }

        public static void test_vector_2() {
            /*Master node*/
            byte[] seed = BigInteger.Parse("00fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542", System.Globalization.NumberStyles.HexNumber).ToByteArray(true, true);
            Mnemonic mnemonic = new Mnemonic(new Wordlist(), seed, "Bitcoin seed");
            byte[] hmac = Hash.HMACSHA512Encode(seed, Encoding.UTF8.GetBytes("Bitcoin seed"));
            HDPrivateKey root = new HDPrivateKey(hmac[0..32], hmac[32..64]);
            Debug.Assert(root.ToString() == "xprv9s21ZrQH143K31xYSDQpPDxsXRTUcvj2iNHm5NUtrGiGG5e2DtALGdso3pGz6ssrdK4PFmM8NSpSBHNqPqm55Qn3LqFtT2emdEXVYsCzC2U");
            Debug.Assert(root.Neuter().ToString() == "xpub661MyMwAqRbcFW31YEwpkMuc5THy2PSt5bDMsktWQcFF8syAmRUapSCGu8ED9W6oDMSgv6Zz8idoc4a6mr8BDzTJY47LJhkJ8UB7WEGuduB");
            /*m/0/2147483647H/1*/
            Debug.Assert(root.ChildAt("m/0/2147483647'/1").ToString() == "xprv9zFnWC6h2cLgpmSA46vutJzBcfJ8yaJGg8cX1e5StJh45BBciYTRXSd25UEPVuesF9yog62tGAQtHjXajPPdbRCHuWS6T8XA2ECKADdw4Ef");
            Debug.Assert(root.ChildAt("m/0/2147483647'/1").Neuter().ToString() == "xpub6DF8uhdarytz3FWdA8TvFSvvAh8dP3283MY7p2V4SeE2wyWmG5mg5EwVvmdMVCQcoNJxGoWaU9DCWh89LojfZ537wTfunKau47EL2dhHKon");
            /*m/0/2147483647H/1/2147483646H/2*/
            Debug.Assert(root.ChildAt("m/0/2147483647'/1/2147483646'/2").ToString() == "xprvA2nrNbFZABcdryreWet9Ea4LvTJcGsqrMzxHx98MMrotbir7yrKCEXw7nadnHM8Dq38EGfSh6dqA9QWTyefMLEcBYJUuekgW4BYPJcr9E7j");
            Debug.Assert(root.ChildAt("m/0/2147483647'/1/2147483646'/2").Neuter().ToString() == "xpub6FnCn6nSzZAw5Tw7cgR9bi15UV96gLZhjDstkXXxvCLsUXBGXPdSnLFbdpq8p9HmGsApME5hQTZ3emM2rnY5agb9rXpVGyy3bdW6EEgAtqt");
        }

        public static void test_vector_3() {
            /*Master node*/
            byte[] seed = BigInteger.Parse("4b381541583be4423346c643850da4b320e46a87ae3d2a4e6da11eba819cd4acba45d239319ac14f863b8d5ab5a0d0c64d2e8a1e7d1457df2e5a3c51c73235be", System.Globalization.NumberStyles.HexNumber).ToByteArray(true, true);
            Mnemonic mnemonic = new Mnemonic(new Wordlist(), seed, "Bitcoin seed");
            byte[] hmac = Hash.HMACSHA512Encode(seed, Encoding.UTF8.GetBytes("Bitcoin seed"));
            HDPrivateKey root = new HDPrivateKey(hmac[0..32], hmac[32..64]);
            Debug.Assert(root.ToString() == "xprv9s21ZrQH143K25QhxbucbDDuQ4naNntJRi4KUfWT7xo4EKsHt2QJDu7KXp1A3u7Bi1j8ph3EGsZ9Xvz9dGuVrtHHs7pXeTzjuxBrCmmhgC6");
            Debug.Assert(root.Neuter().ToString() == "xpub661MyMwAqRbcEZVB4dScxMAdx6d4nFc9nvyvH3v4gJL378CSRZiYmhRoP7mBy6gSPSCYk6SzXPTf3ND1cZAceL7SfJ1Z3GC8vBgp2epUt13");
            /*m/0'*/
            Debug.Assert(root.ChildAt("m/0'").ToString() == "xprv9uPDJpEQgRQfDcW7BkF7eTya6RPxXeJCqCJGHuCJ4GiRVLzkTXBAJMu2qaMWPrS7AANYqdq6vcBcBUdJCVVFceUvJFjaPdGZ2y9WACViL4L");
            Debug.Assert(root.ChildAt("m/0'").Neuter().ToString() == "xpub68NZiKmJWnxxS6aaHmn81bvJeTESw724CRDs6HbuccFQN9Ku14VQrADWgqbhhTHBaohPX4CjNLf9fq9MYo6oDaPPLPxSb7gwQN3ih19Zm4Y");
        }
    }

    static class MnemonicTest {

        public static void RunAll() {

            TestVector12Words();
            TestVector24Words();
        }

        public static void TestVector12Words() {

            string mnemonicStr = "legal winner thank year wave sausage worth useful legal winner thank yellow";
            Mnemonic mnemonic = new Mnemonic(mnemonicStr, password: "TREZOR");
            Debug.Assert(Byte.bytesToString(mnemonic.ToSeed("TREZOR")) == "2e8905819b8723fe2c1d161860e5ee1830318dbf49a83bd451cfb8440c28bd6fa457fe1296106559a3c80937a1c1069be3a3a5bd381ee6260e8d9739fce1f607");
            HDPrivateKey xpriv = new HDPrivateKey(mnemonic, "TREZOR");
            Debug.Assert(xpriv.ToString() == "xprv9s21ZrQH143K2gA81bYFHqU68xz1cX2APaSq5tt6MFSLeXnCKV1RVUJt9FWNTbrrryem4ZckN8k4Ls1H6nwdvDTvnV7zEXs2HgPezuVccsq");
        }

        public static void TestVector24Words() {

            string mnemonicStr = "void come effort suffer camp survey warrior heavy shoot primary clutch crush open amazing screen patrol group space point ten exist slush involve unfold";
            Mnemonic mnemonic = new Mnemonic(mnemonicStr, password: "TREZOR");
            Debug.Assert(Byte.bytesToString(mnemonic.ToSeed("TREZOR")) == "01f5bced59dec48e362f2c45b5de68b9fd6c92c6634f44d6d40aab69056506f0e35524a518034ddc1192e1dacd32c1ed3eaa3c3b131c88ed8e7e54c49a5d0998");
            HDPrivateKey xpriv = new HDPrivateKey(mnemonic, "TREZOR");
            Debug.Assert(xpriv.ToString() == "xprv9s21ZrQH143K39rnQJknpH1WEPFJrzmAqqasiDcVrNuk926oizzJDDQkdiTvNPr2FYDYzWgiMiC63YmfPAa2oPyNB23r2g7d1yiK6WpqaQS");
        }
        
    }

    static class Bip84Test {

        public static void RunAll() {

            TestVector12Words();
        }

        public static void TestVector12Words() {
            string mnemonicStr = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
            Mnemonic mnemonic = new Mnemonic(mnemonicStr);
            HDPrivateKey zpriv = new HDPrivateKey(mnemonic, "", false, 2);
            Debug.Assert(zpriv.ToString() == "zprvAWgYBBk7JR8Gjrh4UJQ2uJdG1r3WNRRfURiABBE3RvMXYSrRJL62XuezvGdPvG6GFBZduosCc1YP5wixPox7zhZLfiUm8aunE96BBa4Kei5");
            Debug.Assert(zpriv.Neuter().ToString() == "zpub6jftahH18ngZxLmXaKw3GSZzZsszmt9WqedkyZdezFtWRFBZqsQH5hyUmb4pCEeZGmVfQuP5bedXTB8is6fTv19U1GQRyQUKQGUTzyHACMF");
        }
    }

    static class Bip49Test {
        public static void RunAll() {

            TestVector12Words();
        }

        public static void TestVector12Words() {
            string mnemonicStr = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
            Mnemonic mnemonic = new Mnemonic(mnemonicStr);
            HDPrivateKey upriv = new HDPrivateKey(mnemonic, "", true, 1);
            upriv = upriv.ChildAt("m/49'/1'/0'");
            Debug.Assert(upriv.ToString() == "uprv91G7gZkzehuMVxDJTYE6tLivdF8e4rvzSu1LFfKw3b2Qx1Aj8vpoFnHdfUZ3hmi9jsvPifmZ24RTN2KhwB8BfMLTVqaBReibyaFFcTP1s9n");
            Debug.Assert(upriv.Neuter().ToString() == "upub5EFU65HtV5TeiSHmZZm7FUffBGy8UKeqp7vw43jYbvZPpoVsgU93oac7Wk3u6moKegAEWtGNF8DehrnHtv21XXEMYRUocHqguyjknFHYfgY");
        }
    }
}
