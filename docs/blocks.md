[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Blocks

A block is a data structure used for storing bitcoin transactions and the principle is used in many other cryptocurrencies. In the next chapter we will look at mining and how chaining the blocks together into a 'blockchain' gives it the properties of an immutable ledger. For now, we will just look at how an indivudual block is constructed. 

## Block headers

The block header is a sub-section of a block that contains useful information about its contents. These include:

- Version
- Timestamp
- Nonce
- Merkle root
- Previous block hash
- Bits (used for difficulty calculation)

If you are unsure of some of these, we will cover the nonce, previous hash and difficulty in the [Mining](/mining.md) chapter and why they are important. 

## Calculating the block ID/hash

The block hash is calculated by the miners in a specific way, however it must include the header bytes we just mentioned. Parameters such as the nonce can be changed to give a 'correct' ID that is below a certain value, dictated by the current difficulty. 

The byte values are appended together, and a potential block hash is the double sha256 digest of this input. I say potential block hash, or candidate hash, because in all liklihood it will not meet the diffulty parameters in the mining algorthm. Miners will compete to find the block hash that is low enough that it falls into the correct margin. 

## Merkle trees

We haven't yet mentioned what the merkle root is. For the rest of the chapter, we will talk about merkle trees and how they are an important data structure used in blocks. 

Merkle trees provide a way to maintain data integrity inside a block. The data in this case are the bitcoin transactions that make up the vast majority of the block. 

To construct a tree, first we use the bitcoin transactions as leaf nodes by first hashing them, then arranging them in pairs at the base of the tree (this tree is an inverse one). Each pair is hashed together forming another set of pairs with 1/2n the size. If thre is an odd number of transaction hashes, the last hash is duplicated at the end of the set. This process is repeated until a single hash (merkle root is reached). 

![Merkle tree](/assets/merkletree.png)

An important feature of the merkle tree is that you can't change a single byte in the transaction without changing its hash. This subsequently affects the merkle root hash, which changes the overall block hash *and* every single block built on this block! If you remember, the block hash calculation uses the previous block hash as input. This is important as we shall see in the next chapter on bitcoin mining. 

[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
