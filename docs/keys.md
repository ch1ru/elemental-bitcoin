# Keys


Ownership in bitcoin, or rather the ability to redeem coins, comes from public and private keys. The public key, what you give to the world, is how people can pay you. The private key is used to redeem coins associated with a public key. Unlike the case with a bank, where they own the exclusive property of your money, and the only way to spend it it to prove to them that you know a secret - ie your pin or card number. However the way this is done requires the spender to transmit their secret to the bank over a secure channel. Although the data is encrypted, it still leaves an attack vector for the secret to be intercepted, not to mention that the bank owns the exclusive right to do whatever they want with your money.

With bitcoin, the secret key, a 256-bit number can prove an identity via signing a message containing the transaction. When this signature is verified, it can be assertained that the person wishing to spend the coins is the rightful owner. Note how once the transaction is signed, the signature can be broadcasted to the network unencrypted since there is no information such as personal details linked to the transaction, or more importantly any private data that can compromise the security of one's coins. The private key can remain on your device such as a phone or hardware wallet.

So let's see how someone can send bitcoin, then subsequently spend from that address.
*Note: For the rest of this chapter when we say key we will refer to the private key*

```
var rand = csrng.randomInt(32);
PrivateKey pk = new PrivateKey(rand);
PublicKey pubKey = pk.pubKey();
BitcoinAddress addr = pubKey.getAddr();
Console.WriteLine(addr);

//output: bc1qvmpf6cgqrsntk5mndduqaqfgwcf6thv4qdc8nq
```

Let's break down what we did:
- Created some random entropy
- Create a private key (32 byte number) from that entropy
- Derived a public key from that private key through elliptic curve key generation (we will discuss how this works in much more detail in the next chapter. For now, just be aware that the public key is a coordinate on an elliptic curve (y^2 = x^3 + 7 mod p)
- Converted the public key to a human readable address by hashing it with sha256 then ripemd160 to get the public key hash, then encoding it using a format called Bech32 to make it more readable.
- This address can be easily encoded in a qr code and acts as an invoice for a payee

![Public key to address](/images/pubkeytoaddress.png)


