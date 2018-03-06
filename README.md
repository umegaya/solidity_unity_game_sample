## neko
- test block chain game using truffle and Nethereum

### Goal
  - import or automatically create wallet when user launch app first time
  - build private ethereum chain network
    - build some token on it
    - can buy / sell tokens with eth on this private chain
    - can buy cats / sell cats / apply some operation (eg. breed) cats 
  - send token to client wallet for IAP (maybe soon or immediately banned by apple lol)
  - cashout your cat as token (then go somewhere and trade with eth. wow, now we are completely scrooge!!)

### sources
  - solidity
    - Moritapo.sol : erc20 token
    - Inventory.sol : cats ownership mapping
    - World.sol : non-restrictable, all entry of public contract call from users
  - proto
    - Cat.proto : declare cat data
    - Town.proto : declare town data

