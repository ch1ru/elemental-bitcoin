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

var pub1 = xpub.ChildAt(path: "m84/0'/0'/0/0");
var pub2 = xpub.ChildAt(index: 1);
Console.WriteLine(new BitcoinAddress(pub1));
Console.WriteLine(new BitcoinAddress(pub2));
```

## What we did

- Defined our wordlist (some wallet applications might want a wordlist in a specific language, although this comes with the risk that wallets may not support the language, and fail to show funds).
- Created a set of 12 words using our wordlist, and encrypt it with a very secure password
- Created an extended key from these words (Remember the words just represent random entropy, which all private keys need)
- Derived a public key from the private key
- Both pairs of extended keys can now derive children keys.
- Derived children are also extended public/private keys, and can be converted to addresses

![bip32 derivation diagram](/images/bitcoin.png)
Note that extended private keys can derive both the child private key and public key, but extended public keys (xpub) can only derive public keys. This may be very useful, for example if you are running a Web server and want to generate a fresh address, while storing the funds on an offline hardware wallet, this is now possible.

