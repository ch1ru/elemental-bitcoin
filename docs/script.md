[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Script 

Bitcoin has a scripting language called Script (it never really got given a name!) which defines spending conditions for transactions. An important aspect of bitcoin's scripting language is that it is Turing incomplete, meaning it comes without loops. Rather, it's a stack based language that will have a guaranteed finish. This is important since nodes are executing any operations that the spender defines. If there is an infinite loop somewhere, it could DDOS the entire bitcoin network!

In the last chapter, we said that a transaction has inputs and outputs, and therein lies scriptsigs and scriptpubkeys respectively. As the name suggests, these are both scripts that contain commands. We will look at the most common type of scriptsigs and scriptpubkeys found in p2pkh (pay-to-pubkey-hash) transactions.

## Scriptsig

Scriptsigs usually contain only two items: a signature and public key.

```
3045022100f5386ba7fb3562a729b225be9474ad8abac10929c07c8c779bdb27ddf7d64ba90220244fe1e912481a1711a5a048b59b73a4c3961121edf040b81152c7729b6507dd0121 //signature
035ca5c197fb8646d4871d7e930a1b7085f9406c44532fecfb3964d7f70d7c4bb3 //public key
```

In segwit transaction, the scriptsig is a null-byte (0x00). There is also a segwit marker at the start of the transaction. This is because in segwit, the scriptsig (witness data) is not used in the ID caluculated (remember the txid is the transaction hash). This is to prevent malleability of transactions, which elliptic curve signatures are vulnerable to. To find out more about transaction malleability in elliptic curves, see this [article]. Instead there is a witness field outside the transaction, which contains the scriptsig:

```
Version 2
Segwit: true
Number of inputs: 1
        Input 0
                txid d989dd24e98e74bf24436b24c1910c61389de48b42feffe9ff136df1bc6a7819
                txindex 1
                Scriptsig: (none)
                Witness: 30440220217bb0c181abb71b09e80577664be0bcba6903edb2969a6589e4b2c07c41d8f8022020855ecceb590430cffa3748b1eae97f523697423dc22ba1f5df901de2ab91c40102d4eef825ea8bf139a81a264faff3a45b5ce2d388a4b913e0d11aa9ae0f8f9ba8
                Sequence feffffff
```

You can see that the format of the witness data is exactly the same as the scriptsig in legacy transactions, just in a different place.

## Scriptpubkey

The scriptpubkey will contain the hash of the public key. When a spender tries to redeem coins, they have to prove that the public key they provide in the scriptsig hashes to the hash in the scriptsig, then verify that the signature is valid using that public key.

But how does it know how to do all this? That's where the commands come in. Most scriptpubkeys for standard outputs will look something like this:

```
Op_Dup
Op_Hash160
<20 byte hash of pubkey>
Op_Equalverify
Op_Checksig
```

It's OK if this seems a little confusing, it should make a bit more sense once we combine the 2 scripts!

**Also note: Segwit scriptpubkeys look a little different. They only have 2 items: OP_0 and the pubkey hash. This is due to the segwit softfork. Segwit nodes will recognise this scriptpubkey pattern and replace it with the same script as above. So don't be surprised if the scriptpubkey in segwit transactions looks different in block explorers.**

## Combining scripts

So we have the script sig, which the spender provides to redeem the coins. We also have the preexisting scriptpubkey, as part of the output from the last transaction. When we combine them, it looks like this:

```
<sig>
<pubkey>
Op_Dup
Op_Hash160
<hash of pubkey>
Op_Equalverify
Op_Checksig
```
If you remember we said it was a stack-based language, so we simply execute each command, and any data element we place on the stack. Let's execute this one command at a time:

\<sig\>
```
Stack        Current command   
<sig>      | Add signature
-------------------
```

\<pubkey\>
```
Stack        Current command
<Pubkey>   | Add pubkey
<sig>      |
-------------------
```

OP_Dup (duplicate top item)
 ```
Stack        Current command
<Pubkey>   | Op_dup (duplicate) 
<Pubkey>   |
<sig>      |
-------------------
  ```
  
OP_Hash160 (hash160 the top item)
```
Stack         Current command
<pubkeyhash>| Op_hash160
<pubkey>    |
<sig>       |
--------------------
```
  
\<pubkeyhash\>
```
Stack         Current command
<pubkeyhash>| Add pubkey hash
<pubkeyhash>|
<pubkey>.   |
<sig>.      |
---------------------
 ```

OP_equalverify is 2 commands in 1: 

OP_Equal (add 1 to the stack if items are equal)
```
Stack        Current command
1          | Op_Equal
<pubkey>   |
<sig>      |
---------------------
```

OP_verify (check that that 1 is on the stack)
```
Stack        Current command
<pubkey>   | Op_Verify
<sig>      |
----------------------

Stack        Current command
1          | Op_Checksig

```

If 1 is on the stack, it means the signature is valid. Yay! We can spend the coins! 

## Coding this in LBitcoin

We already coded our own transactions in the last chapter, but most of it was done automatically for us. Let's do the same thing manually:

To make a scriptsig:
```c#
using LBitcoin;
using LBitcoin.Ecc;
...

Script scriptsig = new Script();
scriptsig.Add(sig.DerEncode()); //signature in byte format
scriptsig.Add(pubKey.Compressed); //public key in SEC compressed format
```

To make the scriptpubkey:
```c#
Script scriptpubkey = new Script();
scriptpubkey.Add(opcodes.OP_DUP);
scriptpubkey.Add(opcodes.OP_HASH160);
scriptpubkey.Add(Hash.hash160(pubKey.Compressed));
scriptpubkey.Add(opcodes.OP_EQUALVERIFY);
scriptpubkey.Add(opcodes.OP_CHECKSIG);
```

The opcode commands are just a single-byte representation. For example:

OP_Dup = 0x76
OP_Hash160 = 0xa9
OP_EqualVerify = 0x88
OP_Checksig = 0xac

## Scripting our own spending conditions

What we have covered so far is sufficient for simple spending conditions, suitable for the majority of cases. But perhaps we want to add some complexity, for example create a 2/3 multisig solution. This way, even if 1 of the keys gets compromised, the funds still stay secure. This is useful for large amounts, or if there are multiple parties involved and we want to create spending conditions suited to their trust relationships. We have to go a bit further for this as our current method of single keys and static scripts is not enough. We must script our *own* spending conditions using a feature called P2SH (pay-to-script-hash). 

## Creating a 2/3 multisig

Let's say we want to do the above and create a bitcoin output that can only be spend with 2 out of 3 possible keys. To do this let's first create the keys:

```c#
var rand = Csrng.GenKey();
PrivateKey pk1 = new PrivateKey(rand);
PublicKey pub1 = pk.pubKey();

rand = Csrng.GenKey();
PrivateKey pk2 = new PrivateKey(rand);
PublicKey pub2 = pk.pubKey();

rand = Csrng.GenKey();
PrivateKey pk3 = new PrivateKey(rand);
PublicKey pub3 = pk.pubKey();
```

Now let's create the redeem script:

```c#
PublicKey[] pubkeys = new PublicKey[] { pub1, pub2, pub3 };
Script redeemScript = Script.CreateMultisigRedeemScript(2, 3, pubkeys);

//OR we can create it manually

Script redeemScript = new Script();
redeemScript.Add((byte)2);
redeemScript.Add(pub1.Compressed);
redeemScript.Add(pub2.Compressed);
redeemScript.Add(pub3.Compressed);
redeemScript.Add((byte)3);
redeemScript.Add(Opcodes.OP_CHECKMULTISIG);
```

For the scriptpubkey, we provide a hash of the script. This is why the transaction type is called pay-to-script-hash.

```c#
byte[] scriptHash = Hash.hash160(redeemScript.Serialise());
Script scriptpubkey = Script.P2SH(scriptHash);

//OR manually

byte[] scriptHash = Hash.hash160(redeemScript.Serialise());
Script scriptpubkey = new Script();
scriptpubkey.Add(Opcodes.OP_HASH160);
scriptpubkey.Add(scriptHash);
scriptpubkey.Add(Opcodes.OP_EQUAL);
```
When we send bitcoin to the corresponding address with this hash, it will be locked with a multisig contract. Providing 2/3 keys is needed to spend the funds.

In the pay-to-pubkey script type, we have to provide the public key, since it's not possible to reverse the hash to return to the public key. In a similar way, we need to provide the redeem script corresponding to the script hash in our scriptsig. Let's create this now:

```c#
fixme: show how to get signatures
Script scriptsig = new Script();
scriptsig.Add(Opcodes.OP_0);
scriptsig.Add(sig1.DerEncode());
scriptsig.Add(sig2.DerEncode());
redeemScript.Serialise();
```

You may be wondering why we add the OP_0 opcode at the start? This is because of an overflow bug in the checkmultisig operation that consumes one more element than it should. This has existed since the very first multisig transaction but went unnoticed. At this point, it would require a hardfork to fix, which is why we just add a null byte instead.


## Putting it all together

Let's walk through how a p2sh transaction is processed on the stack. Our combined script now looks something like this:

OP_0
\<sig1\>
\<sig2\>
Redeem script
OP_Hash160
\<script hash\>
OP_Equal

Let's execute it on the stack:

Add OP_0:
```
Stack         Current command
OP_0        | Add null byte
---------------------
```

Add \<sig1\>:
```
Stack         Current command
\<sig1\>    | Add first signature
OP_0        | 
---------------------
```

Add \<sig2\>:
```
Stack         Current command
\<sig2\>    | Add second signature
\<sig1\>    | 
OP_0        | 
---------------------
```

Add redeem script:
```
Stack          Current command
redeem script| \<redeem script\>
\<sig2\>     | 
\<sig1\>     | 
OP_0         | 
---------------------
```

Hash the top element:
```
Stack          Current command
script hash  | OP_Hash160
\<sig2\>     | 
\<sig1\>     | 
OP_0         | 
---------------------
```

Add script hash:
```
Stack          Current command
script hash  | \<script hash\>
script hash  | 
\<sig2\>     | 
\<sig1\>     | 
OP_0         | 
---------------------
```

Check top elements are equal, 1 if true:
```
Stack          Current command
1            | OP_Equal
\<sig2\>     | 
\<sig1\>     | 
OP_0         | 
---------------------
```

At this point, nodes will recognise the pattern of 1, sig, sig, 0 as the bip16 special rule. This essentially means: the potential spender provided the correct script corresponding to the hash, now proceed to validate the redeem script. 

This is the fun part! We place the redeem script on the stack to be executed. This is because we know this is the correct spending conditions provided by the original depositer of funds. If the hash of the script did not match the original hash, OP_Equal would place a 0 instead of a 1, and the script would fail.

Our stack now looks something like this:
```
OP_Checkmultisig
OP_3
\<pubkey3\>
\<pubkey2\>
\<pubkey1\>
OP_2
\<sig2\>
\<sig1\>
OP_0
```

The top 5 elements form our redeem script (notice it's reversed after placing it on the stack), the remaining items are the remaining signatures we need to verify. All we need to do now is make sure both signatures are valid using the corresponding public keys. Notice that there is a total 3 public keys. Any 2 may be used to validate signatures (hence 2 out of 3). We can execute the whole thing in one command, giving us a final value of 1 if the signatures are valid, or 0 if they are invalid (or if not enough signatures are valid to meet the spending condition of OP_M of OP_N). The OP_0 is consumed during the OP_Checkmultisig operation - the bug we mentioned earlier.

## Summary

- Script is a stack-based language for specifying spending conditions in bitcoin
- It is turing incomplete (no possibility for loops) to prevent DDOS attacks occuring on the network
- The scriptsig in most transactions contain a signature and public key
- In segwit transactions the witness data is separate from the scriptsig field
- The scriptpubkey contains the public key hash, and opcodes to check if the public key matches the hash, and the signatures are valid
- We can specify our own spending conditions using p2sh, called a redeem script
- The redeem script needs to be including in the scriptsig (or in the witness data for segwit transactions)
-  The redeem script will contain the public keys of possible spending keys
-  If the OP_Checkmultisig opcode is used (for p2sh multisig scripts) there has to be one additional element (0x00) because of the overflow bug

Hopefully you understand more about scripting. In the next chapter, we will look at how blocks are constructed and interconnected in the bitcoin blockchain.


[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
