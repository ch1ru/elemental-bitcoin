[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Simplified Payment Verification

When we spend or receive bitcoin, knowing that our payment has been verified by miners is important, especially if it is a large value transfer. This can be done using full nodes that validate all transactions and keep a full record of the block chain. We can check if our transaction is included in a block and calculate the block ID to make sure it is valid. However what if we wanted to do the same with low hardware or network resources, such as on a mobile phone? 

This is achieved using a method called simplified payment verification, or SPV for short. By using inclusion proofs by means of a merkle tree, it's possible to know if a transaction is included in a block with a very high degree of certainty. 

![Merkle tree inclusion](/assets/merkleinclusion.png)

[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
