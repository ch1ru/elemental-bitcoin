# elementary-bitcoin

A hackey bitcoin library written for the .NET platform which implements most relevant bips. It provides basic functionality such as elliptic curve key
generation and signing, HD key derivation, bitcoin scripting, networking and segwit. 

Largely based on Jimmy Song's *Programming Bitcoin* book, ported in C#.


***Note:** this library is for development/testing or educational purposes and not intended for real-life financial applications.*


## **Examples:**

**Generate bitcoin address**
```c#
var rand = Csrng.GenKey();
PrivateKey pk = new PrivateKey(rand);
PublicKey pubKey = pk.pubKey();
BitcoinAddress addr = pubKey.GetAddr(AddressType.nativeSegwit, testnet: true);
Console.WriteLine(addr);

//tb1qyk0j2yt44z4y9rns98duph7pkuarde5hhtskth
 ```
 
 **Signing a message**
 ```c#
 byte[] message = Encoding.UTF8.GetBytes("This is an important message");
 Signature sig = pk.Sign(message);
 ```
 and verify
 ```c#
if(pubKey.Verify("This is an important message", sig)) {
	Console.WriteLine("signature is valid");
}
else {
	Console.WriteLine("Signature is not valid");
}
```


**Creating transactions**

Inputs = previous txid + txindex + scriptSig (unlocking script) + sequence
```c#
Script scriptSig = new Script();
scriptSig.Add(sig.DerEncode());
scriptSig.Add(pubKey.GetCompressed());
TxIn input = new TxIn(prevTxid, 0, scriptSig);
```

Outputs = amount + scriptPubKey (locking script)
```c#
byte[] hashedOutput = Hash.hash160(pubKey.getCompressed());
Script lockingScript = Script.P2WPKH(hashedOutput);
TxOut output = new TxOut(100000, lockingScript); //output for 100,000 satoshis
```

Full transaction = version + inputs + outputs + locktime
```c#
int version = 1;
TxIn[] txins = new TxIn[] { input };
TxOut[] txouts = new TxOut[] { output };
byte[] locktime = 0xffffffff;
Transaction tx = new Transaction(version, txins, txouts, locktime);
```

## **Documentation**

See our beginner's [Tutorial](https://ch1ru.github.io/elementary-bitcoin/) 

## How to use?

**Install with NUGET Packet Manager:**

Navigate to the ```Manage NuGet Packages``` window in visual studio and install *LBitcoin* package.

![Nuget install](https://github.com/ch1ru/elementary-bitcoin/blob/main/docs/assets/Nuget%20install.png)

**Install with .NET core:**

```dotnet add package LBitcoin```

**Packet Manager CLI:**

```Install-Package LBitcoin```

## Dependencies

- SshNet cryptography library (https://github.com/sshnet/Cryptography/) Used for Ripemd hash algorithm.

Can be installed with NUGET packet manager or from [here](https://www.nuget.org/packages/SshNet.Security.Cryptography/1.3.0?_src=template)

## Limitations

Some of the networking capabilities need extending and contain bugs 🐛 
Newer protocols used in p2p messaging such as addrv2 formats and compact blocks not present.  

## **Future Development**

- Schnorr signature scheme and taproot-compatibility
- Replace bip37 legacy bloom filters with bip158 compact blocks
- Add RPC commands
- Add opcodes for contructing HTLCs
- Add version 2 network address formats
- Add cmpctBlock network commands

Contributing with a pull request is much appreciated!


Bitcoin address: bc1q25dlf23qvafvnewx5c0hnxkyv7u7pudz9an5se
