# elementary-bitcoin

A hackey bitcoin library written for the .NET platform which implements most relevant bips. It provides basic functionality such as elliptic curve key
generation and signing, HD key derivation, bitcoin scripting, networking and segwit. 

Largely based on Jimmy Song's *Programming Bitcoin* book, ported in C#.


***Note:** this library is for development/testing or educational purposes and not intended for real-life financial applications.*


## **Examples:**

**Generate bitcoin address**
```c#
var rand = csrng.genKey();
PrivateKey pk = new PrivateKey(rand);
PublicKey pubKey = pk.pubKey();
BitcoinAddress addr = pubKey.getAddr(AddressType.nativeSegwit, testnet: true);
Console.WriteLine(addr);

//tb1qyk0j2yt44z4y9rns98duph7pkuarde5hhtskth
 ```
 
 **Signing a message**
 ```c#
 byte[] message = Encoding.UTF8.GetBytes("This is an important message");
 Signature sig = pk.sign(message);
 ```
 and verify
 ```c#
if(pubKey.verify("This is an important message", sig)) {
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
scriptSig.Add(sig.derEncode());
scriptSig.Add(pubKey.getCompressed());
TxIn input = new TxIn(prevTxid, 0, scriptSig);
```

Outputs = amount + scriptPubKey (locking script)
```c#
byte[] hashedOutput = Hash.hash160(pubKey.getCompressed());
Script lockingScript = Script.p2wpkh(hashedOutput);
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

## Install with NUGET Packet Manager

No NUGET package as of yet, on it's way.

## Dependencies

- SshNet cryptography library (https://github.com/sshnet/Cryptography/) Used for Ripemd hash algorithm.

Can be installed with NUGET packet manager or from [here](https://www.nuget.org/packages/SshNet.Security.Cryptography/1.3.0?_src=template)

## Limitations

Some of the networking capabilities need extending and contain bugs 🐛 
Newer protocols used in p2p messaging such as addrv2 formats and compact blocks not present.  

## **Future Development**

- Schnorr signature scheme and taproot-compatibility
- Replace bip37 legacy bloom filters with bip158 compact blocks
- Add support for torv2 &torv3
- Add version 2 network address formats
- Add cmpctBlock network commands

Contributing with a pull request is much appreciated!


Bitcoin address: bc1q25dlf23qvafvnewx5c0hnxkyv7u7pudz9an5se
