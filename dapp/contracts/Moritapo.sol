pragma solidity ^0.4.24;

import './libs/math/SafeMath.sol';
import './libs/token/ERC20/ERC20Basic.sol';
import './libs/token/ERC20/ERC20.sol';
import './libs/token/ERC20/BasicToken.sol';
import './libs/token/ERC20/StandardToken.sol';
import './Constants.sol';
import './libs/Restrictable.sol';

//this contract cannot be upgradable easily, so please do not add complex features in this contract.
contract Moritapo is StandardToken, Restrictable, Constants {

  string public constant name = "Moritapo";
  string public constant symbol = "MRT";

  uint8 public constant decimals = 18;

  /**
   * @dev Constructor that gives msg.sender all existing tokens.
   */
  constructor() Restrictable() public {
    totalSupply_ = MRT_INITIAL_SUPPLY;
    balances[administrator_] = MRT_INITIAL_SUPPLY;
  }

  //pay token to receiver. only privileged account can call this
  function privilegedTransfer(address receiver, uint value) public writer returns (bool) {
    emit Approval(administrator_, receiver, value);
    balances[administrator_] = balances[administrator_].sub(value);
    balances[receiver] = balances[receiver].add(value);
    emit Transfer(administrator_, receiver, value);
    return true;
  }
}
