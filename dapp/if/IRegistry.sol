pragma solidity ^0.4.24;

import "./libs/token/ERC721/ERC721Token.sol";
import "./libs/Restrictable.sol";
import "./libs/if/IAudit.sol";

contract IRegistry is ERC721Token, Restrictable {
  IAudit audit_;

  constructor(string name, string symbol) ERC721Token(name, symbol) Restrictable() public {
  }

  function privilegedTransfer(address from, address to, uint card_id) public writer {
    require(ownerOf(card_id) == from);
    require(to != address(0));
    require(getApproved(card_id) == address(0));
    //force set approval and transfer (sender address)
    tokenApprovals[card_id] = msg.sender; 
    emit Approval(msg.sender, to, card_id);
    //msg.sender retained. (thus, sender address of this contract call)
    super.transferFrom(from, to, card_id);
  }

  function burn(address user, uint target_card_id) public writer {
    _burn(user, target_card_id);
  }

  function mint(address user, uint new_card_id) public writer {
    _mint(user, new_card_id);
  }

}