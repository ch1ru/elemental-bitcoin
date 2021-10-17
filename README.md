# elemental-bitcoin

A hackey bitcoin library written for the .NET platform which implements most relevant bips. It provides basic functionality such as elliptic curve key
generation and signing, HD key derivation, bitcoin scripting, networking and segwit. 

Largely based on Jimmy Song's *Programming Bitcoin* book, ported in C#.


***Note:** this library is for development/testing or educational purposes and should not be used for real-life financial applications.*


# **Examples:**

*Generate bitcoin address
```
var rand = csrng.randomInt(32);
PrivateKey pk = new PrivateKey(rand);
PublicKey pubKey = pk.pubKey();
BitcoinAddress addr = new PublicKey().getAddr(AddressType.legacy);
Console.WriteLine(addr);
 ```
 

# **Documentation**

See our Tutorial 

Full docs can be found here

We also have a youtube tutorial

# *NUGET package*

# **Future Development**

We intend to add Schnorr signature scheme and taproot-compatibility in upcoming versions. 
Contributing with a pull request is much appreciated!
