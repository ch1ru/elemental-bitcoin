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

- Defined our wordlist (some wallet applications might want a wordlist in a specific language, although this comes with the risk that other wallets may not support the language, and fail to show funds).
- Created a set of 12 words using our wordlist, and encrypt it with a very secure password
- Created an extended key from these words (Remember the words just represent random entropy, which all private keys need)
- Derived a public key from the private key
- Both pairs of extended keys can now derive children keys.
- Derived children are also extended public/private keys, and can be converted to addresses

![bip32 derivation diagram](/images/bitcoin.png)

Note that extended private keys can derive both the child private key and public key, but extended public keys (xpub) can only derive public keys. This may be very useful, for example if you are running a Web server and want to generate a fresh address, while storing the funds on an offline hardware wallet, this is now possible.

## Creating the mnemonic 

The mnemonic is a set of words that relate to the random entropy that seeds a key. Mnemonics can be in different sizes, but mostly appear either as 12 or 24 words. In our example, we will show how a 12 word mnemonic is generated, using 128 bits of random entropy. 

**Steps**

- Generate 128 bits of random entropy from a cryptographically secure source
- hash the entropy with sha256 and add the first **4** bits to the end of the entropy
- Divide the 132 bits into 12 equal segments of 11 bits
- Using an ordered wordlist of 2048 words (as defined in bip39) map the binary value to the index of the wordlist

**Security points to note**

- The security of the mnemonic is weakened by a factor of 2^2048 for each word removed
- The last word is not derived from the random entropy but from the checksum
- Recent research shows that 24 words doesn't add as much random entropy as intuitively expected

With these in mind, it is very important not to reduce the number of mnemonic words to less than 12. For example splitting 24 words into 3 groups of 16 words, with a redundant set in each, in an attempt to mimic multisig capabilities is **not** secure and should never be attempted. 

In the above example, should an adversary gain access to one set, in theory they cannot steal funds because they are missing 8 words. However, if they set they are missing contains that last word (which we know is the checksum word) then the adversary would only need to brute fore 7 words. Given current hardware capabilities, this is still a challenge, and would take multiple years to crack. Despite this, it's obvious that this scheme doesn't come close to matching the security of something like a multisig scheme

## Entropy to extended key

The entropy now needs to be converted to a private key. We also need something called a chaincode. This will be used to derive children, as we will soon see. 

**Steps**

- Hash the entropy using HMAC512 algorithm. This is known as a key stretching function, since it turns 256 bits into 512 bits
- The passphrase is added as a key before hashing. This will act as a salt and make it impossible to get the master key from just the mnemonic 
- The left 256 bits of the 512 bit output will be used as the private key
- The right 256 bits is the chaincode

**Why HMAC512?**

HMAC gives us an adequate number of bits for the child private key and chain code (by doubling the size). It is also quite a slow algorithm, and many rounds are applied (1024) before getting the key. This makes it harder for an attacker to brute force the mnemonic. 

## Deriving child private key from extended key

## Deriving child public key from xpub

## Hardened key derivation

**Uses of hardened key derivation?**

## Bip44


