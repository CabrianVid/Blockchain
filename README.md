# Blockchain
Applications in C# that showes a use of blockchain technology.

Functionalities: block validation, blockchain validation, block creation (mining), and proof-of-work consensus algorithm. The application also has network capabilities.

# Block Structure
Each block in the blockchain consists of:

Index
Data
Timestamp
Block hash
Previous block's hash
(For the first block, the previous block's hash is set to 0.)

# Block and Blockchain Validation
As new blocks are added to the chain, the app checks their integrity by ensuring the following conditions are met:

The current block's index is one greater than the previous block's index.
The hash values are correctly computed.
The current block's 'previousHash' is equal to the previous block's hash.
The calculated hash of the current block is equal to the current block's hash.
These checks ensure that every block added to the chain maintains the integrity of the overall blockchain.

# Mining and Proof-of-Work
Application introduces the concept of mining. Mining in this context means solving a computational problem to add a new block to the chain. Each block contains a nonce and a difficulty value. The difficulty value dictates how many zeros the block's hash must start with for it to be valid.

# Network Difficulty
The difficulty of mining a new block adjusts based on the network's mining speed. The more blocks are mined, the more difficult it becomes to mine new ones. This automatic difficulty adjustment ensures that block generation remains consistent across the network.

# Timestamp Validation and Cumulative Difficulty
Implemented is also timestamp validation rule and cumulative difficulty. A block is valid if its timestamp is not more than a minute ahead of the current time, and not more than a minute behind the timestamp of the previous block. In case of conflicts due to simultaneous mining activities, the valid chain is the one with the highest cumulative difficulty, signifying the highest amount of system resources used and time spent mining.

# Note
Please note that this application is for educational purposes and not intended for real-world financial transactions. LOL
