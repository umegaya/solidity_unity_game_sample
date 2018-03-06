pragma solidity ^0.4.17;

import './libs/math/SafeMath.sol';
import './libs/token/ERC20/ERC20Basic.sol';
import './libs/token/ERC20/ERC20.sol';
import './libs/token/ERC20/BasicToken.sol';
import './libs/token/ERC20/StandardToken.sol';
import './Constants.sol';

contract Moritapo is StandardToken, Constants {

  string public constant name = "Moritapo";
  string public constant symbol = "MRT";

  uint8 public constant decimals = 18;

  /**
   * @dev Constructor that gives msg.sender all existing tokens.
   */
  function Moritapo() public {
    totalSupply_ = MRT_INITIAL_SUPPLY;
    balances[msg.sender] = MRT_INITIAL_SUPPLY;
  }

}
