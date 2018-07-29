pragma solidity ^0.4.24;

import './libs/Restrictable.sol';
import './libs/StorageAccessor.sol';
import './libs/pb/Issuance_pb.sol';

//this contract cannot be upgradable easily, so please do not add complex features in this contract.
contract Cards is Restrictable {


  constructor() Restrictable() public {
  }

}
