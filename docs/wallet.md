## Wallets

What we have so far is a way to generate a single address and the potential to spend from that address using a digital signature. However, this is not very private, since a person has a single public key and if their identity is linked to that public key, anyone can tell what they are spending money on (since the transaction history is completely transparent). We also don't have any accounting capabilities. Perhaps we want to separate our funds into three accounts, one for rent, one for investments and other for recreation. We cannot do this using single key pairs. 

In this chapter, we will look at how to solve this with a data structure called a hierarchical deterministic tree, which is the basis for all modern wallets. This allows us to have many keys and addresses that can be derived from a single root private and public key. It will also be necessary to make an easy back up of this key, which we will also discuss in this chapter. If you Remember, the private key is just a 256 bit number and not very human readable! This is where the idea of a mnemonic comes in - a direct mapping of the random entropy, which can be translated into the extended key. 

Since we are already getting ahead of ourselves, let's look at the code to do this, then break it apart. 

```
Wordlist wordlist = new Wordlist() 
Mnemonic mnemonic = new Mnemonic(wordlist, 12, "password123");
Console.WriteLine(mnemonic);

HDPrivate Key extKey = new HDPrivateKey(mnemonic);
Console.WriteLine(extKey);
HDPublicKey xpub = extKey.Neuter();
Console.WriteLine(xpub);

var address1 = xpub.ChildAt(path: "m84/0'/0'/0/0");
var address2 = xpub.ChildAt(index: 1);
Console.WriteLine(address1);
Console.WriteLine(address2);
```

