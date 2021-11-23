# elementary-bitcoin

A hackey bitcoin library written for the .NET platform which implements most relevant bips. It provides basic functionality such as elliptic curve key
generation and signing, HD key derivation, bitcoin scripting, networking and segwit. 

Largely based on Jimmy Song's *Programming Bitcoin* book, ported in C#.


***Note:** this library is for development/testing or educational purposes and not intended for real-life financial applications.*


## **Examples:**

**Generate bitcoin address**
```
var rand = csrng.randomInt(32);
PrivateKey pk = new PrivateKey(rand);
PublicKey pubKey = pk.pubKey();
BitcoinAddress addr = pubKey.getAddr(AddressType.legacy);
Console.WriteLine(addr);
 ```
 
 **Signing a message**
 ```
 byte[] message = Encoding.UTF8.GetBytes("This is an important message");
 Signature sig = pk.sign(message);
 ```
 and verify
 ```
if(pubKey.verify("This is an important message", sig)) {
	Console.WriteLine("signature is valid");
}
else {
	Console.WriteLine("Signature is not valid");
}
```


**Creating transactions**

Inputs = previous txid + txindex + scriptSig (unlocking script) + sequence
```
Script scriptSig = new Script();
scriptSig.Add(sig.derEncode());
scriptSig.Add(pubKey.getCompressed());
TxIn input = new TxIn(prevTxid, 0, scriptSig);
```

Outputs = amount + scriptPubKey (locking script)
```
byte[] hashedOutput = Hash.hash160(pubKey.getCompressed());
Script lockingScript = Script.p2pkh(hashedOutput);
TxOut output = new TxOut(100000, lockingScript) //output for 100,000 satoshis
```

Full transaction = version + inputs + outputs + locktime
```
int version = 1;
TxIn[] txins = new TxIn[] { input };
TxOut[] txouts = new TxOut[] { output };
byte[] locktime = 0xffffffff;
Transaction tx = new Transaction(version, txins, txouts, locktime);
```

Broadcast  transaction to the network:
```
tx.broadcast();
```

## **Documentation**

See our beginner's [Tutorial](https://ch1ru.github.io/elemental-bitcoin/) 

Full api docs can be found here

## NUGET package

## Dependencies

- SshNet cryptography library (https://github.com/sshnet/Cryptography/) Used for Ripemd hash algorithm.

Can be installed with NUGET packet manager or from [website](https://www.nuget.org/packages/SshNet.Security.Cryptography/1.3.0?_src=template)

## **Future Development**

Schnorr signature scheme and taproot-compatibility in upcoming versions. 
Contributing with a pull request is much appreciated!


Bitcoin address: bc1q25dlf23qvafvnewx5c0hnxkyv7u7pudz9an5se
