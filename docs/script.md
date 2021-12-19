## Script 

Bitcoin has a scripting language called Script (it never really got given a name!) which defines spending conditions for transactions. An important aspect of bitcoin's scripting language is that it is Turing incomplete, meaning it comes without loops. Rather, it's a stack based language that will have a guaranteed finish. This is important since nodes are executing any operations that the spender defines. If there is an infinite loop somewhere, it could DDOS the entire bitcoin network!

In the last chapter, we said that a transaction has inputs and outputs, and therein lies scriptsigs and scriptpubkeys respectively. As the name suggests, these are both scripts that contain commands.

## Scriptsig

Scriptsigs usually contain only two items: a signature and public key.

```
3045af45e29f850bc41cc3df2
025845838487372272723738
```

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

## Combining scripts

So we have the script sig, which the spender provides to try to redeem the coins. We also have the preexisting scriptpubkey, as part of the output from the last transaction. When we combine them, it looks like this:

```
<sig>
<pubkey>
Op_Dup
Op_Hash160
<hash of pubkey>
Op_Equalverify
Op_Checksig
```
If you remember we said it was a stack-based language, so we simply execute each command, and any data element we place on the stack:

```
Stack        Current command   
<sig>      | Add signature
-------------------

Stack        Current command
<Pubkey>   | Add pubkey
<sig>      |
-------------------

Stack        Current command
<Pubkey>   | Op_dup (duplicate) 
<Pubkey>   |
<sig>      |
-------------------

Stack         Current command
<pubkeyhash>| Op_hash160
<pubkey>    |
<sig>       |
--------------------

Stack         Current command
<pubkeyhash>| Add pubkey hash
<pubkeyhash>|
<pubkey>.   |
<sig>.      |
---------------------

Op_equalverify is like 2 commands in 1: 

add 1 to the stack if items are equal

Stack        Current command
1          | Op_Equal
<pubkey>   |
<sig>      |
---------------------

verify that 1 is on the stack

Stack        Current command
<pubkey>   | Op_Verify
<sig>      |
----------------------

Stack        Current command
1          | Op_Checksig

```

If 1 is on the stack, it means the signature is valid. Yay! We can spend the coins! 

## Scripting out own transactions

