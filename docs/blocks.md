[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Blocks

A block is a data structure used for storing bitcoin transactions and the principle is used in many other cryptocurrencies. In the next chapter we will look at mining and how chaining the blocks together into a 'blockchain' gives it the properties of an immutable ledger. For now, we will just look at how an indivudual block is constructed, and how each block can be linked to the last one.

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

## Block body

The body of each block contains the transactions, stored in hex format. The capacity of a block is capped at 1MB. To get a thourough explanation as to why this is, I recommend this [article]. But essentially, there is a limited storage capacity of transactions. Some argue this is an artificial scarcity, but imagine all node operators (many are simply raspberry pis or cloud servers) have to store and validate every transaction since 2008. The 1MB limit maps to a very real limit on the availablity of hardware to store the transactions. If there was a bigger limit, such as in bitcoin cash (32MB) or sv (a ridiculous amount up to 4GB), it would dramatically effect the cost of running a node, and hence effect who could run the node and the decentralisation of nodes. 

Despite this, it's possible to reduce the number of transactions stored in a block by running a node in 'Pruned' mode. This will remove transactions whose outputs have been spent. Furthermore, it's possible to run a light-client with limited access to storage resources, but can still validate transactions and prove that they have been included in a block. Both these possibilities become feasible with the use of Merkle trees.

## Merkle trees

For the rest of the chapter, we will talk about merkle trees and how they are an important data structure used in blocks. 

Merkle trees provide a way to maintain data integrity inside a block. The data in this case are the bitcoin transactions that make up the vast majority of the block. 

To construct a tree, first we use the bitcoin transactions as leaf nodes by first hashing them, then arranging them in pairs at the base of the tree (this tree is an inverse one). Each pair is hashed together forming another set of pairs with 1/2n the size. If thre is an odd number of transaction hashes, the last hash is duplicated at the end of the set. This process is repeated until a single hash (the merkle root). The merkle root is included in the block header.

![Merkle tree](/assets/merkletree.png)

An important feature of the merkle tree is that you can't change a single byte in the transaction without changing its hash. This subsequently affects the merkle root hash, which changes the overall block hash *and* every single block built on this block! If you remember, the block hash calculation uses the previous block hash as input. This is important as we shall see in the next chapter on bitcoin mining. 

## Parsing a block

Let's parse a block in hex format to extract information:

```c#
BigInteger blockint = BigInteger.Parse("020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d", System.Globalization.NumberStyles.HexNumber);
byte[] blockBytes = blockint.ToByteArray(true, true);
Stream s = new MemoryStream(blockBytes);
Block block = Block.Parse(s);
Console.WriteLine("Version {0}", block.Version);
Console.WriteLine("Merkle root {0}", Byte.bytesToString(block.MerkleRoot));
Console.WriteLine("Previous block hash {0}", Byte.bytesToString(block.PrevBlock));
Console.WriteLine("Block header {0}", Byte.bytesToString(block.GetHeader()));
Console.WriteLine("Block hash {0}", Byte.bytesToString(block.CalculateHash()));
```

The output:

```
Version 536870914
Merkle root 5b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be
Previous block hash 8ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd000000000000000000
Block header 020000208ec39428b17323fa0ddec8e887b4a7c53b8c0a0a220cfd0000000000000000005b0750fce0a889502d40508d39576821155e9c9e3f5c3157f961db38fd8b25be1e77a759e93c0118a4ffd71d
Block hash 2375044d646ad73594dd0b37b113becdb03964584c9e7e000000000000000000
```
**Note: The block hash is reversed due to endianess**

[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
