[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Wallets

What we have so far is a way to generate a single address and the potential to spend from that address using a digital signature (note we will go over signing fully in a later chapter). However, this is not very private, since a person has a single public key and if their identity is linked to that public key, anyone can tell what they are spending money on (since all transactions are recorded on a public ledger). We also don't have any accounting capabilities. Perhaps we want to separate our funds into three accounts, one for rent, one for investments and another for recreation. We cannot do this using single key pair without having multiple keys, which requires extra backup overheads. 

In this chapter, we will look at how to solve this with a data structure called a hierarchical deterministic tree, which is the basis for all modern wallets. This allows us to have many keys and addresses that can be derived from a single root private and public key. It will also be necessary to make an easy back up of this key, which we will also discuss in this chapter. If you Remember, the private key is just a 256-bit number and not very human readable (try writing down 32 hex numbers and you will know what I mean!). This is where the idea of a mnemonic comes in - a direct mapping of the random entropy into human readable words, which can be later translated into the extended key or root of the tree.

Since we are already getting ahead of ourselves, let's look at the code to do this, then break it apart. 

```c#
using LBitcoin;
...

Wordlist wordlist = new Wordlist();
Mnemonic mnemonic = new Mnemonic(wordlist, 12, "password123");
Console.WriteLine(mnemonic);

HDPrivateKey extKey = new HDPrivateKey(mnemonic);
Console.WriteLine(extKey);
HDPublicKey xpub = extKey.Neuter();
Console.WriteLine(xpub);

var pub1 = xpub.ChildAt(path: "m84/0'/0'/0/0");
var pub2 = xpub.ChildAt(index: 1);
Console.WriteLine(new BitcoinAddress(pub1));
Console.WriteLine(new BitcoinAddress(pub2));
```

**What we did**

- Defined our wordlist (some wallet applications might want a wordlist in a specific language, although this comes with the risk that other wallets may not support the language, and fail to show funds).
- Created a set of 12 words using our wordlist, and encrypt it with a very secure password: "password123"
- Created an extended key from these words (Remember the words just represent random entropy, which is used to make the key)
- Derived a public key from the private key
- Both pairs of extended keys can now derive children keys.
- Derived children are also extended public/private keys, and can be converted to addresses

**What is an extended key?**
Put simply, an extended key, also known as an xpriv, is a single private key that can be used to generate all private keys in the tree. There is also an extended public key (also known as xpub) which can generate all the child public keys. 

Note that extended private keys can derive both the child private key and public key, but extended public keys (xpub) can only derive public keys. This may be very useful, for example if you are running a Web server and want to generate fresh addresses, while storing the funds on an offline hardware wallet, this is now possible.

## Creating the mnemonic 

The mnemonic is a set of words that relate to the random entropy that seeds a key. Mnemonics can be in different sizes, but mostly appear either as 12 or 24 words. In our example, we will show how a 12 word mnemonic is generated, using 128 bits of random entropy. It is very important that the entropy is genrated securely by a cryptographically secure random number generator. Seeding entropy based on time-based components in the underlying hardware is usually insufficient by itself. Taking randomness from flipping a coin, mouse input or background light and sound can be effective ways to ensure sufficient random entropy for a key. 

**Steps (for a 12-word mnemonic)**

- Generate 128 bits of random entropy from a cryptographically secure source
- hash the entropy with sha256 and add the first **4** bits to the end of the entropy
- Divide the 132 bits into 12 equal segments of 11 bits
- Using an ordered wordlist of 2048 words (as defined in bip39) map the binary value to the index of the wordlist

![creating the mnemonic](/assets/mnemonic_generation.png)

**Security points to note**

- The security of the mnemonic is weakened by a factor of 2^2048 for each word removed
- The last word is not derived from the random entropy but from the checksum
- Research suggests that 24 words doesn't add as much random entropy as intuitively expected, many wallets opt for 12 words since they are easier to backup

With these in mind, it is very important not to reduce the number of mnemonic words to less than 12. For example splitting 24 words into 3 groups of 16 words (a redundant set in each) in an attempt to mimic multisig capabilities is **not** secure and should never be attempted. 

In the above example, should an adversary gain access to one set, in theory they cannot steal funds because they are missing 8 words. However, if the set they are missing contains that last word (which we know is the checksum word) then the adversary would only need to brute fore 7 words. Given current hardware capabilities, this is still a challenge, and would take multiple years to crack. Despite this, it's obvious that this scheme doesn't come close to matching the security of something like a multisig scheme

## Mnemonic to extended key

The mnemonic now needs to be converted to a private key. We also need something called a chaincode. This will be used to derive children, as we will soon see. 

**Steps**

- Hash the mnemonic using HMAC512 algorithm.
- The passphrase is added as a key before hashing. This will act as a salt and make it impossible to get the master key from just the mnemonic 
- Perform 2048 iterations of this process
- The result is hashed again with HMAC512 with the password "Bitcoin seed" (see [bip32](https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki))
- The left 256 bits of the 512 bit output will be used as the private key
- The right 256 bits is the chaincode

![mnemonic to extended key](/assets/mnemonic_to_seed.png)

**Why HMAC512?**

HMAC gives us an adequate number of bits for the child private key and chain code (by doubling the size). It is also quite a slow algorithm, and many rounds are applied (2048) before getting the key. This makes it harder for an attacker to brute force the mnemonic. 

## Deriving a child private key from an extended key

To derive a child private key we first need an index. Each key can have 2^8 (256) children, however only half of that will be used for standard derivation as we'll soon see. 

**The steps**

- Append index to parent private key (also prepend a 1-byte padding of 0x00 to the key)
- HMAC256 using the parent chaincode as the key
- The right 256 bits is the child chaincode 
- The left 256 bits is added to the parent private key in the field of **N**
- Notation for this is cpk = L|HMAC(ppk, pcc) + ppk, ccc = R|HMAC(ppk, pcc) 

![Deriving child private key](/assets/nonhardened_derivation.png)

## Deriving child public key from xpub

A similar process is used for deriving the child public key from the xpub:

- Append index to parent public key
- HMAC using the parent chain code as the key
- the right 256 bits is the child chaincode 
- The base point (G) of secp256k1 is multiplied by the left 256 bits (in the field of P)
- this is added to the parent point
- The result is our child public key

![Deriving child public key](/assets/derive_pubkey.png)

## Hardened key derivation

What we have so far is great, but there is a big security risk if we were to only use this type of derivation. This is because if one of the child private keys get leaked, it's possible to deduce all its child private keys. Worse, because an attacker could obtain the chain code and public key from the xpub, they can also deduce the parent key! This leads us to create an alternative derivation function called hardened derivation. This is where the private key is used to derive the child key, instead of the public key. The process is shown below. 

**Uses of hardened key derivation?**

Because a public key cannot derive a hardened key (public keys cannot derive private keys) it's not useful for use with an xpub. However we can derive an xpub from a hardened key which mitigates the risk described above; the public key cannot be used in conjunction with the chain code to deduce any private keys. 

![hardened key derivation](/assets/hardened_derivation.png)

## Bip44

The process we have been describing so far is a specification known as bip32. However this is very general, and not very standardised. What if, for example, different wallets used different derivation paths? There are billions of possible nodes where funds might be stored, it may be possible for money to get lost, even if we have the correct mnemonic! 

Bip44 was a specification to solve this, by creating standardised uses for each chain:

**Levels of the tree**

- Level 0 is the master node
- Level 1 is used for the type - e.g. m/84 is native segwit
- Level 2 is used for the "purpose" or type of coin (litecoin, bitcoin test et etc.) 
- Level 3 is used for the account branch
- Level 4 is used for receiving change (more on change addresses in transactions) 
- Level 5 is the incremental index for each address

**Examples**

M/44/0'/2'/0/0 is the third account and first address (bitcoin)
M/44/1'/0'/1/0 is the first change address in the first account (testnet bitcoin)

Note that:
- The first 2 levels are hardened for reasons discussed above
- M is the master node (root key)

## bip84 & 49

Bip84 used the same structure as bip44 but is for segwit addresses. Bip49 is for segwit script addresses.

For example,

M/84'/0'/0'/1/0
M/49'/0'/1'/0/0

## Summary

This was quite a dense chapter, so we will summarise briefly the main components:

- All major wallets are 'HD wallets' that use a master private and public key to derive all other keys
- The standard for this is called Bip32, however this is quite a broad specification and was later known as [bip44](https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki), [bip49](https://github.com/bitcoin/bips/blob/master/bip-0049.mediawiki), and [bip84](https://github.com/bitcoin/bips/blob/master/bip-0084.mediawiki) for legacy address schemes, nested segwit and segwit respectively.
- A mnemonic is just random entropy converted into a human-readable format
- An extended key (xpriv and xpub) have a chaincode which is used to derive child addresses
- Hardened key derivation is used to prevent an attacker using a child key to derive a parent key. It's especially useful for deriving an account xpub, for example for use on a web server
- The derivation path, part of the [Bip44](https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki) standard, uses the format m/Type/purpose/account/change/address
- The first 3 levels usually by default derive by hardened derivation, denoted by a ' in the derivation paths 

[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

