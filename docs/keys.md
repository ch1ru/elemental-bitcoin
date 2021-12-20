[/Intro](index.md)|[/Install](install.md)|[/keys](keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Keys

Ownership in bitcoin, or rather the ability to redeem coins, comes from public and private keys. The public key, what you give to the world, is how people can pay you. The private key is used to spend coins associated with a public key. This is distinctly different from a bank, where to spend money you have to prove to them your identity and that you can produce the secret associated with your account - ie your pin or card number. However the way this is done requires the spender to transmit their secret to the bank over a secure channel. Although the data is encrypted, it still leaves an attack vector for the secret to be intercepted, not to mention that the bank owns the exclusive right to do whatever they want with your money.

With bitcoin, the secret key, a 256-bit number can prove an identity via signing a transaction. When this signature is verified, it can be assertained that the person wishing to spend the coins is the rightful owner. Note how once the transaction is signed, the signature can be broadcasted to the network unencrypted since there is no information such as personal details linked to the transaction. Or more importantly, there is no private data that can compromise the security of one's coins. The private key can remain on your device such as a phone or offline hardware wallet.

So let's see how someone can generate their own bitcoin address.

*Note: For the rest of this chapter when we say key we will refer to the private key*

The code:
```c#
var rand = csrng.genKey();
PrivateKey pk = new PrivateKey(rand);
PublicKey pubKey = pk.pubKey();
BitcoinAddress addr = pubKey.getAddr();
Console.WriteLine(addr);

//output: bc1qvmpf6cgqrsntk5mndduqaqfgwcf6thv4qdc8nq
```

Let's break down what we did:
- Created some random entropy
- Created a private key (32 byte number) from that entropy
- Derived a public key from that private key through elliptic curve key generation (we will discuss how this works in much more detail in the next chapter. For now, just be aware that the public key is a coordinate on an elliptic curve (y^2 = x^3 + 7 mod p)
- Converted the public key to a human readable address by double hashing it with sha256 then ripemd160 to get the public key hash, then encoding it using a format called Bech32 to make it more readable.
- This address can be easily encoded in a qr code and acts as an invoice for a payee

![Public key to address](/assets/pubkeytoaddress.png)

[/Intro](index.md)|[/Install](install.md)|[/keys](keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
