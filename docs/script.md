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

We already 

## Scripting our own transactions



[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
