[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Transactions

transactions are the meat and bones of bitcoin and will take up a fair bit in this tutorial. But first, let's look at what value transfers in money look like historically, and how bitcoin can reproduce similar properties of this millennia old technology.

## A bit of history

Money can be though as the *most tradable good in a society*. Historically, money has taken many forms such as seashells, salt, gold coins or even limestone rocks.

Conceptually, Bitcoin deals with transactions in similar aspecs that money has been traded for thousands of years, but with some striking uniqueness. One commonality in trade is trust in the currency being used to buy or sell a good or service. This is vital for trade to flourish; money provides a base unit of measurement and when implemented in society it comes to measure value in a highly accurate way. 

Historically, governments or those providing protection services have often created their own currency which mediates this trust. If people pay their taxes in the currency, and know it is enforced by government, they may be more inclined to accept it, and this idea becomes a self fulfilling prophecy. 

*One point to make here is that money doesn't emerge from government, but often governments have enforced the use of the currency by fiat (Latin for decreed).* 

So much so was this the case in China in the 12th century and in other societies that refusal to use the currency resulted in a death penalty. Furthermore, mediation by a third party comes with a liability: we now have to trust the money creators not to deceive its users, and to create rules or conditions that are fair and equitable. An example of this is when a government can debase their currency by creating more of it at a cheaper or even zero cost. This might be in the form of removing gold from coins and replacing with base metals - making coins smaller. A similar result can be achieved by printing more paper money that in gold reserves held by banks. Perhaps it is by just printing money to buy government debt (and earn interest on it) via a central bank like today. 

If the the issuer of currency is honest, they will not print money for themselves, as this provides no actual value, only dilutes other holders' purchasing power. However, history has proven the temptation of inflating the money supply all too much for most. People including the creator of bitcoin, Satoshi Nakamoto, recognised this attack vector that can be exploited by the money creator and rebuilt this trust model so as not to rely on a central third party. This was technologically possible with the use of cryptographic math, computer networking and falling cost of electronic equipment to create many hardware nodes.

The model of a centralised database to store transactions was replaced by a computer network of distributed nodes around the world that could verify transactions for financial incentives. They could prove to the whole world that the transaction was valid, using asymmetric key cryptography. Due to the fact that there are many independent verifyers, no one could trust another but presumed that the majority were honest (because it paid better!) hence trusted the network. As an added bonus, this replaces identity at the cryptographic level, so you don't have to have a panopticon of financial surveillance requiring your birth certificate and proof that you're not an extended cousin of an Iranian expat. 

Such is the chaos that bitcoin was envisioned and brought into existence.

## Enough with the history, let's get to the meat of it!

Let's define what a transaction in a bitcoin is. A transaction consists of 4 parts:

- Version
- Inputs
- Outputs
- Locktime

The version is obvious, most transactions use version 1, although version 2 can also be seen for specific lock times.

The lock time is just the amount of time funds have to be locked up before they can be spent. Miners for example cannot spend funds immediately, as we shall see later, so they have to wait a while before they can spend their mining rewards. It's important to note here that time is a little ambiguous, since bitcoin uses its own time measured in blocks. It can also use a Unix timestamp, but again more on this later.

The inputs and outputs can be thought of as a lock and key model. Inputs are the key: they contain the signatures defining which unspent outputs we want to spend (inputs contain outputs from previous transactions). The output is like the lockbox, we define where we are sending the money and the conditions in which it can be spend for future transactions. In most cases we tie it to a hash of the public key, then only the private key of that public key can spend the money.

## Inputs

Let's take a closer look at what a transaction input is. Before, we said that an input is like a key that can prove we can spend the coins (while specifying exactly the money we want to move). Precisely, an input consists of:

- transaction id
- transaction index (vout)
- scriptsig/unlocking script
- sequence

Let's run through what each of these are:

A transaction id is how we specify which transaction we are interested in. It is the hash of the previous transaction. 

However, there may have been many outputs in this transaction. We also need a way to index which output we are interested in. This is what the transaction index is for. 

The scriptsig, or unlocking script, is how we prove ownership. This used to consist of just the signature, but was later changed to include the public key. This is because outputs used to be linked to a public key. However these took up a larger amount of space on the block chain, so instead the public key was hashed. Clearly I can't get the public key with the hash, so it became necessary for the spender to provide their public key. After proving it created an identical hash, it is used along with the signature to verify the message.

The sequence was originally what Satoshi called high frequency trades, and was intended to be used for off chain, low-cost payments. However, this idea was problematic since there was no way to enforce miners to be honest. It's now used for flags such as RBF (replace by fee) for bumping up transaction fees after they have been broadcasted. Accidently setting an extremely low fee can be problematic and can leave a transactions unverified for days or even weeks! There is little incentive for miners to include a transaction with a low fee in their block (it's also a mistake every bitcoiner makes at least once!).

## Outputs

Outputs are perhaps a little simpler, and consists of only 2 parts:

- Scriptpubkey
- Amount

The amount is fairly easy, it's just an unsigned 8 byte value of the amount we want to spend in satoshis. Interestingly, it wasn't always an unsigned value. Back in the early days, someone tried their luck by creating a negative output value. Because of how this was interpreted, the person managed to give themselves 80 billion bitcoin! This is problematic given that there are only 21 million bitcoin that can exist! After a block re-org this was subsequently changed to only be a positive value. 

The scriptpubkey is the spending condition for how the coins can be spent. This mostly consists of a hash of the public key. It can also be the hash of a script for more complex spending conditions, as we'll see later. 

## Constructing a transaction

It's now time to code our own transaction, which we will broadcast and, as proof, view it on a block explorer to check it has confirmed. Before we start, you will need some testnet bitcoin. Testnet bitcoin has no monetary value but works is almost every single way as bitcoin, so is suitable for testing.

**It's not recommended to use real bitcoin when testing your software application, as it can lead to financial loss**

To get testnet bitcoin you can visit a faucet such as [add faucet address]. You'll also need a testnet bitcoin wallet. Blockstream's Green wallet allows you to add one.

Next, let's create a testnet address and the corresponding private key:

```
var rand = csrng.genKey();
PrivateKey pk = new PrivateKey(rand);
PublicKey pubKey = pk.pubKey();
BitcoinAddress addr = pubKey.getAddr(AddressType.nativeSegwit, testnet: true);
Console.WriteLine(addr);
Console.WriteLine(pk);
//Output:
//tb1qh2a3uw97qnqxastzru00p70glx368djlq0n8x3
//L1vkcXRfeY3g6gPdWXvFyiPjLUzjuD9s26Nv7EaRE7C9JEPuxoJk
```

[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

