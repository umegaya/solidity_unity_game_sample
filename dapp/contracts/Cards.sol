pragma solidity ^0.4.24;

import './libs/math/SafeMath.sol';
import "./libs/token/ERC721/ERC721Token.sol";
import './Constants.sol';
import './libs/Restrictable.sol';

contract Cards is ERC721Token, Restrictable, Constants {

  string public constant NAME= "Caravan Heroes Cards";
  string public constant SYMBOL = "CHC";

  constructor() 
    ERC721Token(NAME, SYMBOL) 
    Restrictable() public {
    
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

  function merged(address user, uint target_card_id) public writer {
    //burn merged card
    _burn(user, target_card_id);
  }

  function mint(address user, uint new_card_id) public writer {
    _mint(user, new_card_id);
  }
}
